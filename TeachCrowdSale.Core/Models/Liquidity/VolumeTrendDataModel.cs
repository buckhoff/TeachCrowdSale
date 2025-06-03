namespace TeachCrowdSale.Core.Models.Liquidity
{
    public class VolumeTrendDataModel
    {
        public DateTime Date { get; set; }
        public decimal Volume24h { get; set; }
        public decimal FeesGenerated { get; set; }
        public int TransactionCount { get; set; }
    }
}
