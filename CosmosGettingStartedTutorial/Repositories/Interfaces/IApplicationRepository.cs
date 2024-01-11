using CosmosGettingStartedTutorial.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CosmosGettingStartedTutorial.Repositories.Interfaces
{
  public interface IApplicationRepository
  {
    Task<Application> CreateApplication(Application app);
    Task<Application> GetApplication(string appId);
    Task<IEnumerable<Application>> GetAllApplicationsByClient(string userId);

    Task<Application> CreateApiKey(string appId, ApiKey apiKey);
    Task<bool> ValidateApiKey(string appId, string apiKey); // for Andrew...
    Task<Application> RegenerateApiKey(string appId, string apiKey);
    Task<Application> UpdateApiLabel(string appId, string apiKey, string label);
    Task RetireApiKey(string appId, string apiKey);
  }
}
