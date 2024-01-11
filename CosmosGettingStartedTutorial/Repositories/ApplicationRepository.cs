using CosmosGettingStartedTutorial.Models;
using CosmosGettingStartedTutorial.Repositories.Base;
using CosmosGettingStartedTutorial.Repositories.Interfaces;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CosmosGettingStartedTutorial.Repositories
{
  public class ApplicationRepository : RepositoryBase, IApplicationRepository
  {
    public ApplicationRepository(string endpointUri, string primaryKey, string containerId)
      : base(endpointUri, primaryKey, containerId)
    { }

    public async Task<Application> CreateApplication(Application app)
    {
      try
      {
        ItemResponse<Application> dpApplicationResponse = await CosmosContainer.ReadItemAsync<Application>(app.Id, new PartitionKey(app.PartitionKey));

        return dpApplicationResponse.Resource;
      }
      catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
      {
        ItemResponse<Application> dpApplicationResponse = await this.CosmosContainer.CreateItemAsync(app, new PartitionKey(app.PartitionKey), new ItemRequestOptions());

        // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
        return dpApplicationResponse.Resource;
      }
    }

    public async Task<IEnumerable<Application>> GetAllApplicationsByUser(string userId)
    {
      var sqlQueryText = $"SELECT * FROM app WHERE app.userId = '{userId}'";

      QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
      FeedIterator<Application> queryResultSetIterator = CosmosContainer.GetItemQueryIterator<Application>(queryDefinition);

      List<Application> appList = new List<Application>();

      if (queryResultSetIterator.HasMoreResults)
      {
        FeedResponse<Application> currentResultSet = await queryResultSetIterator.ReadNextAsync();

        foreach (var c in currentResultSet)
        {
          appList.Add(c);
        }
      }

      return appList;
    }

    public async Task<Application> GetApplication(string appId)
    {
      var sqlQueryText = $"SELECT * FROM app WHERE app.appId = '{appId}'";
      Console.WriteLine("Running query: {0}\n", sqlQueryText);

      QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
      FeedIterator<Application> queryResultSetIterator = CosmosContainer.GetItemQueryIterator<Application>(queryDefinition);

      if (queryResultSetIterator.HasMoreResults)
      {
        FeedResponse<Application> currentResultSet = await queryResultSetIterator.ReadNextAsync();

        return currentResultSet.FirstOrDefault();
      }

      return null;
    }

    public async Task<Application> CreateApiKey(string appId, ApiKey apiKey)
    {
      // locate this application...
      var application = await GetApplication(appId);

      if (application != null && application.ApiKeys != null && !application.ApiKeys.Any(a => a.Value == apiKey.Value))
      {
        var keys = new List<ApiKey>(application.ApiKeys);
        keys.Add(apiKey);

        application.ApiKeys = keys.ToArray();

        var appResponse = await CosmosContainer.ReplaceItemAsync(application, application.Id, new PartitionKey(application.PartitionKey));

        return appResponse.Resource;
      }

      return null;
    }

    public async Task<Application> RegenerateApiKey(string appId, string apiKey)
    {
      // locate this application...
      var application = await GetApplication(appId);

      // determine if we have a app and if the key already exists...
      if (application != null && application.ApiKeys.Any(ak => ak.Value == apiKey))
      {
        // update the appropriate key and persist...
        var index = application.ApiKeys.TakeWhile(ak => ak.Value != apiKey).Count();

        if (index >= 0)
        {
          application.ApiKeys[index].Expiry = null;
          application.ApiKeys[index].Value = Guid.NewGuid().ToString();
          application.ApiKeys[index].Updated = DateTime.UtcNow;
        }

        // replace the item with the updated content...
        var appResponse = await CosmosContainer.ReplaceItemAsync(application, application.Id, new PartitionKey(application.PartitionKey));

        return appResponse.Resource;
      }
      else
        return null;
    }

    public async Task RetireApiKey(string appId, string apiKey)
    {
      // locate this application...
      var application = await GetApplication(appId);

      // determine if we have a app and if the key already exists...
      if (application != null && application.ApiKeys.Any(ak => ak.Value == apiKey))
      {
        // update the appropriate key and persist...
        DateTime now = DateTime.UtcNow;

        var keyRef = application.ApiKeys.First(ck => ck.Value == apiKey);
        keyRef.Expiry = now;
        keyRef.Updated = now;

        // replace the item with the updated content...
        await CosmosContainer.ReplaceItemAsync(application, application.Id, new PartitionKey(application.PartitionKey));
      }
    }

    public async Task<Application> UpdateApiLabel(string appId, string apiKey, string label)
    {
      // locate this application...
      var application = await GetApplication(appId);

      // determine if we have a app and if the key already exists...
      if (application != null && application.ApiKeys.Any(ak => ak.Value == apiKey))
      {
        // update the appropriate key and persist...
        DateTime now = DateTime.UtcNow;

        var keyRef = application.ApiKeys.First(ck => ck.Value == apiKey);
        keyRef.Name = label;

        // replace the item with the updated content...
        var appResponse = await CosmosContainer.ReplaceItemAsync(application, application.Id, new PartitionKey(application.PartitionKey));

        return appResponse.Resource;
      }
      else
        return null;
    }

    public async Task<bool> ValidateApiKey(string appId, string apiKey)
    {
      // locate this application...
      var application = await GetApplication(appId);

      // determine if we have a app and if the key exists...
      return (application != null && application.ApiKeys.Any(ak => ak.Value == apiKey));
    }
  }
}
