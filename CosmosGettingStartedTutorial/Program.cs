using System;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using System.Linq;
using CosmosGettingStartedTutorial.Repositories.Interfaces;
using CosmosGettingStartedTutorial.Repositories;
using CosmosGettingStartedTutorial.Models;
using Azure.Core;

namespace CosmosGettingStartedTutorial
{
  class Program
  {
    // The Azure Cosmos DB endpoint for running this sample.
    private static readonly string EndpointUri = ConfigurationManager.AppSettings["EndPointUri"];

    // The primary key for the Azure Cosmos account.
    private static readonly string PrimaryKey = ConfigurationManager.AppSettings["PrimaryKey"];

    private IApplicationRepository ApplicationRepository { get; set; }
    private IApiRequestRepository ApiRequestRepository { get; set; }

    public static async Task Main(string[] args)
    {
      try
      {
        Console.WriteLine("Beginning operations...\n");
        Program p = new Program();

        await p.GetStartedDemoAsync();
      }
      catch (CosmosException de)
      {
        Exception baseException = de.GetBaseException();
        Console.WriteLine("{0} error occurred: {1}", de.StatusCode, de);
      }
      catch (Exception e)
      {
        Console.WriteLine("Error: {0}", e);
      }
      finally
      {
        Console.WriteLine("End of demo, press any key to exit.");
        Console.ReadKey();
      }
    }

    private void WriteOptionsToConsole()
    {
      Console.WriteLine();
      Console.WriteLine("OPTIONS:");
      Console.WriteLine();
      Console.WriteLine("a: Get all Applications");
      Console.WriteLine("b: Get Applications by userId");
      Console.WriteLine("c: Get Application by appId");
      Console.WriteLine("d: Create application");
      Console.WriteLine("e: Create Api key");
      Console.WriteLine("f: Regenerate Api key");
      Console.WriteLine("f: Update Api key label");
      Console.WriteLine("h: Delete (retire) Api key");
      Console.WriteLine("i: Get all Api Requests");
      Console.WriteLine("j: Get all Api Requests by appId");
      Console.WriteLine("k: Get all Api Request summaries by appId");
      Console.WriteLine("l: Create Api Request");


      Console.WriteLine("x or SPACE: Exit");
    }

