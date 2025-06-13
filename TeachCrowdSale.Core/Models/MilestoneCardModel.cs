using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Milestone card display model optimized for grid layouts
    /// </summary>
    public class MilestoneCardModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty; // Truncated for cards
        public string Category { get; set; } = string.Empty;
        public string CategoryClass { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
        public string StatusIcon { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string PriorityClass { get; set; } = string.Empty;

        [Range(0, 100)]
        public decimal ProgressPercentage { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }

        // Card-specific display properties
        public string ProgressBarClass { get; set; } = string.Empty;
        public string TimelineText { get; set; } = string.Empty;
        public string DurationText { get; set; } = string.Empty;
        public bool ShowProgressBar { get; set; } = true;
        public bool ShowDueDate { get; set; } = true;
        public bool IsOverdue { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsNew { get; set; } // Recently created
        public bool HasRecentActivity { get; set; }

        // Task summary for cards
        public int CompletedTasksCount { get; set; }
        public int TotalTasksCount { get; set; }
        public string TasksSummary { get; set; } = string.Empty;

        // Links and actions
        public string? GitHubIssueUrl { get; set; }
        public string? DocumentationUrl { get; set; }
        public bool HasExternalLinks { get; set; }
    }
}
