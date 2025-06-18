using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Response model for liquidity guide step data
    /// Educational content for API consumption
    /// </summary>
    public class LiquidityGuideStepResponse
    {
        public int StepNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;

        // FIXED: Added missing ActionText and ActionUrl properties
        public string ActionText { get; set; } = string.Empty;
        public string ActionUrl { get; set; } = string.Empty;

        public List<string> ActionItems { get; set; } = new();
        public List<string> Tips { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public string DexName { get; set; } = string.Empty;
        public string ExternalUrl { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public bool RequiresWalletConnection { get; set; }
    }
}
