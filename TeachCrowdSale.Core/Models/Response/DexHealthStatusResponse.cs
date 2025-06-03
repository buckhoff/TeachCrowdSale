using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// DEX integration health status response
    /// </summary>
    public class DexHealthStatusResponse
    {
        public bool IsHealthy { get; set; }
        public Dictionary<string, DexHealthInfo> DexStatus { get; set; } = new();
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
        public string OverallStatus { get; set; } = string.Empty;
    }
}
