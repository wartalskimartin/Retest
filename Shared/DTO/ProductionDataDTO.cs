using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace SharedLib.DTO
{
    public class ProductionDataDTO
    {
        public long? Id { get; set; }
        public string ProductNumber { get; set; }

        public int? Operation { get; set; }

        public int? OrderNumber { get; set; }

        public int? OperatorNumber { get; set; }

        public int? Ok { get; set; }

        public int? Nok { get; set; }

        public DateTime? ProductionDateTime { get; set; }

        public string Device { get; set; }

        public string Loc { get; set; }

        public string ProgramNumber { get; set; }

        public string Controller { get; set; }

        public string ScrapCode { get; set; }

        public int? ShiftNumber { get; set; }

        public string BbxNum { get; set; }

        public DateTime? ConfirmedDateTime { get; set; }

        public DateTime? DatetimeServer { get; set; }

        public string TeamNumber { get; set; }

        [NotMapped]
        public bool DelayedRetest { get; set; }
    }
}
