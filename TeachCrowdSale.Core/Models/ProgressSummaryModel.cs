using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Progress summary metrics for dashboard widgets
    /// </summary>
    public class ProgressSummaryModel
    {
        public string Title { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal TargetValue { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string DisplayText { get; set; } = string.Empty;
        public string TrendDirection { get; set; } = "stable"; // up, down, stable
        public string TrendClass { get; set; } = "trend-stable";
        public decimal PercentageComplete { get; set; }
        public string ProgressBarClass { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string BackgroundClass { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }
}
