using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Response model for liquidity pool data
    /// Maps from LiquidityPool entity for API consumption
    /// </summary>
    public class LiquidityPoolResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TokenPair { get; set; } = string.Empty;
        public string Token0Symbol { get; set; } = string.Empty;
        public string Token1Symbol { get; set; } = string.Empty;
        public string DexName { get; set; } = string.Empty;
        public string PoolAddress { get; set; } = string.Empty;
        public decimal CurrentAPY { get; set; }
        public decimal TotalValueLocked { get; set; }
        public decimal Volume24h { get; set; }
        public decimal FeePercentage { get; set; }
        public bool IsActive { get; set; }
        public bool IsRecommended { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
