using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Pool performance data for API response
    /// </summary>
    public class PoolPerformanceDataResponse
    {
        public int PoolId { get; set; }
        public string PoolName { get; set; } = string.Empty;
        public string TokenPair { get; set; } = string.Empty;
        public decimal APY { get; set; }
        public decimal TotalValueLocked { get; set; }
        public decimal Volume24h { get; set; }
        public decimal FeesGenerated { get; set; }
        public int LiquidityProviders { get; set; }
    }
}
