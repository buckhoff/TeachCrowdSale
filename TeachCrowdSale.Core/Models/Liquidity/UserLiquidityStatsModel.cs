namespace TeachCrowdSale.Core.Models.Liquidity
{
    public class UserLiquidityStatsModel
    {
        public string WalletAddress { get; set; } = string.Empty;
        public string DisplayAddress { get; set; } = string.Empty;
        public decimal TotalValueProvided { get; set; }
        public decimal TotalFeesEarned { get; set; }
        public int ActivePositions { get; set; }
        public DateTime FirstProvisionDate { get; set; }
    }
}
