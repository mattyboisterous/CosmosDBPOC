using CosmosGettingStartedTutorial.Models;
using CosmosGettingStartedTutorial.Repositories.Base;
using CosmosGettingStartedTutorial.Repositories.Interfaces;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CosmosGettingStartedTutorial.Repositories
{
  public class ApplicationRepository : RepositoryBase, IApplicationRepository
  {
    public ApplicationRepository(string endpointUri, string primaryKey)
      : base(endpointUri, primaryKey, "Application")
    { }

    public ApplicationRepository(string endpointUri, string primaryKey, string containerId)
      : base(endpointUri, primaryKey, containerId)
    { }


    //Task<bool> ValidateApiKey(string apiKey);
    //Task<Application> RegenerateApiKey(string appId);


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

    public async Task<IEnumerable<Application>> GetAllApplications()
    {
      var sqlQueryText = $"SELECT * FROM app";

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

    public async Task<Application> UpdateApplicationLabel(string appId, string label)
    {
      // locate this application...
      var application = await GetApplication(appId);

      // determine if we have a app...
      if (application != null)
      {
        // update and persist...
        application.AppName = label;
        application.Updated = DateTime.UtcNow;

        // replace the item with the updated content...
        var appResponse = await CosmosContainer.ReplaceItemAsync(application, application.Id, new PartitionKey(application.PartitionKey));

        return appResponse.Resource;
      }
      else
        return null;
    }

    public async Task RetireApplication(string appId)
    {
      // locate this application...
      var application = await GetApplication(appId);

      // determine if we have a app and if the key already exists...
      if (application != null)
      {
        // update the appropriate key and persist...
        DateTime now = DateTime.UtcNow;
        application.Expiry = now;
        application.Updated = now;

        // replace the item with the updated content...
        await CosmosContainer.ReplaceItemAsync(application, application.Id, new PartitionKey(application.PartitionKey));
      }
    }

    public async Task<bool> ValidateApiKey(string apiKey)
    {
      //// locate this application...
      //var application = await GetApplication(appId);

      //// determine if we have a app and if the key exists...
      //return (application != null && application.KeyValue == apiKey);
      return true;
    }

    public async Task<Application> RegenerateApiKey(string appId)
    {
      // locate this application...
      var application = await GetApplication(appId);

      // determine if we have a app and if the key already exists...
      if (application != null)
      {
        // update and persist...
        application.Expiry = null;
        application.KeyValue = Guid.NewGuid().ToString();
        application.Updated = DateTime.UtcNow;

        // replace the item with the updated content...
        var appResponse = await CosmosContainer.ReplaceItemAsync(application, application.Id, new PartitionKey(application.PartitionKey));

        return appResponse.Resource;
      }
      else
        return null;
    }
  }
}
