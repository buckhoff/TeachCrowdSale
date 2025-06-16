using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Response model for progress summary data
    /// </summary>
    public class ProgressSummaryResponse
    {
        public decimal OverallProgress { get; set; }
        public int TotalMilestones { get; set; }
        public int CompletedMilestones { get; set; }
        public int InProgressMilestones { get; set; }
        public int UpcomingMilestones { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int ActiveTasks { get; set; }
        public int OverdueTasks { get; set; }
        public string AverageCompletionTime { get; set; } = string.Empty;
        public string CurrentPhase { get; set; } = string.Empty;
        public string NextMilestone { get; set; } = string.Empty;
        public DateTime? EstimatedProjectCompletion { get; set; }
    }
}
