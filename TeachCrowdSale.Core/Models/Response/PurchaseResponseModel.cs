using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Response;

public class PurchaseResponseModel
{
    [TransactionHash]
    public string? TransactionHash { get; set; }
    [EthereumAddress]
    public string Address { get; set; } = string.Empty;
    [Range(0, int.MaxValue)]
    public int TierId { get; set; }
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
    [Range(0, double.MaxValue)]
    public decimal TokensReceived { get; set; }
    [Range(0, double.MaxValue)]
    public decimal TotalTokens { get; set; }
    public DateTime TransactionTime { get; set; } = DateTime.UtcNow;
}