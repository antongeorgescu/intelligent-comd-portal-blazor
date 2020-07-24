using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Alvianda.Software.Service.COMD.Classes;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Alvianda.Software.Service.COMD.Extensions;
using Alvianda.Software.Service.COMD.Data;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using System.Security.Cryptography;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Alvianda.Software.Service.COMD.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class EventViewerController : ControllerBase
    {
        private IConfiguration Configuration;
        private static int MAXRECORDS;
        private static int PAGESIZE;
        public EventViewerController(IConfiguration configuration)
        {
            Configuration = configuration;

            if (MAXRECORDS == 0)
                MAXRECORDS = int.Parse(Configuration.GetValue<string>("LogReaderServiceSettings:MaxRecords"));
            if (PAGESIZE == 0)
                PAGESIZE = int.Parse(Configuration.GetValue<string>("LogReaderServiceSettings:PageSize"));

        }

        // GET: api/<LogReaderController>/apps
        [HttpGet("logs/info")]
        [Route("logs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<string> GetControllerInfo()
        {
            return Ok($"Web API - EventViewer Controller started on {DateTime.Now}");
        }

        // GET: api/<LogReaderController>/apps
        [HttpGet("logs/capmaxrecs")]
        [Route("logs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<int> GetCappedMaxRecs()
        {
            return Ok(MAXRECORDS);
        }

        // GET: api/<LogReaderController>/apps
        [HttpGet("logs/categories")]
        [Route("logs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [CbsRoleRequirement("COMPAgent")]
        [CbsPermissionRequirement("API.LogReader","AAD")]
        public ActionResult<IList<Log>> Logs()
        {
            // return events recorded in the local EventViewer
            IList<Log> logs = new List<Log>();
            foreach (var log in System.Diagnostics.EventLog.GetEventLogs())
                logs.Add(new Log() {Name=log.Log, DisplayName=log.LogDisplayName });
            return Ok(logs);
        }

        // GET api/<LogReaderController>/5
        [HttpGet("logs/records/{logName}/{pageNumber}/{maxRetrieved}")]
        [HttpGet("logs/records/{logName}/{pageNumber}")]
        [Route("logs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [CbsRoleRequirement("COMPAgent")]
        //[CbsPermissionRequirement("API.LogReader","AAD")]
        [CbsPermissionRequirement("API.LogReader.LogEntriesPaged", "IDM")]
        public ActionResult<PaginatedList<LogEntry>> LogEntriesPaged(string logName, int? pageNumber, int? maxRetrieved)
        {
            EventLog myLog = new EventLog(logName);

            IList<EventLogEntry> myLogEntries = myLog.Entries.Cast<EventLogEntry>().ToList<EventLogEntry>();
            if (myLogEntries == null)
                return NotFound(null);

            IList<LogEntry> logEntries = new List<LogEntry>();
            int i = 0;
            foreach (EventLogEntry entry in myLogEntries)
            {
                logEntries.Add(new LogEntry()
                {
                    Id = i++,
                    TimeGenerated = entry.TimeGenerated,
                    Message = entry.Message,
                    MessageShort = entry.Message.Split('\r')[0],
                    Source = entry.Source,
                    InstanceId = entry.InstanceId,
                    UserName = entry.UserName,
                    MachineName = entry.MachineName
                });
                if (i >= MAXRECORDS)
                    break;
            }

            int pageSize = PAGESIZE;
            var paginatedResult = PaginatedList<LogEntry>.Create(logEntries, pageNumber ?? 1, pageSize);

            if (maxRetrieved == null)
                paginatedResult.MaxRecords = MAXRECORDS;
            else
                paginatedResult.MaxRecords = (int)maxRetrieved;
            
            return Ok(paginatedResult);
        }
        
        // GET api/<LogReaderController>/5
        [HttpGet("logs/records/{logName}/{fromDate}/{toDate}/{pageNumber}/{maxRetrieved}")]
        [HttpGet("logs/records/{logName}/{fromDate}/{toDate}/{pageNumber}")]
        [Route("logs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [CbsRoleRequirement("COMPAgent")]
        [CbsPermissionRequirement("API.LogReader","AAD")]
        public ActionResult<PaginatedList<LogEntry>> DatedLogEntriesPaged(
                                                                string logName,
                                                                string fromDate,
                                                                string toDate,
                                                                int? pageNumber,
                                                                int? maxRetrieved)
        {
            //int DATEDMAXRECORDS = int.Parse(Configuration.GetValue<string>("LogReaderServiceSettings:DatedMaxRecords"));
            
            DateTime dateFrom;
            DateTime.TryParse(fromDate, out dateFrom);

            DateTime dateTo;
            DateTime.TryParse(toDate, out dateTo);

            EventLog myLog = new EventLog(logName);

            IList<EventLogEntry> myLogEntries = myLog.Entries.Cast<EventLogEntry>().ToList();

            if (myLogEntries == null)
                return NotFound(null);

            IList<LogEntry> logEntries = new List<LogEntry>();
            int i = 0;
            foreach (EventLogEntry entry in myLogEntries)
            {
                if (DateTime.Compare(entry.TimeGenerated, dateFrom) >= 0 && DateTime.Compare(entry.TimeGenerated, dateTo) <= 0)
                {
                    logEntries.Add(new LogEntry()
                    {
                        Id = i++,
                        TimeGenerated = entry.TimeGenerated,
                        Message = entry.Message,
                        MessageShort = entry.Message.Split('\r')[0],
                        Source = entry.Source,
                        InstanceId = entry.InstanceId,
                        UserName = entry.UserName,
                        MachineName = entry.MachineName
                    });
                    if (i >= MAXRECORDS)
                        break;
                }
            }

            int pageSize = PAGESIZE;
            var paginatedResult = PaginatedList<LogEntry>.Create(logEntries, pageNumber ?? 1, pageSize);

            //paginatedResult.MaxRecords = DATEDMAXRECORDS;
            if (maxRetrieved == null)
                paginatedResult.MaxRecords = MAXRECORDS;
            else
                paginatedResult.MaxRecords = (int)maxRetrieved;

            return Ok(paginatedResult);
        }

        // GET api/<LogReaderController>/5
        [HttpGet("logs/categories/{fromDate}/{toDate}")]
        [Route("logs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [CbsRoleRequirement("COMPAgent")]
        [CbsPermissionRequirement("API.LogReader.Charts", "IDM")]
        public ActionResult<List<Tuple<Log,int>>> DatedLogsDistribution(
                                                                string fromDate,
                                                                string toDate)
        {
            DateTime dateFrom;
            DateTime.TryParse(fromDate, out dateFrom);

            DateTime dateTo;
            DateTime.TryParse(toDate, out dateTo);

            IList<Log> logs = new List<Log>();
            foreach (var log in System.Diagnostics.EventLog.GetEventLogs())
            {
                if (fromDate == "null" && toDate=="null")
                    logs.Add(new Log() { Name = log.Log, DisplayName = log.LogDisplayName, EntriesCount = log.Entries.Count });
                else
                {
                    IList<EventLogEntry> logEntries = log.Entries.Cast<EventLogEntry>().ToList();
                    int count = 0;
                    foreach (var entry in logEntries)
                        if (DateTime.Compare(entry.TimeGenerated, dateFrom) >= 0 && DateTime.Compare(entry.TimeGenerated, dateTo) <= 0)
                            count++;
                    logs.Add(new Log() { Name = log.Log, DisplayName = log.LogDisplayName, EntriesCount = count });
                }
            }
            
            if (logs == null)
                return NotFound(null);

            return Ok(logs);
        }


        // GET api/<LogReaderController>/5
        [HttpGet("logs/records/{logName}/keywords/{keywordList}/{pageNumber}/{maxRetrieved}")]
        [HttpGet("logs/records/{logName}/keywords/{keywordList}/{pageNumber}")]
        [Route("logs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [CbsRoleRequirement("COMPAgent")]
        [CbsPermissionRequirement("API.LogReader","AAD")]
        public ActionResult<PaginatedList<LogEntry>> KeywordsLogEntriesPaged(
                                                                string logName,
                                                                string keywordList,
                                                                int? pageNumber,
                                                                int? maxRetrieved)
        {
            // validate keywordList to be comma separated
            // if not return 500 error
            IList<string> keywords;
            //var isValid = Regex.Match(keywordList, "^([a-zA-Z]+,)+[a-zA-Z]+$");
            var isValid = Regex.Match(keywordList, @"\w+(\s*,\s*\w+)*");
            if (isValid.Success)
                keywords = keywordList.Split(',').ToList();
            else
                return StatusCode(StatusCodes.Status500InternalServerError);
            
            EventLog myLog = new EventLog(logName);

            IList<EventLogEntry> myLogEntries = myLog.Entries.Cast<EventLogEntry>().ToList();

            if (myLogEntries == null)
                return NotFound(null);

            IList<LogEntry> logEntries = new List<LogEntry>();
            int i = 0;
            foreach (EventLogEntry entry in myLogEntries)
            {
                ObjectExtensions.Each(keywords, el =>
                {
                    if (entry.Message.Contains(el) || entry.Source.Contains(el) || entry.MachineName.Contains(el) || entry.UserName.Contains(el))
                    {
                        logEntries.Add(new LogEntry()
                        {
                            Id = i++,
                            TimeGenerated = entry.TimeGenerated,
                            Message = entry.Message.Replace(el,$"<mark style=\"background-color:yellow;\">{el}</mark>"),
                            MessageShort = entry.Message.Split('\r')[0].Replace(el, $"<mark style=\"background-color:yellow;\">{el}</mark>"),
                            Source = entry.Source.Replace(el, $"<mark style=\"background-color:yellow;\">{el}</mark>"),
                            InstanceId = entry.InstanceId,
                            UserName = entry.UserName == null ? entry.UserName : entry.UserName.Replace(el, $"<mark style=\"background-color:yellow;\">{el}</mark>"),
                            MachineName = entry.MachineName == null ? entry.MachineName : entry.MachineName.Replace(el, $"<mark style=\"background-color:yellow;\">{el}</mark>")
                        });
                    }
                });
                if (i >= MAXRECORDS)
                    break;
            }

            int pageSize = PAGESIZE;
            var paginatedResult = PaginatedList<LogEntry>.Create(logEntries, pageNumber ?? 1, pageSize);

            //paginatedResult.MaxRecords = DATEDMAXRECORDS;
            if (maxRetrieved == null)
                paginatedResult.MaxRecords = MAXRECORDS;
            else
                paginatedResult.MaxRecords = (int)maxRetrieved;

            return Ok(paginatedResult);
        }

        // GET api/<LogReaderController>/5
        [HttpGet("logs/records/{logName}")]
        [Route("logs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [CbsRoleRequirement("COMPAgent")]
        [CbsPermissionRequirement("API.LogReader","AAD")]
        public ActionResult<IList<LogEntry>> LogEntries(string logName)
        {
            IList<LogEntry> logEntries = new List<LogEntry>();
            EventLog myLog = new EventLog(logName);

            var myLogEntries = myLog.Entries.Cast<EventLogEntry>().ToList<EventLogEntry>();
            if (myLogEntries == null)
                return NotFound(null);

            int i = 0;
            foreach (EventLogEntry entry in myLogEntries)
            {
                logEntries.Add(new LogEntry()
                {
                    Id = i,
                    TimeGenerated = entry.TimeGenerated,
                    Message = entry.Message,
                    MessageShort = entry.Message.Split('\r')[0],
                    Source = entry.Source,
                    InstanceId = entry.InstanceId,
                    UserName = entry.UserName,
                    MachineName = entry.MachineName
                });
                i++;
                if (i > MAXRECORDS)
                    break;
            }
            return Ok(logEntries);
        }

        // GET api/<LogReaderController>/5
        [HttpGet("logs/count/{logName}")]
        [Route("logs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [CbsRoleRequirement("COMPAgent")]
        [CbsPermissionRequirement("API.LogReader","AAD")]
        public ActionResult<int> LogEntriesCount(string logName)
        {
            EventLog myLog = new EventLog(logName);
            var count = myLog.Entries.Count;
            return Ok(count);
        }

        // POST api/<LogReaderController>
        [HttpPost]
        [Route("logs/capmaxrecs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public void Post([FromBody] CappedRecsPayload payload)
        {
            MAXRECORDS = int.Parse(payload.CappedMaxRecs);
        }

        // PUT api/<LogReaderController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<LogReaderController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
