using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Configuration
{
    /// <summary>
    /// Configuration model for DEX integration settings
    /// </summary>
    public class DexIntegrationSettings
    {
        public string QuickSwapApiUrl { get; set; } = string.Empty;
        public string UniswapApiUrl { get; set; } = string.Empty;
        public string SushiSwapApiUrl { get; set; } = string.Empty;
        public string OneInchApiUrl { get; set; } = string.Empty;
        public int RequestTimeoutSeconds { get; set; } = 30;
        public int RetryAttempts { get; set; } = 3;
        public int RateLimitPerMinute { get; set; } = 60;
        public int CacheDurationMinutes { get; set; } = 5;
        public bool EnableFallbackPricing { get; set; } = true;
        public decimal DefaultSlippageTolerance { get; set; } = 0.5m;
    }
}
