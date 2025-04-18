namespace TeachCrowdSale.Api.Models;

public class UserPurchaseModel
{
    public string Address { get; set; }
    public decimal TotalTokens { get; set; }
    public decimal UsdAmount { get; set; }
    public decimal ClaimableTokens { get; set; }
    public List<decimal> TierPurchases { get; set; }
}