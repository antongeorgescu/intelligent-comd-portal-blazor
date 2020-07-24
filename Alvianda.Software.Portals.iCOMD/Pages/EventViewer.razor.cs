using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;
using Alvianda.Software.Portals.iCOMD.Data;
using System.Net.Http;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;

namespace Alvianda.Software.Portals.iCOMD.Pages
{
    [Authorize]
    public partial class EventViewer : ComponentBase
    {
        [Inject] HttpClient Http { get; set; }
        [Inject] IAccessTokenProvider AuthService { get; set; }
        [Inject] IConfiguration Config { get; set; }
        [Inject] EventViewerService DataService { get; set; }
        [Inject] IJSRuntime jsRuntime { get; set; }

        private string MachineIdentity { get; set; }

        public PaginatedList<EventViewerEntry> paginatedList; // = new PaginatedList<EventViewerEntry>();

        private List<Log> Logs;
        public string SelectedLog { get; set; }
        private string LongMessage { get; set; }
        private int SelectedId { get; set; }

        private List<EventViewerEntry> LogEntries;

        //private IDictionary<long, Tuple<string, string>> DetailBtnAttributes = new Dictionary<long, Tuple<string, string>>();
        CultureInfo provider = CultureInfo.InvariantCulture;
        
        int totalPages { get; set; }
        int pageIndex;
        bool hasNextPage;
        bool hasPreviousPage;
        static int maxRecords;

        // Page and Sort data
        int? pageNumber = 1;
        //string currentSortField = "Name";
        //string currentSortOrder = "Asc";

        string toDate;
        string fromDate;

        string keywordList;

        int CapMaxRecs;

        private bool isError;
        private string retrievEntriesMsg = "Retrieving records...";

        //private async Task Sort(string sortField)
        //{
        //    if (sortField.Equals(currentSortField))
        //    {
        //        currentSortOrder = currentSortOrder.Equals("Asc") ? "Desc" : "Asc";
        //    }
        //    else
        //    {
        //        currentSortField = sortField;
        //        currentSortOrder = "Asc";
        //    }
        //    await GetLogEntries();
        //}

        //private string SortIndicator(string sortField)
        //{
        //    if (sortField.Equals(currentSortField))
        //    {
        //        return currentSortOrder.Equals("Asc") ? "fa fa-sort-asc" : "fa fa-sort-desc";
        //    }
        //    return string.Empty;
        //}

        protected override async Task OnInitializedAsync()
        {
            MachineIdentity = Config.GetSection("LoggerServicesAPI").GetValue<string>("EventViewerHostMachine");

            await GetCappedMaxRecs();

            paginatedList = new PaginatedList<EventViewerEntry>();
            toDate = DateTime.Today.ToString("yyyy-MM-dd", provider);
            fromDate = DateTime.Today.AddDays(-60).ToString("yyyy-MM-dd", provider);
            await GetLogs();
        }

        public async Task PageIndexChanged(int newPageNumber)
        {
            if (newPageNumber < 1 || newPageNumber > totalPages)
            {
                return;
            }
            pageNumber = newPageNumber;

            LongMessage = null;
            if (LogEntries != null)
                LogEntries = null;
            await GetLogEntries();

            StateHasChanged();
        }

        private async Task GetCappedMaxRecs()
        {
            try
            {
                var serviceEndpoint = $"{Config.GetValue<string>("LoggerServicesAPI:BaseURI")}{Config.GetValue<string>("LoggerServicesAPI:EventViewerRouting")}/logs/capmaxrecs";
                this.CapMaxRecs = await Http.GetFromJsonAsync<int>(serviceEndpoint);
                maxRecords = this.CapMaxRecs;
            }
            catch (Exception exception)
            {
                LongMessage = $"Error:{exception.Message} [{exception.InnerException.Message}]";
            }
        }

