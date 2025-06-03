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
    /// Request model for liquidity guide steps
    /// </summary>
    public class LiquidityGuideRequest
    {
        [EthereumAddress]
        public string? WalletAddress { get; set; }

        public bool IncludeCompletedSteps { get; set; } = true;

        [StringLength(50)]
        public string? UserLevel { get; set; } = "Beginner"; // Beginner, Intermediate, Advanced
    }
}
