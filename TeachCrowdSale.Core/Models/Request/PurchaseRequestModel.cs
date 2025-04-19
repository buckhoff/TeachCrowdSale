using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Request;

public class PurchaseRequestModel
{
    [Required(ErrorMessage = "Wallet address is required")]
    [EthereumAddress] 
    public string Address { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Tier ID is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Tier ID must be a non-negative number")]
    public int TierId { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Purchase amount must be greater than 0.01")]
    public decimal Amount { get; set; }
}