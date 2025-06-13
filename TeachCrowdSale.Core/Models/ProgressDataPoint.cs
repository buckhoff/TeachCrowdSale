using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Progress chart data point
    /// </summary>
    public class ProgressDataPoint
    {
        public DateTime Date { get; set; }
        public decimal OverallProgress { get; set; }
        public int CompletedMilestones { get; set; }
        public int TotalMilestones { get; set; }
        public string FormattedDate { get; set; } = string.Empty;
    }
}
