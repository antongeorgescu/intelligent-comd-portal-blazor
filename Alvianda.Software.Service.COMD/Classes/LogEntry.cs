using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alvianda.Software.Service.COMD.Classes
{
    public class LogEntry
    {
        public int Id { get; set; }
        public long InstanceId { get; set; }
        public DateTime TimeGenerated { get; set; }
        public string Source { get; set; }

        public string MessageShort { get; set; }
        public string Message { get; set; }
        public string UserName { get; set; }
        public string MachineName { get; set; }

    }
}
