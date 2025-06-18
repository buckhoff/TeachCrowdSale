using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Response model for liquidity validation
    /// </summary>
    public class LiquidityValidationResponse
    {// Core validation result
        public bool IsValid { get; set; }
        public bool HasSufficientBalance { get; set; }
        public bool HasSufficientAllowance { get; set; }

        // Balance information
        public decimal Token0Balance { get; set; }
        public decimal Token1Balance { get; set; }
        public decimal EthBalance { get; set; }

        // Transaction cost estimates
        public decimal EstimatedGasFee { get; set; }
        public decimal EstimatedGasCost { get; set; } // Alternative naming used in service

        // Price and market impact
        public decimal PriceImpact { get; set; }
        public decimal SlippageImpact { get; set; }

        // Expected results for add liquidity
        public decimal ExpectedLpTokens { get; set; }
        public decimal ExpectedValueUsd { get; set; }

        // Remove liquidity calculations
        public decimal Token0AmountToRemove { get; set; }
        public decimal Token1AmountToRemove { get; set; }
        public decimal ValueToRemove { get; set; }

        // Fees information
        public decimal FeesToClaim { get; set; }

        // Messages
        public List<string> ValidationMessages { get; set; } = new();
        public List<string> WarningMessages { get; set; } = new();

        // Transaction information
        public string TransactionHash { get; set; } = string.Empty;

        // Metadata
        public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
        public string ValidationId { get; set; } = Guid.NewGuid().ToString();
    }
}
