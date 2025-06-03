using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Request
{
    /// <summary>
    /// Request model for searching liquidity pools
    /// </summary>
    public class LiquidityPoolSearchRequest
    {
        [StringLength(50)]
        public string? DexName { get; set; }

        [StringLength(20)]
        public string? TokenPair { get; set; }

        [StringLength(100)]
        public string? SearchTerm { get; set; }

        public bool? IsFeatured { get; set; }

        public bool? IsActive { get; set; } = true;

        [Range(0, double.MaxValue)]
        public decimal? MinTvl { get; set; }

        [Range(0, 200)]
        public decimal? MinApy { get; set; }

        [Range(1, 100)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 20;

        [StringLength(50)]
        public string SortBy { get; set; } = "TotalValueLocked";

        public bool SortDescending { get; set; } = true;
    }
}
