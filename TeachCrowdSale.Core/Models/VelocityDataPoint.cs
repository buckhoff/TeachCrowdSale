using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Development velocity data point
    /// </summary>
    public class VelocityDataPoint
    {
        public DateTime WeekStart { get; set; }
        public int TasksCompleted { get; set; }
        public int MilestonesCompleted { get; set; }
        public int CommitsCount { get; set; }
        public decimal VelocityScore { get; set; }
        public string WeekLabel { get; set; } = string.Empty;
    }
}
