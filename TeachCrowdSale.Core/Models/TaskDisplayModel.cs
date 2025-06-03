using System.ComponentModel.DataAnnotations;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Task display model for UI
    /// </summary>
    public class TaskDisplayModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string TypeClass { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }

        [Range(0, 100)]
        public decimal ProgressPercentage { get; set; }

        public string? AssignedDeveloper { get; set; }
        public string? GitHubIssueUrl { get; set; }
        public string? PullRequestUrl { get; set; }

        // Calculated fields
        public string DurationEstimate { get; set; } = string.Empty;
        public bool IsOverdue { get; set; }
        public bool IsBlocked { get; set; }
    }
}
