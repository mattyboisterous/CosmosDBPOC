using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace CosmosGettingStartedTutorial.Repositories.Base.Interfaces
{
  public interface IRepositoryBase : IDisposable
  {
    string EndpointUri { get; set; }
    string PrimaryKey { get; set; }
    
    CosmosClient CosmosClient { get; set; }
    Database CosmosDataBase { get; set; }
    Container CosmosContainer { get; set; }

    string DatabaseId { get; }
    string ContainerId { get; set; }

    Task Initialise();
  }
}
