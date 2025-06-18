using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Data.Entities
{
    /// <summary>
    /// Liquidity pool configuration and metrics
    /// </summary>
    public class LiquidityPool
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string DexName { get; set; } = string.Empty; // Uniswap, PancakeSwap, etc.

        [Required]
        [StringLength(20)]
        public string TokenPair { get; set; } = string.Empty; // TEACH/USDC, TEACH/ETH

        [EthereumAddress]
        public string PoolAddress { get; set; } = string.Empty;

        [EthereumAddress]
        public string Token0Address { get; set; } = string.Empty; // TEACH

        [EthereumAddress]
        public string Token1Address { get; set; } = string.Empty; // USDC/ETH/etc

        [StringLength(10)]
        public string Token0Symbol { get; set; } = string.Empty;

        [StringLength(10)]
        public string Token1Symbol { get; set; } = string.Empty;

        public int Token0Decimals { get; set; } = 18;
        public int Token1Decimals { get; set; } = 18;

        [Range(0, double.MaxValue)]
        public decimal TotalValueLocked { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Volume24h { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Volume7d { get; set; }

        [Range(0, 1000)]
        public decimal FeePercentage { get; set; } = 0.3m; // 0.3% typical

        [Range(0, 200)]
        public decimal APY { get; set; }

        [Range(0, 200)]
        public decimal APR { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Token0Reserve { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Token1Reserve { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CurrentPrice { get; set; } // Token0 price in Token1

        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; }
        public bool IsRecommended { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? DexUrl { get; set; }

        [StringLength(200)]
        public string? AnalyticsUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastSyncAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public List<UserLiquidityPosition> UserPositions { get; set; } = new();
        public List<LiquidityPoolSnapshot> Snapshots { get; set; } = new();
    }
}
