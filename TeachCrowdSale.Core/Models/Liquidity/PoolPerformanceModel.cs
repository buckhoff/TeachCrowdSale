namespace TeachCrowdSale.Core.Models.Liquidity
{
    /// <summary>
    /// Model for individual pool performance analytics
    /// </summary>
    public class PoolPerformanceModel
    {
        public int PoolId { get; set; }
        public string PoolName { get; set; } = string.Empty;
        public string TokenPair { get; set; } = string.Empty;
        public string DexName { get; set; } = string.Empty;
        public decimal TVL { get; set; }
        public decimal TVLChange { get; set; }
        public decimal Volume24h { get; set; }
        public decimal VolumeChange { get; set; }
        public decimal APY { get; set; }
        public decimal APYChange { get; set; }
        public decimal PerformanceScore { get; set; }
        public string PerformanceReason { get; set; } = string.Empty;

        // Display properties
        public string TVLDisplay => FormatCurrency(TVL);
        public string TVLChangeDisplay => FormatPercentage(TVLChange, true);
        public string Volume24hDisplay => FormatCurrency(Volume24h);
        public string VolumeChangeDisplay => FormatPercentage(VolumeChange, true);
        public string APYDisplay => FormatPercentage(APY);
        public string APYChangeDisplay => FormatPercentage(APYChange, true);
        public string PerformanceScoreDisplay => $"{PerformanceScore:F1}/10";

        public string PerformanceClass => PerformanceScore switch
        {
            >= 8 => "performance-excellent",
            >= 6 => "performance-good",
            >= 4 => "performance-fair",
            _ => "performance-poor"
        };

        private static string FormatCurrency(decimal amount)
        {
            return amount switch
            {
                >= 1_000_000 => $"${amount / 1_000_000:F2}M",
                >= 1_000 => $"${amount / 1_000:F2}K",
                _ => $"${amount:F2}"
            };
        }

        private static string FormatPercentage(decimal percentage, bool showSign = false)
        {
            var sign = showSign && percentage >= 0 ? "+" : "";
            return $"{sign}{percentage:F2}%";
        }
    }
}
