using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Response;

public class ClaimResponseModel
{
    [EthereumAddress]
    public string Address { get; set; } = string.Empty;
    [Range(0, double.MaxValue)]
    public decimal TokensClaimed { get; set; }
    public DateTime TransactionTime { get; set; }
    [Range(0, double.MaxValue)]
    public decimal NextVestingAmount { get; set; }
    public DateTime NextVestingDate { get; set; }
    [TransactionHash]
    public string? TransactionHash { get; set; }
}