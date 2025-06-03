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
    /// Request model for calculating remove liquidity preview
    /// </summary>
    public class RemoveLiquidityCalculationRequest
    {
        [Required]
        [EthereumAddress]
        public string WalletAddress { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int PositionId { get; set; }

        [Required]
        [Range(0.1, 100)]
        public decimal PercentageToRemove { get; set; }
    }
}
