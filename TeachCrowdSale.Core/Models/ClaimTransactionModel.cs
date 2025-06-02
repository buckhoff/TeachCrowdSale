using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models;

/// <summary>
/// Claim transaction model for web consumption
/// Maps from ClaimTransaction entity via ClaimResponse
/// </summary>
public class ClaimTransactionModel
{
    public int Id { get; set; }

    [EthereumAddress]
    public string WalletAddress { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal TokenAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TokensClaimed { get; set; }

    [TransactionHash]
    public string TransactionHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime TransactionTime { get; set; }

    public string Status { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal NextVestingAmount { get; set; }

    public DateTime? NextVestingDate { get; set; }
}