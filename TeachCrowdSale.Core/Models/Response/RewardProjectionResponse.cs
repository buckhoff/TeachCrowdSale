using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Response model for reward projections
    /// </summary>
    public class RewardProjectionResponse
    {
        public DateTime Date { get; set; }
        public decimal CumulativeRewards { get; set; }
        public decimal PeriodRewards { get; set; }
        public decimal CompoundedAmount { get; set; }
        public decimal UserShare { get; set; }
        public decimal SchoolShare { get; set; }
        public string FormattedRewards { get; set; } = string.Empty;
        public string FormattedCompounded { get; set; } = string.Empty;
    }
}
