using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models;

/// <summary>
/// Purchase transaction model for web consumption
/// Maps from PurchaseTransaction entity via PurchaseResponse
/// </summary>
public class PurchaseTransactionModel
{
    public int Id { get; set; }

    [EthereumAddress]
    public string WalletAddress { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int TierId { get; set; }

    public string TierName { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal UsdAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TokenAmount { get; set; }

    [Range(0.00000001, double.MaxValue)]
    public decimal TokenPrice { get; set; }

    [TransactionHash]
    public string TransactionHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime TransactionTime { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TotalTokens { get; set; }

    // Vesting information
    [PercentageRange]
    public int VestingTGE { get; set; }

    [Range(0, 60)]
    public int VestingMonths { get; set; }
}