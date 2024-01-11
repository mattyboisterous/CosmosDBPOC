using CosmosGettingStartedTutorial.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CosmosGettingStartedTutorial.Repositories.Interfaces
{
  public interface IApiRequestRepository
  {
    Task<ApiRequest> CreateApiRequest(ApiRequest request);
  }
}
