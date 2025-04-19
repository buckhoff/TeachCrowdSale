using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Response;

public class ClaimableTokensModel
{
    [EthereumAddress]
    public string Address { get; set; } = string.Empty;
    [Range(0, double.MaxValue)]
    public decimal TotalTokens { get; set; }
    [Range(0, double.MaxValue)]
    public decimal ClaimableTokens { get; set; }
    public DateTime? LastClaimTime { get; set; }
    public DateTime? NextVestingDate { get; set; }
    [Range(0, double.MaxValue)]
    public decimal NextVestingAmount { get; set; }
}