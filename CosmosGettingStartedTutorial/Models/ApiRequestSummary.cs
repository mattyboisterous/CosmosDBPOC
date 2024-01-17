using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosGettingStartedTutorial.Models
{
  public class ApiRequestSummary
  {
    public string AppId { get; set; }
    public string ApiName { get; set; }
    public string ApiVersion { get; set; }
    public string HttpVerb { get; set; }
    public string WebMethod { get; set; }
    public string SubHeader { get; set; }
    public int Requests { get; set; }

    public override string ToString()
    {
      return JsonConvert.SerializeObject(this, new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
    }
  }
}
