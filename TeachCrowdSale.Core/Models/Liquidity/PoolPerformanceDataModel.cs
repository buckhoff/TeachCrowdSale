namespace TeachCrowdSale.Core.Models.Liquidity
{
    public class PoolPerformanceDataModel
    {
        public int PoolId { get; set; }
        public string PoolName { get; set; } = string.Empty;
        public decimal TotalValueLocked { get; set; }
        public decimal Volume24h { get; set; }
        public decimal APY { get; set; }
        public decimal FeesGenerated { get; set; }
        public int ProvidersCount { get; set; }
    }
}
