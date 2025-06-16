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

        // Display helper properties
        public string OverallProgressFormatted => $"{OverallProgress:F1}%";
        public string CompletionRateText => $"{CompletedMilestones}/{TotalMilestones} milestones";
        public string TaskProgressText => $"{CompletedTasks}/{TotalTasks} tasks";
        public string EstimatedCompletionFormatted => EstimatedProjectCompletion?.ToString("MMM yyyy") ?? "TBD";
        public bool HasOverdueTasks => OverdueTasks > 0;
        public string ProgressBarClass => OverallProgress >= 75 ? "success" : OverallProgress >= 50 ? "warning" : "info";
        public string HealthStatus => OverdueTasks == 0 ? "healthy" : OverdueTasks <= 2 ? "caution" : "warning";
    }
}
