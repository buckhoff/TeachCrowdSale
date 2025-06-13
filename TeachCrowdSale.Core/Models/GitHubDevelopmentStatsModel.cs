using System.ComponentModel.DataAnnotations;
namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Development statistics model
    /// </summary>
    public class GitHubDevelopmentStatsModel
    {
        [Range(0, int.MaxValue)]
        public int TotalMilestones { get; set; }

        [Range(0, int.MaxValue)]
        public int CompletedMilestones { get; set; }

        [Range(0, int.MaxValue)]
        public int InProgressMilestones { get; set; }

        [Range(0, int.MaxValue)]
        public int PlannedMilestones { get; set; }

        [Range(0, 100)]
        public decimal OverallProgress { get; set; }

        [Range(0, int.MaxValue)]
        public int TotalTasks { get; set; }

        [Range(0, int.MaxValue)]
        public int CompletedTasks { get; set; }

        [Range(0, int.MaxValue)]
        public int BlockedTasks { get; set; }

        [Range(0, 100)]
        public decimal TaskCompletionRate { get; set; }

        [Range(0, int.MaxValue)]
        public int ActiveDevelopers { get; set; }

        [Range(0, double.MaxValue)]
        public decimal AverageCompletionTime { get; set; }

        public string CurrentSprintName { get; set; } = string.Empty;
        public DateTime? CurrentSprintEndDate { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}

