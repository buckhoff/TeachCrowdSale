using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Liquidity
{
    /// <summary>
    /// User's liquidity position model
    /// </summary>
    public class UserLiquidityPositionModel
    {
        public int Id { get; set; }
        public int PoolId { get; set; }
        public string PoolName { get; set; } = string.Empty;
        public string TokenPair { get; set; } = string.Empty;
        public string DexName { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal LpTokenAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Token0Amount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Token1Amount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal InitialValueUsd { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CurrentValueUsd { get; set; }

        [Range(0, double.MaxValue)]
        public decimal FeesEarnedUsd { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ImpermanentLoss { get; set; }

        public decimal NetPnL { get; set; }
        public decimal PnLPercentage { get; set; }

        public DateTime AddedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }

        public bool IsActive { get; set; }
        public bool CanRemove { get; set; }
        public bool CanClaimFees { get; set; }

        // Formatted display values
        public string InitialValueDisplay { get; set; } = string.Empty;
        public string CurrentValueDisplay { get; set; } = string.Empty;
        public string FeesEarnedDisplay { get; set; } = string.Empty;
        public string PnLDisplay { get; set; } = string.Empty;
        public string PnLClass { get; set; } = string.Empty;
        public string DaysActive { get; set; } = string.Empty;

        // Token amounts display
        public string Token0AmountDisplay { get; set; } = string.Empty;
        public string Token1AmountDisplay { get; set; } = string.Empty;
        public string Token0Symbol { get; set; } = string.Empty;
        public string Token1Symbol { get; set; } = string.Empty;
    }
}
