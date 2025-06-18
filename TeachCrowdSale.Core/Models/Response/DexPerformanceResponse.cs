using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Response model for DEX performance comparison data
    /// Maps from DEX performance calculations for API consumption
    /// </summary>
    public class DexPerformanceResponse
    {
        public string DexName { get; set; } = string.Empty;
        public decimal TotalValueLocked { get; set; }
        public decimal Volume24h { get; set; }
        public decimal AverageAPY { get; set; }
        public int PoolsCount { get; set; }
        public string LogoUrl { get; set; } = string.Empty;
        public decimal MarketShare { get; set; }
        public DateTime CalculatedAt { get; set; }
    }
}
