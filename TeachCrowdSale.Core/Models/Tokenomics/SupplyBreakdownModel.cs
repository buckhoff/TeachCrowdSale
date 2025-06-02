using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Tokenomics
{
    /// <summary>
    /// Token supply breakdown model
    /// </summary>
    public class SupplyBreakdownModel
    {
        public List<TokenAllocationModel> Allocations { get; set; } = new();
        public SupplyMetricsModel Metrics { get; set; } = new();
        public List<SupplyTimelineModel> Timeline { get; set; } = new();
    }
}
