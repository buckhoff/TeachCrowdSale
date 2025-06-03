using System.ComponentModel.DataAnnotations;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Milestone display model for UI
    /// </summary>
    public class MilestoneDisplayModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string PriorityClass { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }

        [Range(0, 100)]
        public decimal ProgressPercentage { get; set; }

        public string? TechnicalDetails { get; set; }
        public string? GitHubIssueUrl { get; set; }
        public string? DocumentationUrl { get; set; }

        public List<TaskDisplayModel> Tasks { get; set; } = new();
        public List<DependencyDisplayModel> Dependencies { get; set; } = new();
        public List<UpdateDisplayModel> RecentUpdates { get; set; } = new();

        // Calculated fields
        public string DurationEstimate { get; set; } = string.Empty;
        public string TimeRemaining { get; set; } = string.Empty;
        public bool IsOverdue { get; set; }
        public bool IsBlocked { get; set; }
        public int CompletedTasksCount { get; set; }
        public int TotalTasksCount { get; set; }
    }
}

// TeachCrowdSale.Core/Models/Response/TaskDisplayModel.cs
using System.ComponentModel.DataAnnotations;
