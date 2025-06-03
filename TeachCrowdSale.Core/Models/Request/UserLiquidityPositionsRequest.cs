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
    /// Request model for getting user liquidity positions
    /// </summary>
    public class UserLiquidityPositionsRequest
    {
        [Required]
        [EthereumAddress]
        public string WalletAddress { get; set; } = string.Empty;

        public bool? IsActive { get; set; }

        [Range(1, int.MaxValue)]
        public int? PoolId { get; set; }

        [StringLength(50)]
        public string? DexName { get; set; }

        [Range(1, 100)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 50;

        [StringLength(50)]
        public string SortBy { get; set; } = "AddedAt";

        public bool SortDescending { get; set; } = true;
    }
}
