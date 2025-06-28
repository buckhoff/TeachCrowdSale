using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    public class TokenIntegrationModel
    {
        public decimal CurrentPrice { get; set; }
        public decimal TotalRaised { get; set; }
        public int TotalHolders { get; set; }
        public bool IsSaleActive { get; set; }
        public DateTime? SaleEndDate { get; set; }
        public List<TokenBenefit> Benefits { get; set; } = new()
        {
            new() { Title = "Reduced Fees", Description = "Pay lower platform fees when donating with TEACH tokens", Icon = "💰" },
            new() { Title = "Governance Rights", Description = "Vote on featured projects and platform decisions", Icon = "🗳️" },
            new() { Title = "Staking Rewards", Description = "Earn rewards while supporting your chosen school", Icon = "🏫" },
            new() { Title = "Impact Tracking", Description = "Enhanced analytics and impact reporting", Icon = "📈" }
        };
    }
    public class TokenBenefit
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }
}
