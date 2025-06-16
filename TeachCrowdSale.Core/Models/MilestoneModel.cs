using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Data.Enum;
using TeachCrowdSale.Core.Helper;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Milestone display model for UI
    /// </summary>
    public class MilestoneModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string PriorityClass { get; set; } = string.Empty;
        public decimal ProgressPercentage { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }
        public string? DurationEstimate { get; set; }
        public string TimeRemaining { get; set; } = string.Empty;
        public int CompletedTasksCount { get; set; }
        public int TotalTasksCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<TaskModel>? Tasks { get; set; }
        public List<DependencyModel>? Dependencies { get; set; }
        public List<UpdateModel>? Updates { get; set; }

        // Display helper properties
        public string StartDateFormatted => StartDate?.ToString("MMM dd, yyyy") ?? "Not set";
        public string EstimatedCompletionFormatted => EstimatedCompletionDate?.ToString("MMM dd, yyyy") ?? "TBD";
        public string ActualCompletionFormatted => ActualCompletionDate?.ToString("MMM dd, yyyy") ?? "";
        public string ProgressText => $"{CompletedTasksCount}/{TotalTasksCount} tasks";
        public bool IsCompleted => Status.Equals("Completed", StringComparison.OrdinalIgnoreCase);
        public bool IsInProgress => Status.Equals("In Progress", StringComparison.OrdinalIgnoreCase);
        public bool IsOverdue => EstimatedCompletionDate.HasValue && EstimatedCompletionDate < DateTime.UtcNow && !IsCompleted;
        public string CategoryIcon => DisplayHelpers.GetCategoryIcon(Category);
        public string PriorityBadgeClass => $"priority-badge {PriorityClass}";
        public string StatusBadgeClass => $"status-badge {StatusClass}";
    }
}
