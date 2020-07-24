using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alvianda.Software.Portals.COMD.Data
{
    public class EventViewerEntry
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
