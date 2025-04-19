using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Response;

public class PurchaseValidationModel
{
    public bool IsValid { get; set; }
    [EthereumAddress]
    public string Address { get; set; } = string.Empty;
    [Range(0, int.MaxValue)]
    public int TierId { get; set; }
    public string TierName { get; set; } = string.Empty;
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
    [Range(0, double.MaxValue)]
    public decimal TokensToReceive { get; set; }
    [Range(0.00000001, double.MaxValue)]
    public decimal Price { get; set; }
    [Range(0, double.MaxValue)]
    public decimal ExistingPurchase { get; set; }
    [PercentageRange]
    public int VestingTGE { get; set; }
    [Range(0, 60, ErrorMessage = "Vesting period cannot exceed 60 months")]
    public int VestingMonths { get; set; }
}