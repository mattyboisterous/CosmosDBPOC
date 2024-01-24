using CosmosGettingStartedTutorial.Models;
using CosmosGettingStartedTutorial.Repositories.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CosmosGettingStartedTutorial.Repositories.Interfaces
{
  public interface IApplicationRepository : IRepositoryBase
  {
    Task<Application> CreateApplication(Application app);
    Task<Application> GetApplication(string appId);
    Task<IEnumerable<Application>> GetAllApplications();
    Task<IEnumerable<Application>> GetAllApplicationsByUser(string userId);
    Task<Application> UpdateApplicationLabel(string appId, string label);
    Task RetireApplication(string appId);

    Task<bool> ValidateApiKey(string apiKey);
    Task<Application> RegenerateApiKey(string appId);
  }
}
