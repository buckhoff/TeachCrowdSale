using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Data.Entities
{
    /// <summary>
    /// Historical liquidity pool metrics
    /// </summary>
    public class LiquidityPoolSnapshot
    {
        public int Id { get; set; }

        public int LiquidityPoolId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalValueLocked { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Volume24h { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Token0Reserve { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Token1Reserve { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, 200)]
        public decimal APY { get; set; }

        [Range(0, 200)]
        public decimal APR { get; set; }

        [Range(0, int.MaxValue)]
        public int ActivePositions { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string Source { get; set; } = "DEX_API";

        public bool IsLatest { get; set; }

        // Navigation properties
        public LiquidityPool LiquidityPool { get; set; } = null!;
    }
}
