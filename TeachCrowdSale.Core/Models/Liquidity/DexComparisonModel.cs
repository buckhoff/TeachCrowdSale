namespace TeachCrowdSale.Core.Models.Liquidity
{
    public class DexComparisonModel
    {
        public string DexName { get; set; } = string.Empty;
        public decimal TotalValueLocked { get; set; }
        public decimal Volume24h { get; set; }
        public decimal AverageAPY { get; set; }
        public int PoolsCount { get; set; }
    }
}
