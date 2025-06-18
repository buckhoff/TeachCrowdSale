namespace TeachCrowdSale.Core.Models.Liquidity
{
    /// <summary>
    /// Model for DEX performance analytics
    /// </summary>
    public class DexPerformanceModel
    {
        public string DexName { get; set; } = string.Empty;
        public decimal TotalTVL { get; set; }
        public decimal MarketShare { get; set; }
        public decimal GrowthRate { get; set; }
        public int PoolCount { get; set; }
        public decimal AverageAPY { get; set; }
        public decimal UserCount { get; set; }
        public decimal PerformanceScore { get; set; }
        public decimal Volume24h { get; set; }

        public string TotalTVLDisplay => FormatCurrency(TotalTVL);
        public string MarketShareDisplay => $"{MarketShare:F1}%";
        public string GrowthRateDisplay => $"{(GrowthRate >= 0 ? "+" : "")}{GrowthRate:F1}%";
        public string AverageAPYDisplay => $"{AverageAPY:F2}%";
        public string PerformanceScoreDisplay => $"{PerformanceScore:F1}/10";

        private static string FormatCurrency(decimal amount)
        {
            return amount switch
            {
                >= 1_000_000_000 => $"${amount / 1_000_000_000:F2}B",
                >= 1_000_000 => $"${amount / 1_000_000:F2}M",
                _ => $"${amount:F2}"
            };
        }
    }
}
