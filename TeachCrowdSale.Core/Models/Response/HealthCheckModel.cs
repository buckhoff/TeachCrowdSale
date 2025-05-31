using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    public class HealthCheckModel
    {
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Service { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public Dictionary<string, object>? Details { get; set; }
        public string? Error { get; set; }
    }
}
