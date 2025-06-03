using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Configuration
{
    /// <summary>
    /// Configuration model for liquidity settings
    /// </summary>
    public class LiquiditySettings
    {
        public int SyncIntervalMinutes { get; set; } = 5;
        public int PriceRefreshIntervalMinutes { get; set; } = 1;
        public int MaxCacheAgeMinutes { get; set; } = 10;
        public decimal DefaultSlippageTolerance { get; set; } = 0.5m;
        public decimal MaxPriceImpactWarning { get; set; } = 5.0m;
        public int DefaultDeadlineMinutes { get; set; } = 20;
        public bool EnableBackgroundSync { get; set; } = true;
        public bool EnablePriceValidation { get; set; } = true;
        public int HistoricalDataRetentionDays { get; set; } = 90;
    }
}
