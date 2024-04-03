using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLib.Models
{
    public class QualityCounterResult
    {
        public long Id { get; set; }

        public string ProductNumber { get; set; }

        public int? OrderNumber { get; set; }

        public int? OperationNumber { get; set; }

        public string IndexNumber { get; set; }

        public string DeviceNumber { get; set; }

        public string LocationNumber { get; set; }

        public int? ControllerNumber { get; set; }

        public int? ShiftNumber { get; set; }

        public int? OperatorNumber { get; set; }

        public int? QualityCounter1 { get; set; }

        public string BilboxNumber { get; set; }

        public DateTime? ProductionDateTime { get; set; }

        public DateTime? DateTimeServer { get; set; }
    }
}
