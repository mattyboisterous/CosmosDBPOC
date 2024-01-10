using System;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using System.Linq;

namespace CosmosGettingStartedTutorial
{
  class Program
  {
    // The Azure Cosmos DB endpoint for running this sample.
    private static readonly string EndpointUri = ConfigurationManager.AppSettings["EndPointUri"];

    // The primary key for the Azure Cosmos account.
    private static readonly string PrimaryKey = ConfigurationManager.AppSettings["PrimaryKey"];

    // The Cosmos client instance
    private CosmosClient cosmosClient;

    // The database we will create
    private Database database;

    // The container we will create.
    private Container container;

    // The name of the database and container we will create
    private string databaseId = "DevPortal";
    private string containerId = "Clients";

    public static async Task Main(string[] args)
    {
      try
      {
        Console.WriteLine("Beginning operations...\n");
        Program p = new Program();

        await p.GetStartedDemoAsync();
      }
      catch (CosmosException de)
      {
        Exception baseException = de.GetBaseException();
        Console.WriteLine("{0} error occurred: {1}", de.StatusCode, de);
      }
      catch (Exception e)
      {
        Console.WriteLine("Error: {0}", e);
      }
      finally
      {
        Console.WriteLine("End of demo, press any key to exit.");
        Console.ReadKey();
      }
    }

    public async Task GetStartedDemoAsync()
    {
      Console.Write("Initialising client...");

      // initialise Cosmo client...
      this.cosmosClient = new CosmosClientBuilder(EndpointUri, PrimaryKey)
        .WithSerializerOptions(new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase })
        .Build();

      Console.WriteLine("Done.");
      Console.Write("Initialising db, container...");

      await this.CreateDatabaseAsync();
      await this.CreateContainerAsync();

      Console.WriteLine("Done.");

      WriteOptionsToConsole();

      ConsoleKeyInfo key = Console.ReadKey(true);

      while (key.KeyChar != ' ' && key.KeyChar != 'x' && key.KeyChar != 'X')
      {
        Console.WriteLine();

        switch (key.KeyChar)
        {
          case 'a':
            var clients = await GetAllClients();
            WriteClientsToConsole(clients);
            break;

          case 'b':
            Console.Write("Please enter ObjectId: ");
            var objectId = Console.ReadLine();

            if (!string.IsNullOrEmpty(objectId))
            {
              var client = await GetClient(objectId);
              WriteClientToConsole(client);
            }
            break;

          case 'c':
            Console.Write("Please enter ObjectId: ");
            objectId = Console.ReadLine();

            Console.Write("Please enter a name for the client: ");
            var clientName = Console.ReadLine();

            if (!string.IsNullOrEmpty(objectId) && !string.IsNullOrEmpty(clientName))
            {
              var client = await CreateClient(new Client()
              {
                Id = $"{objectId}.1",
                PartitionKey = objectId,
                ObjectId = objectId,
                Name = clientName,
                ClientKeys = new ClientKey[] { }
              });

              WriteClientToConsole(client);
            }
            break;

          case 'd':
            Console.Write("Please enter ObjectId: ");
            objectId = Console.ReadLine();

            Console.Write("Please enter a name for the new key: ");
            var keyName = Console.ReadLine();

            if (!string.IsNullOrEmpty(objectId) && !string.IsNullOrEmpty(keyName))
            {
              var client = await CreateKey(objectId, new ClientKey()
              {
                Name = keyName,
                Value = Guid.NewGuid().ToString(),
                Scopes = new string[] { }
              });

              WriteClientToConsole(client);
            }
            break;

          case 'e':
            Console.Write("Please enter ObjectId: ");
            objectId = Console.ReadLine();

            Console.Write("Please enter the existing value for the key to be regenerated: ");
            var keyValue = Console.ReadLine();

            if (!string.IsNullOrEmpty(objectId) && !string.IsNullOrEmpty(keyValue))
            {
              var client = await RegenerateKey(objectId, new ClientKey()
              {
                Value = keyValue
              });

              WriteClientToConsole(client);
            }
            break;

          case 'f':
            Console.Write("Please enter ObjectId: ");
            objectId = Console.ReadLine();

            Console.Write("Please enter the key value: ");
            keyValue = Console.ReadLine();

            Console.Write("Please enter the new key label: ");
            var keyLabel = Console.ReadLine();

            if (!string.IsNullOrEmpty(objectId) && !string.IsNullOrEmpty(keyValue) && !string.IsNullOrEmpty(keyLabel))
            {
              var client = await UpdateKey(objectId, new ClientKey()
              {
                Value = keyValue,
                Name = keyLabel
              });

              WriteClientToConsole(client);
            }
            break;

          case 'g':
            Console.Write("Please enter ObjectId: ");
            objectId = Console.ReadLine();

            Console.Write("Please enter the key value: ");
            keyValue = Console.ReadLine();

            if (!string.IsNullOrEmpty(objectId) && !string.IsNullOrEmpty(keyValue))
            {
              var client = await DeleteKey(objectId, new ClientKey()
              {
                Value = keyValue
              });

              WriteClientToConsole(client);
            }
            break;
        }

        WriteOptionsToConsole();

        key = Console.ReadKey(true);
      }

