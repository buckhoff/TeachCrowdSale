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
    /// Stake tokens request model
    /// </summary>
    public class StakeTokensRequest
    {
        [Required(ErrorMessage = "Wallet address is required")]
        [EthereumAddress]
        public string WalletAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pool ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Pool ID must be a valid positive number")]
        public int PoolId { get; set; }

        [Required(ErrorMessage = "Stake amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Stake amount must be greater than 0.01")]
        public decimal Amount { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "School beneficiary ID must be a valid positive number")]
        public int? SchoolBeneficiaryId { get; set; }
    }
}
