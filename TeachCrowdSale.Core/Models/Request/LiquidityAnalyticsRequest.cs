using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Request
{
    /// <summary>
    /// Request model for liquidity analytics data
    /// </summary>
    public class LiquidityAnalyticsRequest
    {
        [Range(1, 365)]
        public int Days { get; set; } = 30;

        [StringLength(50)]
        public string? DexName { get; set; }

        [Range(1, int.MaxValue)]
        public int? PoolId { get; set; }

        public bool IncludeTvlTrends { get; set; } = true;

        public bool IncludeVolumeTrends { get; set; } = true;

        public bool IncludePoolPerformance { get; set; } = true;

        public bool IncludeTopProviders { get; set; } = true;

        public bool IncludeDexComparison { get; set; } = true;

        [Range(1, 100)]
        public int TopProvidersLimit { get; set; } = 10;
    }
}
