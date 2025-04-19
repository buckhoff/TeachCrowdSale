using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Api.Models;

public class UserPurchaseModel
{
    [EthereumAddress]
    public string Address { get; set; }
    [Range(0, double.MaxValue)]
    public decimal TotalTokens { get; set; }
    [Range(0, double.MaxValue)]
    public decimal UsdAmount { get; set; }
    [Range(0, double.MaxValue)]
    public decimal ClaimableTokens { get; set; }
    public List<decimal> TierPurchases { get; set; }
}