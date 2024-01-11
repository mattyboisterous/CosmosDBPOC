using CosmosGettingStartedTutorial;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosGettingStartedTutorial
{
  public class Client
  {
    public string Id { get; set; } // {ObjectId}.1...
    public string PartitionKey { get; set; }  // ObjectId...large cardinality...
    public string ObjectId { get; set; }
    public string Name { get; set; }
    public ClientKey[] ClientKeys { get; set; }

    public override string ToString()
    {
      return JsonConvert.SerializeObject(this, new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
    }
  }

  public class ClientKey
  {
    public string Name { get; set; }
    public string Value { get; set; }
    public string[] Scopes { get; set; }
    public DateTime? Expiry { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
  }
}
