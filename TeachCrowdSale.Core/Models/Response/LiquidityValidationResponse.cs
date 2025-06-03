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
    {
        public bool IsValid { get; set; }
        public bool HasSufficientBalance { get; set; }
        public bool HasSufficientAllowance { get; set; }
        public decimal EstimatedGasFee { get; set; }
        public List<string> ValidationMessages { get; set; } = new();
        public List<string> WarningMessages { get; set; } = new();
        public string TransactionHash { get; set; } = string.Empty;
        public decimal PriceImpact { get; set; }
        public decimal SlippageImpact { get; set; }
    }
}