      ExitDemo();
    }

    private void WriteOptionsToConsole()
    {
      Console.WriteLine();
      Console.WriteLine("OPTIONS:");
      Console.WriteLine();
      Console.WriteLine("a: List all clients");
      Console.WriteLine("b: Get client by Object/UserId");
      Console.WriteLine("c: Create client");
      Console.WriteLine("d: Create Api key");
      Console.WriteLine("e: Regenerate Api key");
      Console.WriteLine("f: Update Api key label");
      Console.WriteLine("g: Delete (retire) Api key");

      Console.WriteLine("x or SPACE: Exit");
    }

    private void ExitDemo()
    {
      Console.WriteLine("Demo exiting...");

      // dipose of the Cosmo client...
      this.cosmosClient.Dispose();

      Environment.Exit(0);
    }

    private async Task CreateDatabaseAsync()
    {
      // Create a new database
      this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);

      //Console.WriteLine("Created Database: {0}\n", this.database.Id);
    }

    private async Task CreateContainerAsync()
    {
      // Create a new container
      this.container = await this.database.CreateContainerIfNotExistsAsync(containerId, "/partitionKey");

      //Console.WriteLine("Created Container: {0}\n", this.container.Id);
    }

    private async Task<bool> ClientExists(string objectId)
    {
      var sqlQueryText = $"SELECT * FROM c WHERE c.objectId = '{objectId}'";

      Console.WriteLine("Running query: {0}\n", sqlQueryText);

      QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
      FeedIterator<Client> queryResultSetIterator = this.container.GetItemQueryIterator<Client>(queryDefinition);

      if (queryResultSetIterator.HasMoreResults)
      {
        FeedResponse<Client> currentResultSet = await queryResultSetIterator.ReadNextAsync();

        return currentResultSet.Count > 0;
      }

      return false;
    }

    private async Task<List<Client>> GetAllClients()
    {
      var sqlQueryText = $"SELECT * FROM c";
      Console.WriteLine("Running query: {0}\n", sqlQueryText);

      QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
      FeedIterator<Client> queryResultSetIterator = this.container.GetItemQueryIterator<Client>(queryDefinition);

      List<Client> clientList = new List<Client>();

      if (queryResultSetIterator.HasMoreResults)
      {
        FeedResponse<Client> currentResultSet = await queryResultSetIterator.ReadNextAsync();

        foreach (var c in currentResultSet)
        {
          clientList.Add(c);
        }
      }

      return clientList;
    }

    private async Task<Client> GetClient(string objectId)
    {
      var sqlQueryText = $"SELECT * FROM c WHERE c.objectId = '{objectId}'";
      Console.WriteLine("Running query: {0}\n", sqlQueryText);

      QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
      FeedIterator<Client> queryResultSetIterator = this.container.GetItemQueryIterator<Client>(queryDefinition);

      if (queryResultSetIterator.HasMoreResults)
      {
        FeedResponse<Client> currentResultSet = await queryResultSetIterator.ReadNextAsync();

        return currentResultSet.FirstOrDefault();
      }

      return null;
    }

    private async Task<Client> CreateClient(Client client)
    {
      try
      {
        ItemResponse<Client> dpClientResponse = await this.container.ReadItemAsync<Client>(client.Id, new PartitionKey(client.PartitionKey));
        Console.WriteLine("Item in database with id: {0} already exists\n", dpClientResponse.Resource.Id);

        return dpClientResponse.Resource;
      }
      catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
      {
        ItemResponse<Client> dpClientResponse = await this.container.CreateItemAsync(client, new PartitionKey(client.PartitionKey), new ItemRequestOptions());

        // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
        Console.WriteLine("Created client in database with id: {0} Operation consumed {1} RUs.\n", dpClientResponse.Resource.Id, dpClientResponse.RequestCharge);

        return dpClientResponse.Resource;
      }
    }

    private async Task<Client> CreateKey(string objectId, ClientKey key)
    {
      var client = await GetClient(objectId);

      // determine if we have a client and if the key already exists...
      if (client != null && !client.ClientKeys.Any(ck => ck.Value == key.Value))
      {
        // add the new key to the collection and persist...
        DateTime now = DateTime.UtcNow;
        key.Expiry = null;
        key.Created = now;
        key.Updated = now;

        client.ClientKeys = client.ClientKeys.Append(key).ToArray();

        // replace the item with the updated content...
        var clientResponse = await this.container.ReplaceItemAsync(client, client.Id, new PartitionKey(client.PartitionKey));
        Console.WriteLine("Updated Client [{0},{1}].\n \tBody is now: {2}\n", client.Name, client.Id, clientResponse.Resource);

        return clientResponse.Resource;
      }
      else
        return null;
    }

    private async Task<Client> RegenerateKey(string objectId, ClientKey key)
    {
      var client = await GetClient(objectId);

      // determine if we have a client and if the key already exists...
      if (client != null && client.ClientKeys.Any(ck => ck.Value == key.Value))
      {
        // update the appropriate key and persist...
        var index = client.ClientKeys.TakeWhile(ck => ck.Value != key.Value).Count();

        if (index >= 0)
        {
          client.ClientKeys[index].Expiry = null;
          client.ClientKeys[index].Value = Guid.NewGuid().ToString();
          client.ClientKeys[index].Updated = DateTime.UtcNow;
        }

        // replace the item with the updated content...
        var clientResponse = await this.container.ReplaceItemAsync(client, client.Id, new PartitionKey(client.PartitionKey));
        Console.WriteLine("Updated Client [{0},{1}].\n \tBody is now: {2}\n", client.Name, client.Id, clientResponse.Resource);

        return clientResponse.Resource;
      }
      else
        return null;
    }

    private async Task<Client> UpdateKey(string objectId, ClientKey key)
    {
      var client = await GetClient(objectId);

      // determine if we have a client and if the key already exists...
      if (client != null && client.ClientKeys.Any(ck => ck.Value == key.Value))
      {
        // update the appropriate key and persist...
        var keyRef = client.ClientKeys.First(ck => ck.Value == key.Value);
        keyRef.Name = key.Name;
        keyRef.Updated = DateTime.UtcNow;

        // replace the item with the updated content...
        var clientResponse = await this.container.ReplaceItemAsync(client, client.Id, new PartitionKey(client.PartitionKey));
        Console.WriteLine("Updated Client [{0},{1}].\n \tBody is now: {2}\n", client.Name, client.Id, clientResponse.Resource);

        return clientResponse.Resource;
      }
      else
        return null;
    }

    private async Task<Client> DeleteKey(string objectId, ClientKey key)
    {
      var client = await GetClient(objectId);

      // determine if we have a client and if the key already exists...
      if (client != null && client.ClientKeys.Any(ck => ck.Value == key.Value))
      {
        // update the appropriate key and persist...
        DateTime now = DateTime.UtcNow;

        var keyRef = client.ClientKeys.First(ck => ck.Value == key.Value);
        keyRef.Expiry = now;
        keyRef.Updated = now;

        // replace the item with the updated content...
        var clientResponse = await this.container.ReplaceItemAsync(client, client.Id, new PartitionKey(client.PartitionKey));
        Console.WriteLine("Updated Client [{0},{1}].\n \tBody is now: {2}\n", client.Name, client.Id, clientResponse.Resource);

        return clientResponse.Resource;
      }
      else
        return null;
    }
    private void WriteClientsToConsole(List<Client> clients)
    {
      foreach (var c in clients)
      {
        WriteClientToConsole(c);
      }
    }

    private void WriteClientToConsole(Client client)
    {
      Console.WriteLine();
      Console.WriteLine($"{client}");
    }
  }
}