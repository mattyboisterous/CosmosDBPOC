﻿using CosmosGettingStartedTutorial.Models;
using CosmosGettingStartedTutorial.Repositories.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CosmosGettingStartedTutorial.Repositories.Interfaces
{
  public interface IApiRequestRepository : IRepositoryBase
  {
    Task<ApiRequest> CreateApiRequest(ApiRequest request);
    Task<IEnumerable<ApiRequest>> GetAllApiRequests();
    Task<IEnumerable<ApiRequest>> GetAllApiRequestsByApplication(string appId);
    Task<IEnumerable<ApiRequestSummary>> GetAllApiRequestSummariesByApplication(string appId, TimeRange reportingRange);
  }
}
