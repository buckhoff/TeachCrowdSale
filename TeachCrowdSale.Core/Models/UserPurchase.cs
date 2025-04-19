using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models;

public class UserPurchase
{
    [EthereumAddress]
    public string Address { get; set; }
    [Range(0, double.MaxValue)]
    public decimal Tokens { get; set; }
    [Range(0, double.MaxValue)]
    public decimal UsdAmount { get; set; }
    public List<decimal> TierAmounts { get; set; }
    public DateTime? LastClaimTime { get; set; }
}