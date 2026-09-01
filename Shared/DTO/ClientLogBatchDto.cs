using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLib.DTO
{
    public sealed class ClientLogBatchDto
    {
        public string DeviceId { get; set; } = string.Empty;
        public List<ClientLogEntryDto> Items { get; set; } = [];
    }
}
