using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLib.DTO
{
    public sealed class ClientLogEntryDto
    {
        public long? LocalId { get; set; }
        public DateTime UtcTime { get; set; }
        public string Level { get; set; } = "Info";
        public string Message { get; set; } = string.Empty;
        public string? Exception { get; set; }
        public string? Page { get; set; }
        public Dictionary<string, string>? Tags { get; set; }
    }
}
