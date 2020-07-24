using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Alvianda.Software.Portals.COMD.Data
{
    public interface IEventViewerService
    {
        Task<List<EventViewerEntry>> GetPaginatedResult(string logCategory,int currentPage, int pageSize = 10);
        Task<int> GetCount(string logCategory);
    }

    public class EventViewerService : IEventViewerService
    {
        private HttpClient _httpClient;
        IConfiguration _configuration;

        public EventViewerService(HttpClient client,
                                    IConfiguration configuration)
        {
            _httpClient = client;
            _configuration = configuration;
        }

        public async Task<List<EventViewerEntry>> GetPaginatedResult(string logCategory,int currentPage, int pageSize = 10)
        {
            var data = await GetEventViewerEntries(logCategory);
            return data.OrderBy(d => d.Id).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }

        public async Task<int> GetCount(string logCategory)
        {
            var data = await GetEventViewerEntries(logCategory);
            return data.Count;
        }

        private async Task<List<EventViewerEntry>> GetEventViewerEntries(string selectedLogCategory)
        {
            var serviceEndpoint = $"{_configuration.GetValue<string>("LoggerServicesAPI:BaseURI")}{_configuration.GetValue<string>("LoggerServicesAPI:EventViewerRouting")}/logs/records/{selectedLogCategory}";
            var logEntries = await _httpClient.GetFromJsonAsync<List<EventViewerEntry>>(serviceEndpoint);

            var LogEntries = new List<EventViewerEntry>();
            foreach (var logEntry in logEntries)
            {
                var entry = new EventViewerEntry()
                {
                    Id = logEntry.Id,
                    InstanceId = logEntry.InstanceId,
                    Message = logEntry.Message,
                    MessageShort = logEntry.MessageShort,
                    Source = logEntry.Source,
                    TimeGenerated = logEntry.TimeGenerated,
                    UserName = logEntry.UserName,
                    MachineName = logEntry.MachineName
                };
                LogEntries.Add(entry);
            }
            return LogEntries;
        }

        //public async Task<PaginatedList<EventViewerEntry>> GetPagedResult(string selectedLogCategory,int? pageNumber)
        //{
        //    var serviceEndpoint = $"{_configuration.GetSection("EventViewerLoggerAPI").GetValue<string>("BaseURI")}/api/logreader/logs/records/{selectedLogCategory}/{pageNumber}";

        //    // get a new access token if not cached
        //    if (_httpClient.DefaultRequestHeaders.Authorization == null)
        //    {
        //        var tokenResult = await AuthService.RequestAccessToken();
        //        string jwt = string.Empty;
        //        if (tokenResult.TryGetToken(out var token))
        //        {
        //            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token.Value}");
        //            jwt = $"Bearer {token.Value}";
        //        }
        //    }

        //    var response = await _httpClient.GetAsync(serviceEndpoint));
        //    response.EnsureSuccessStatusCode();

        //    using var responseStream = await response.Content.ReadAsStreamAsync();
        //    var result = await System.Text.Json.JsonSerializer.DeserializeAsync<PaginatedList<EventViewerEntry>>(responseStream, new JsonSerializerOptions
        //    {
        //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        //        PropertyNameCaseInsensitive = true,
        //    });
        //    return result;
        //}


    }
}
