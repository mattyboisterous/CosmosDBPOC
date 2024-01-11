using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosGettingStartedTutorial.Models
{
  public class Application
  {
    public string Id { get; set; } // {AppId}.1...
    public string PartitionKey { get; set; }  // AppId...large cardinality...
    public string UserId { get; set; }
    public string AppId { get; set; }
    public string AppName { get; set; }
    public ApiKey[] ApiKeys { get; set; }

    public override string ToString()
    {
      return JsonConvert.SerializeObject(this, new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
    }
  }

  //{
  //  "id": "3503ff40-d9c5-47b0-b59f-941197893126.1",
  //  "partitionKey": "3503ff40-d9c5-47b0-b59f-941197893126",
  //  "userId": "2a7ae06c-da16-49ca-ac8b-a6e8bfa4511a",
  //  "appKey": "3503ff40-d9c5-47b0-b59f-941197893126",
  //  "appName": "QTeachers",
  //  "apiKeys": [
  //    {
  //    "name": "O/S actions",
  //      "value": "841e31aa-7233-4da0-9cc4-835411235e64",
  //      "scopes": [],
  //      "expiry": null,
  //      "created": "2023-11-14T05:17:14.4278355Z",
  //      "updated": "2023-11-14T05:17:14.4278355Z"
  //    },
  //    {
  //    "name": "Sample data",
  //      "value": "14ac6280-2cf1-426d-8d32-f25b4b5969d1",
  //      "scopes": [],
  //      "expiry": null,
  //      "created": "2023-11-14T05:17:33.0058179Z",
  //      "updated": "2023-11-14T21:49:26.9681572Z"
  //    }
  //  ]
  //}
}