    private async Task GetStartedDemoAsync()
    {
      Console.Write("Initialising repositories...");

      ApplicationRepository = new ApplicationRepository(EndpointUri, PrimaryKey);
      ApiRequestRepository = new ApiRequestRepository(EndpointUri, PrimaryKey);

      Console.WriteLine("Done.");
      Console.Write("Initialising dbs, containers...");
      await ApplicationRepository.Initialise();
      await ApiRequestRepository.Initialise();

      Console.WriteLine("Done.");

      WriteOptionsToConsole();

      ConsoleKeyInfo key = Console.ReadKey(true);

      while (key.KeyChar != ' ' && key.KeyChar != 'x' && key.KeyChar != 'X')
      {
        Console.WriteLine();

        switch (key.KeyChar)
        {
          case 'a':
            var applications = await ApplicationRepository.GetAllApplications();
            WriteApplicationsToConsole(applications.ToList());
            break;

          case 'b':
            Console.Write("Please enter UserId: ");
            var userId = Console.ReadLine();

            applications = await ApplicationRepository.GetAllApplicationsByUser(userId);
            WriteApplicationsToConsole(applications.ToList());
            break;

          case 'c':
            Console.Write("Please enter AppId: ");
            var appId = Console.ReadLine();

            if (!string.IsNullOrEmpty(appId))
            {
              var app = await ApplicationRepository.GetApplication(appId);
              WriteApplicationToConsole(app);
            }
            break;

          case 'd':
            Console.Write("Please enter UserId: ");
            userId = Console.ReadLine();
            Console.Write("Please enter AppId: ");
            appId = Console.ReadLine();
            Console.Write("Please enter a name for the application: ");
            var appName = Console.ReadLine();

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(appId) && !string.IsNullOrEmpty(appName))
            {
              var app = await ApplicationRepository.CreateApplication(new Application()
              {
                Id = $"{appId}.1",
                PartitionKey = appId,
                UserId = userId,
                AppId = appId,
                AppName = appName,
                ApiKeys = new ApiKey[] { }
              });

              WriteApplicationToConsole(app);
            }
            break;

          case 'e':
            Console.Write("Please enter AppId: ");
            appId = Console.ReadLine();

            Console.Write("Please enter a name for the new key: ");
            var keyName = Console.ReadLine();

            var now = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(appId) && !string.IsNullOrEmpty(keyName))
            {
              var app = await ApplicationRepository.CreateApiKey(appId, new ApiKey()
              {
                Name = keyName,
                Scopes = [],
                Value = Guid.NewGuid().ToString(),
                Created = now,
                Updated = now
              });

              WriteApplicationToConsole(app);
            }
            break;

          case 'f':
            Console.Write("Please enter AppId: ");
            appId = Console.ReadLine();

            Console.Write("Please enter the existing value for the key to be regenerated: ");
            var apiKey = Console.ReadLine();

            if (!string.IsNullOrEmpty(appId) && !string.IsNullOrEmpty(apiKey))
            {
              var app = await ApplicationRepository.RegenerateApiKey(appId, apiKey);

              WriteApplicationToConsole(app);
            }
            break;

          case 'g':
            Console.Write("Please enter AppId: ");
            appId = Console.ReadLine();
            Console.Write("Please enter ApiKey: ");
            apiKey = Console.ReadLine();
            Console.Write("Please enter a new label name for the api key: ");
            var keyLabel = Console.ReadLine();

            if (!string.IsNullOrEmpty(appId) && !string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(keyLabel))
            {
              var app = await ApplicationRepository.UpdateApiLabel(appId, apiKey, keyLabel);

              WriteApplicationToConsole(app);
            }
            break;

          case 'h':
            Console.Write("Please enter AppId: ");
            appId = Console.ReadLine();
            Console.Write("Please enter ApiKey: ");
            apiKey = Console.ReadLine();

            if (!string.IsNullOrEmpty(appId) && !string.IsNullOrEmpty(apiKey))
            {
              await ApplicationRepository.RetireApiKey(appId, apiKey);
            }
            break;

          case 'i':
            var requests = await ApiRequestRepository.GetAllApiRequests();

            WriteApiRequestsToConsole(requests.ToList());
            break;

          case 'j':
            Console.Write("Please enter AppId: ");
            appId = Console.ReadLine();

            requests = await ApiRequestRepository.GetAllApiRequestsByApplication(appId);

            WriteApiRequestsToConsole(requests.ToList());
            break;

          case 'k':
            Console.Write("Please enter AppId: ");
            appId = Console.ReadLine();

            Console.Write("Please enter reporting period: 0 -> Last Hour, 1 -> Today, 2 -> Last two weeks, 3 -> Last month");
            var period = Console.ReadLine();

            var requestSummaries = await ApiRequestRepository.GetAllApiRequestSummariesByApplication(appId, (TimeRange)int.Parse(period));

            WriteApiRequestSummariesToConsole(requestSummaries.ToList());
            break;

          case 'l':
            Console.Write("Please enter AppId: ");
            appId = Console.ReadLine();

            if (!string.IsNullOrEmpty(appId))
            {
              var request = await ApiRequestRepository.CreateApiRequest(new ApiRequest()
              {
                Id = Guid.NewGuid().ToString(),
                PartitionKey = appId,
                RequestUtcDateTime = DateTime.UtcNow,
                AppId = appId,
                ApiName = "",
                HttpVerb = "GET",
                WebMethod = "",
                SubHeader = "",
                ApiVersion = "1.0",
                Success = true,
                ResponseTime = 983
              });

              WriteApiRequestToConsole(request);
            }
            break;
        }

        WriteOptionsToConsole();

        key = Console.ReadKey(true);
      }

      ExitDemo();
    }

    private void ExitDemo()
    {
      Console.WriteLine("Demo exiting...");

      // dispose of the Cosmos clients...
      ApplicationRepository.Dispose();
      ApiRequestRepository.Dispose();

      Environment.Exit(0);
    }

    private void WriteApplicationsToConsole(List<Application> apps)
    {
      foreach (var a in apps)
      {
        WriteApplicationToConsole(a);
      }
    }

    private void WriteApplicationToConsole(Application app)
    {
      Console.WriteLine();
      Console.WriteLine($"{app}");
    }

    private void WriteApiRequestsToConsole(List<ApiRequest> requests)
    {
      foreach (var r in requests)
      {
        WriteApiRequestToConsole(r);
      }
    }

    private void WriteApiRequestToConsole(ApiRequest request)
    {
      Console.WriteLine();
      Console.WriteLine($"{request}");
    }

    private void WriteApiRequestSummariesToConsole(List<ApiRequestSummary> requestSummaries)
    {
      foreach (var r in requestSummaries)
      {
        WriteApiRequestSummaryToConsole(r);
      }
    }

    private void WriteApiRequestSummaryToConsole(ApiRequestSummary requestSummary)
    {
      Console.WriteLine();
      Console.WriteLine($"{requestSummary}");
    }
  }
}