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
    /// Staking calculation request model
    /// </summary>
    public class StakingCalculationRequest
    {
        [Required(ErrorMessage = "Wallet address is required")]
        [EthereumAddress]
        public string WalletAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pool ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Pool ID must be a valid positive number")]
        public int PoolId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.01")]
        public decimal Amount { get; set; }

        [Range(1, 365, ErrorMessage = "Lock period must be between 1 and 365 days")]
        public int? LockPeriodDays { get; set; }
    }
}
