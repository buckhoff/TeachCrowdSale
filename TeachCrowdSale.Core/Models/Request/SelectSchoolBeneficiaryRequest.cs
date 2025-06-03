using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Request 
{ 

/// <summary>
/// Select school beneficiary request model
/// </summary>
public class SelectSchoolBeneficiaryRequest
{
    [Required(ErrorMessage = "Wallet address is required")]
    [EthereumAddress]
    public string WalletAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "School beneficiary ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "School beneficiary ID must be a valid positive number")]
    public int SchoolBeneficiaryId { get; set; }
}
}
