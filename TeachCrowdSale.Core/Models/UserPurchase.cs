namespace TeachCrowdSale.Core.Models;

public class UserPurchase
{
    public string Address { get; set; }
    public decimal Tokens { get; set; }
    public decimal UsdAmount { get; set; }
    public List<decimal> TierAmounts { get; set; }
    public DateTime? LastClaimTime { get; set; }
}