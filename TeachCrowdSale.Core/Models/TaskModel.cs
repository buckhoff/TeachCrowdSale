using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Data.Enum;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Task display model for UI
    /// </summary>
    public class TaskModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string PriorityClass { get; set; } = string.Empty;
        public decimal ProgressPercentage { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public int MilestoneId { get; set; }
        public string MilestoneTitle { get; set; } = string.Empty;
        public string? Assignee { get; set; }
        public decimal? EstimatedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Display helper properties
        public string StartDateFormatted => StartDate?.ToString("MMM dd") ?? "";
        public string DueDateFormatted => DueDate?.ToString("MMM dd") ?? "No due date";
        public string CompletionDateFormatted => CompletionDate?.ToString("MMM dd, yyyy") ?? "";
        public bool IsCompleted => Status.Equals("Completed", StringComparison.OrdinalIgnoreCase);
        public bool IsOverdue => DueDate.HasValue && DueDate < DateTime.UtcNow && !IsCompleted;
        public string TimeTrackingText => EstimatedHours.HasValue && ActualHours.HasValue
            ? $"{ActualHours:F1}h / {EstimatedHours:F1}h"
            : EstimatedHours.HasValue ? $"Est: {EstimatedHours:F1}h" : "";
        public string AssigneeDisplay => !string.IsNullOrEmpty(Assignee) ? Assignee : "Unassigned";
    }
}
