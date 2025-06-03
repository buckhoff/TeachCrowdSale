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
    /// Request model for validating liquidity transactions before execution
    /// </summary>
    public class ValidateTransactionRequest
    {
        [Required]
        [EthereumAddress]
        public string WalletAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string TransactionType { get; set; } = string.Empty; // ADD, REMOVE, CLAIM_FEES

        [Range(1, int.MaxValue)]
        public int? PoolId { get; set; }

        [Range(1, int.MaxValue)]
        public int? PositionId { get; set; }

        [Range(0.000001, double.MaxValue)]
        public decimal? Token0Amount { get; set; }

        [Range(0.000001, double.MaxValue)]
        public decimal? Token1Amount { get; set; }

        [Range(0.1, 100)]
        public decimal? PercentageToRemove { get; set; }

        [Range(0.1, 100)]
        public decimal SlippageTolerance { get; set; } = 0.5m;

        public bool CheckBalances { get; set; } = true;

        public bool CheckAllowances { get; set; } = true;

        public bool EstimateGas { get; set; } = true;
    }
}