        private async Task GetLogs()
        {
            try
            {
                // get a new access token if not cached
                if (Http.DefaultRequestHeaders.Authorization == null)
                {
                    var tokenResult = await AuthService.RequestAccessToken();
                    string jwt = string.Empty;
                    if (tokenResult.TryGetToken(out var token))
                    {
                        Http.DefaultRequestHeaders.Add("Authorization", $"Bearer {token.Value}");
                        jwt = $"Bearer {token.Value}";
                    }
                }

                var serviceEndpoint = $"{Config.GetValue<string>("LoggerServicesAPI:BaseURI")}{Config.GetValue<string>("LoggerServicesAPI:EventViewerRouting")}/logs/categories";
                var logs = await Http.GetFromJsonAsync<List<Log>>(serviceEndpoint);

                Logs = new List<Log>();
                foreach (var log in logs)
                    Logs.Add(new Log() { Name = log.Name, DisplayName = log.DisplayName });
            }
            catch (AccessTokenNotAvailableException exception)
            {
                LongMessage = $"Authorization error:{exception.Message} [{exception.InnerException.Message}]";
            }
            catch (UnauthorizedAccessException exception)
            {
                LongMessage = $"Authorization error:{exception.Message} [{exception.InnerException.Message}]";
            }
            catch (Exception exception)
            {
                LongMessage = $"Error:{exception.Message} [{exception.InnerException.Message}]";
            }
        }

        private async Task GetLogEntries()
        {
            LongMessage = null;
            if (SelectedLog == null)
                return;

            if (SelectedLog == "=== Select Log ===")
                return;

            try
            {
                // get a new access token if not already cached
                if (Http.DefaultRequestHeaders.Authorization == null)
                {
                    var tokenResult = await AuthService.RequestAccessToken();
                    string jwt = string.Empty;
                    if (tokenResult.TryGetToken(out var token))
                    {
                        Http.DefaultRequestHeaders.Add("Authorization", $"Bearer {token.Value}");
                        jwt = $"Bearer {token.Value}";
                    }
                }

                //var serviceEndpoint = $"{Config.GetSection("EventViewerLoggerAPI").GetValue<string>("BaseURI")}/api/logreader/logs/records/{SelectedLog}/{pageNumber}";
                var serviceEndpoint = $"{Config.GetValue<string>("LoggerServicesAPI:BaseURI")}{Config.GetValue<string>("LoggerServicesAPI:EventViewerRouting")}/logs/records/{SelectedLog}/{pageNumber}";
                var response = await Http.GetAsync(serviceEndpoint);
                response.EnsureSuccessStatusCode();

                using var responseStream = await response.Content.ReadAsStreamAsync();
                var paginatedList = await System.Text.Json.JsonSerializer.DeserializeAsync<PaginatedList<EventViewerEntry>>(responseStream, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true,
                });

                LogEntries = paginatedList.Items;

                //DetailBtnAttributes.Clear();
                //LogEntries.ForEach(x => DetailBtnAttributes.Add(x.Id, new Tuple<string, string>("btn btn-success", "Show Details")));

                totalPages = paginatedList.TotalPages;
                pageIndex = paginatedList.PageIndex;
                hasNextPage = paginatedList.HasNextPage;
                hasPreviousPage = paginatedList.HasPreviousPage;
                maxRecords = paginatedList.MaxRecords;

                StateHasChanged();

            }
            catch (AccessTokenNotAvailableException exception)
            {
                isError = true;
                retrievEntriesMsg = string.Empty;
                LongMessage = $"Authorization error:{exception.Message} [{exception.InnerException.Message}]";
            }
            catch (UnauthorizedAccessException exception)
            {
                isError = true;
                retrievEntriesMsg = string.Empty;
                LongMessage = $"Authorization error:{exception.Message} [{exception.InnerException.Message}]";
            }
            catch (Exception exception)
            {
                isError = true;
                //LongMessage = $"Error:{exception.Message} [{exception.InnerException.Message}]";
                retrievEntriesMsg = string.Empty;
                LongMessage = $"Error:{exception.Message}";
            }
        }

