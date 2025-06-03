using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Liquidity
{
    /// <summary>
    /// Overall liquidity statistics
    /// </summary>
    public class LiquidityStatsOverviewModel
    {
        [Range(0, double.MaxValue)]
        public decimal TotalValueLocked { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalVolume24h { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalVolume7d { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalFeesGenerated { get; set; }

        [Range(0, int.MaxValue)]
        public int ActivePools { get; set; }

        [Range(0, int.MaxValue)]
        public int ActiveProviders { get; set; }

        [Range(0, 200)]
        public decimal AverageAPY { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TeachPriceUsd { get; set; }

        public decimal PriceChange24h { get; set; }
        public decimal VolumeChange24h { get; set; }
        public decimal TvlChange24h { get; set; }

        // Formatted displays
        public string TvlDisplay { get; set; } = string.Empty;
        public string Volume24hDisplay { get; set; } = string.Empty;
        public string FeesDisplay { get; set; } = string.Empty;
        public string ApyDisplay { get; set; } = string.Empty;
        public string PriceDisplay { get; set; } = string.Empty;
        public string PriceChangeDisplay { get; set; } = string.Empty;
        public string PriceChangeClass { get; set; } = string.Empty;
    }
}
