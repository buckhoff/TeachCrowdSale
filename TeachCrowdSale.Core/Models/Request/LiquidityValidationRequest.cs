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
    /// Request model for liquidity validation
    /// </summary>
    public class LiquidityValidationRequest
    {
        [Required]
        [EthereumAddress]
        public string WalletAddress { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int PoolId { get; set; }

        [Required]
        [Range(0.000001, double.MaxValue)]
        public decimal Token0Amount { get; set; }

        [Required]
        [Range(0.000001, double.MaxValue)]
        public decimal Token1Amount { get; set; }
    }
}
