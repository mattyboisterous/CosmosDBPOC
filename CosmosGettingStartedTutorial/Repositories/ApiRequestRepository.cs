using CosmosGettingStartedTutorial.Models;
using CosmosGettingStartedTutorial.Repositories.Base;
using CosmosGettingStartedTutorial.Repositories.Interfaces;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CosmosGettingStartedTutorial.Repositories
{
  public class ApiRequestRepository : RepositoryBase, IApiRequestRepository
  {
    public ApiRequestRepository(string endpointUri, string primaryKey, string containerId)
      : base(endpointUri, primaryKey, containerId)
    { }

    public async Task<ApiRequest> CreateApiRequest(ApiRequest request)
    {
      try
      {
        ItemResponse<ApiRequest> dpApiRequestResponse = await CosmosContainer.ReadItemAsync<ApiRequest>(request.Id, new PartitionKey(request.PartitionKey));

        return dpApiRequestResponse.Resource;
      }
      catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
      {
        ItemResponse<ApiRequest> dpApiRequestResponse = await this.CosmosContainer.CreateItemAsync(request, new PartitionKey(request.PartitionKey), new ItemRequestOptions());

        // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
        return dpApiRequestResponse.Resource;
      }
    }

    public async Task<IEnumerable<ApiRequest>> GetAllApiRequests()
    {
      var sqlQueryText = $"SELECT * FROM req";

      QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
      FeedIterator<ApiRequest> queryResultSetIterator = CosmosContainer.GetItemQueryIterator<ApiRequest>(queryDefinition);

      List<ApiRequest> requestList = new List<ApiRequest>();

      if (queryResultSetIterator.HasMoreResults)
      {
        FeedResponse<ApiRequest> currentResultSet = await queryResultSetIterator.ReadNextAsync();

        foreach (var r in currentResultSet)
        {
          requestList.Add(r);
        }
      }

      return requestList;
    }

    public async Task<IEnumerable<ApiRequest>> GetAllApiRequestsByApplication(string appId)
    {
      var sqlQueryText = $"SELECT * FROM req WHERE req.appId = '{appId}'";

      QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
      FeedIterator<ApiRequest> queryResultSetIterator = CosmosContainer.GetItemQueryIterator<ApiRequest>(queryDefinition);

      List<ApiRequest> requestList = new List<ApiRequest>();

      if (queryResultSetIterator.HasMoreResults)
      {
        FeedResponse<ApiRequest> currentResultSet = await queryResultSetIterator.ReadNextAsync();

        foreach (var r in currentResultSet)
        {
          requestList.Add(r);
        }
      }

      return requestList;
    }
  }
}