        private async Task GetKeywordListEntries()
        {
            LongMessage = null;

            if (SelectedLog == null)
                return;

            if (SelectedLog == "=== Select Log ===")
                return;

            try
            {
                // get a new access token if not already cached
                if (Http.DefaultRequestHeaders.Authorization == null)
                {
                    var tokenResult = await AuthService.RequestAccessToken();
                    string jwt = string.Empty;
                    if (tokenResult.TryGetToken(out var token))
                    {
                        Http.DefaultRequestHeaders.Add("Authorization", $"Bearer {token.Value}");
                        jwt = $"Bearer {token.Value}";
                    }
                }

                //var serviceEndpoint = $"{Config.GetSection("EventViewerLoggerAPI").GetValue<string>("BaseURI")}/api/logreader/logs/records/{SelectedLog}/{fromDate}/{toDate}/{pageNumber}";
                var serviceEndpoint = $"{Config.GetValue<string>("LoggerServicesAPI:BaseURI")}{Config.GetValue<string>("LoggerServicesAPI:EventViewerRouting")}/logs/records/{SelectedLog}/keywords/{keywordList}/{pageNumber}";
                var response = await Http.GetAsync(serviceEndpoint);
                response.EnsureSuccessStatusCode();

                using var responseStream = await response.Content.ReadAsStreamAsync();
                var paginatedList = await System.Text.Json.JsonSerializer.DeserializeAsync<PaginatedList<EventViewerEntry>>(responseStream, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true,
                });

                LogEntries = paginatedList.Items;

                //DetailBtnAttributes.Clear();
                //LogEntries.ForEach(x => DetailBtnAttributes.Add(x.Id, new Tuple<string, string>("btn btn-success", "Show Details")));

                totalPages = paginatedList.TotalPages;
                pageIndex = paginatedList.PageIndex;
                hasNextPage = paginatedList.HasNextPage;
                hasPreviousPage = paginatedList.HasPreviousPage;
                maxRecords = paginatedList.MaxRecords;

                StateHasChanged();

            }
            catch (AccessTokenNotAvailableException exception)
            {
                LongMessage = $"Authorization error:{exception.Message} [{exception.InnerException.Message}]";
            }
            catch (UnauthorizedAccessException exception)
            {
                LongMessage = $"Authorization error:{exception.Message} [{exception.InnerException.Message}]";
            }
            catch (Exception exception)
            {
                LongMessage = $"Error:{exception.Message} [{exception.InnerException.Message}]";
            }
        }

        private async Task GetDatedLogEntries()
        {
            LongMessage = null;
            
            if (SelectedLog == null)
                return;

            if (SelectedLog == "=== Select Log ===")
                return;

            try
            {
                // get a new access token if not already cached
                if (Http.DefaultRequestHeaders.Authorization == null)
                {
                    var tokenResult = await AuthService.RequestAccessToken();
                    string jwt = string.Empty;
                    if (tokenResult.TryGetToken(out var token))
                    {
                        Http.DefaultRequestHeaders.Add("Authorization", $"Bearer {token.Value}");
                        jwt = $"Bearer {token.Value}";
                    }
                }

                //var serviceEndpoint = $"{Config.GetSection("EventViewerLoggerAPI").GetValue<string>("BaseURI")}/api/logreader/logs/records/{SelectedLog}/{fromDate}/{toDate}/{pageNumber}";
                var serviceEndpoint = $"{Config.GetValue<string>("LoggerServicesAPI:BaseURI")}{Config.GetValue<string>("LoggerServicesAPI:EventViewerRouting")}/logs/records/{SelectedLog}/{fromDate}/{toDate}/{pageNumber}";
                var response = await Http.GetAsync(serviceEndpoint);
                response.EnsureSuccessStatusCode();

                using var responseStream = await response.Content.ReadAsStreamAsync();
                var paginatedList = await System.Text.Json.JsonSerializer.DeserializeAsync<PaginatedList<EventViewerEntry>>(responseStream, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true,
                });

                LogEntries = paginatedList.Items;

                //DetailBtnAttributes.Clear();
                //LogEntries.ForEach(x => DetailBtnAttributes.Add(x.Id, new Tuple<string, string>("btn btn-success", "Show Details")));
                
                totalPages = paginatedList.TotalPages;
                pageIndex = paginatedList.PageIndex;
                hasNextPage = paginatedList.HasNextPage;
                hasPreviousPage = paginatedList.HasPreviousPage;
                maxRecords = paginatedList.MaxRecords;

                StateHasChanged();

            }
            catch (AccessTokenNotAvailableException exception)
            {
                LongMessage = $"Authorization error:{exception.Message} [{exception.InnerException.Message}]";
            }
            catch (UnauthorizedAccessException exception)
            {
                LongMessage = $"Authorization error:{exception.Message} [{exception.InnerException.Message}]";
            }
            catch (Exception exception)
            {
                LongMessage = $"Error:{exception.Message} [{exception.InnerException.Message}]";
            }
        }

        //private void ShowMessage(EventViewerEntry entry, MouseEventArgs e)
        //{

        //    if (DetailBtnAttributes[entry.Id].Item2 == "Hide Details")
        //    {
        //        LongMessage = string.Empty;
        //        //DetailBtnAttributes[entry.Id] = new Tuple<string, string>("btn btn-success", "Show Details");
        //    }
        //    else
        //    {
        //        LongMessage = $"Details for Event ID {entry.Id}: {entry.Message}";
        //        //DetailBtnAttributes[entry.Id] = new Tuple<string, string>("btn btn-warning", "Hide Details");
        //    }

        //    if (SelectedId != entry.Id)
        //        DetailBtnAttributes[SelectedId] = new Tuple<string, string>("btn btn-success", "Show Details");

        //    SelectedId = entry.Id;
        //}

        private async void ChangedCappedMaxRecs(ChangeEventArgs e)
        {
            this.CapMaxRecs = int.Parse(e.Value.ToString());
        }
        private async void UpdateCappedMaxRecs()
        {
            
            var serviceEndpoint = $"{Config.GetValue<string>("LoggerServicesAPI:BaseURI")}{Config.GetValue<string>("LoggerServicesAPI:EventViewerRouting")}/logs/capmaxrecs";

            //var payload = "{\"CapSize\":\"" + CapMaxRecs.ToString() + "\"}";
            CappedRecsPayload payload = new CappedRecsPayload() { CappedMaxRecs = this.CapMaxRecs.ToString() };
            string jsonpayload = await Task.Run(() => JsonConvert.SerializeObject(payload));
            HttpContent c = new StringContent(jsonpayload, Encoding.UTF8, "application/json");
            
            var response = await Http.PostAsync(serviceEndpoint,c);
            response.EnsureSuccessStatusCode();

            maxRecords = this.CapMaxRecs;

        }

        private class CappedRecsPayload
        {
            public string CappedMaxRecs { set; get; }
        }

        private void ClearEntryDetails()
        {
            LongMessage = null;
        }

        private void ShowLogEntryDetails(EventViewerEntry entry)
        {
            LongMessage = $"LOG ENTRY DETAILS:{entry.Message}";
        }

        public async Task SelectItem(ChangeEventArgs e)
        {
            LogEntries = null;
            LongMessage = null;
            SelectedLog = Convert.ToString(e.Value);
            pageNumber = 1;
            await GetLogEntries();
        }

        public void SelectStartDate(ChangeEventArgs e)
        {
            fromDate = Convert.ToString(e.Value);
        }

        public void SelectEndDate(ChangeEventArgs e)
        {
            toDate = Convert.ToString(e.Value);
        }

        public void SetKeywordList(ChangeEventArgs e)
        {
            keywordList = Convert.ToString(e.Value);
        }

        public class Log
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
        }
    }
}
