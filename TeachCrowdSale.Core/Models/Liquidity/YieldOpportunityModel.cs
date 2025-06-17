using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Liquidity
{
    /// <summary>
    /// Model for yield opportunities
    /// </summary>
    public class YieldOpportunityModel
    {
        public int PoolId { get; set; }
        public string PoolName { get; set; } = string.Empty;
        public string TokenPair { get; set; } = string.Empty;
        public decimal APY { get; set; }
        public decimal TVL { get; set; }
        public decimal RiskScore { get; set; }
        public string OpportunityType { get; set; } = string.Empty;
        public List<string> Benefits { get; set; } = new();
        public List<string> Risks { get; set; } = new();

        public string APYDisplay => $"{APY:F2}%";
        public string TVLDisplay => FormatCurrency(TVL);
        public string RiskScoreDisplay => $"{RiskScore:F1}/10";

        private static string FormatCurrency(decimal amount)
        {
            return amount switch
            {
                >= 1_000_000 => $"${amount / 1_000_000:F2}M",
                >= 1_000 => $"${amount / 1_000:F2}K",
                _ => $"${amount:F2}"
            };
        }
    }
}
