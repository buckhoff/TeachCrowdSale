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
    /// Request model for claiming fees from a liquidity position
    /// </summary>
    public class ClaimFeesRequest
    {
        [Required]
        [EthereumAddress]
        public string WalletAddress { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int PositionId { get; set; }

        [Range(1, 60)]
        public int DeadlineMinutes { get; set; } = 20;
    }
}
