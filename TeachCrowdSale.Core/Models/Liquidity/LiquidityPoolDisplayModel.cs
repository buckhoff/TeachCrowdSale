using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Liquidity
{
    /// <summary>
    /// Liquidity pool display model for frontend
    /// </summary>
    public class LiquidityPoolDisplayModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DexName { get; set; } = string.Empty;
        public string TokenPair { get; set; } = string.Empty;
        public string Token0Symbol { get; set; } = string.Empty;
        public string Token1Symbol { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal TotalValueLocked { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Volume24h { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Volume7d { get; set; }

        [Range(0, 1000)]
        public decimal FeePercentage { get; set; }

        [Range(0, 200)]
        public decimal APY { get; set; }

        [Range(0, 200)]
        public decimal APR { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CurrentPrice { get; set; }

        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsRecommended { get; set; }

        public string DexUrl { get; set; } = string.Empty;
        public string AnalyticsUrl { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;

        // Formatted display values
        public string TvlDisplay { get; set; } = string.Empty;
        public string Volume24hDisplay { get; set; } = string.Empty;
        public string ApyDisplay { get; set; } = string.Empty;
        public string FeeDisplay { get; set; } = string.Empty;
        public string PriceDisplay { get; set; } = string.Empty;

        // Status and styling
        public string StatusClass { get; set; } = string.Empty;
        public string RecommendationReason { get; set; } = string.Empty;
    }
}
