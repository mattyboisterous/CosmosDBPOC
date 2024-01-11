using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosGettingStartedTutorial.Models
{
  public class ApiKey
  {
    public string Name { get; set; }
    public string Value { get; set; }
    public string[] Scopes { get; set; }
    public DateTime? Expiry { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
  }
}
