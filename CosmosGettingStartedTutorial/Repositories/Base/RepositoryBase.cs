using CosmosGettingStartedTutorial.Repositories.Base.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CosmosGettingStartedTutorial.Repositories.Base
{
  public class RepositoryBase : IRepositoryBase
  {
    public string EndpointUri { get; set; }
    public string PrimaryKey { get; set; }

    public CosmosClient CosmosClient { get; set; }
    public Database CosmosDataBase { get; set; }
    public Container CosmosContainer { get; set; }

    public string DatabaseId => "DevPortal";
    public string ContainerId { get; set; }

    public RepositoryBase()
    { }

    public RepositoryBase(string endpointUri, string primaryKey)
    {
      EndpointUri = endpointUri;
      PrimaryKey = primaryKey;
    }

    public RepositoryBase(string endpointUri, string primaryKey, string containerId)
    {
      EndpointUri = endpointUri;
      PrimaryKey = primaryKey;
      ContainerId = containerId;
    }

    public async Task Initialise()
    {
      if (string.IsNullOrEmpty(EndpointUri) || string.IsNullOrEmpty(PrimaryKey))
        throw new InvalidOperationException("Ensure both 'EndpointUri' and 'PrimaryKey' have been assigned before initialising this repository.");

      if (string.IsNullOrEmpty(ContainerId))
        throw new InvalidOperationException("Ensure 'ContainerId' has been provided before initialising this repository.");

      // initialise Cosmo client...
      CosmosClient = new CosmosClientBuilder(EndpointUri, PrimaryKey)
        .WithSerializerOptions(new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase })
        .Build();

      await this.CreateDatabaseAsync();
      await this.CreateContainerAsync();
    }

    private async Task CreateDatabaseAsync()
    {
      // create database if it does not already exist...
      CosmosDataBase = await CosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseId);
    }

    private async Task CreateContainerAsync()
    {
      // create this container if it does not alreay exist...
      CosmosContainer = await CosmosDataBase.CreateContainerIfNotExistsAsync(ContainerId, "/partitionKey");
    }
  }
}
