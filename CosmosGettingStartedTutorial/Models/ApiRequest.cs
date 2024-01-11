using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosGettingStartedTutorial.Models
{
  public class ApiRequest
  {
    public string Id { get; set; } // {Guid}...
    public string PartitionKey { get; set; }  // AppId...large cardinality...
    public DateTime RequestUtcDateTime { get; set; }
    public string AppId { get; set; }
    public string ApiName { get; set; }
    public string HttpVerb { get; set; }
    public string WebMethod { get; set; }
    public string SubHeader { get; set; }
    public string ApiVersion { get; set; }
    public bool Success { get; set; }
    public int ResponseTime { get; set; }
  }
}

//"id": "701158fd-548e-4837-b879-679e4604577e",
//"partitionKey": "3503ff40-d9c5-47b0-b59f-941197893126",
//"timestamp": "2023-11-14T05:17:14.4278355Z",
//"apiName": "Parent-API",
//"httpVerb": "Get",
//"webMethod": "/parent/{student_eqid}/{index}",
//"webMethodSubHeader": "View parent by student eqid",
//"apiVersion": "1.0",
//"success": true,
//"responseTime": 845
