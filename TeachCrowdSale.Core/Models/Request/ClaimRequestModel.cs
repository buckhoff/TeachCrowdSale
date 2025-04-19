using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Request;

public class ClaimRequestModel
{
    [Required(ErrorMessage = "Wallet address is required")]
    [EthereumAddress]
    public string Address { get; set; } = string.Empty;
}