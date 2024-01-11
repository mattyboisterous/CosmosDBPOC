using CosmosGettingStartedTutorial.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosGettingStartedTutorial.Repositories.Interfaces
{
  public interface IApplicationRepository
  {
    Application GetApplication(string appId);
    IEnumerable<Application> GetAllApplicationsByClient(string userId);
  }

  public interface IApiKeyRepository
  {
    ApiKey CreateApiKey(ApiKey apiKey);
    bool ValidateApiKey(string appKey, string apiKey); // for Andrew...
    ApiKey RegenerateApiKey(string apiKey);
    ApiKey UpdateApiLabel(string apiKey, string label);
    void RetireApiKey(string apiKey);
  }

  public interface IApiRequestRepository
  {
    ApiRequest CreateApiRequest(ApiRequest request);
  }
}
