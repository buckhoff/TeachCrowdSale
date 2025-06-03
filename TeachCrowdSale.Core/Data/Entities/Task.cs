using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Data.Enum;

namespace TeachCrowdSale.Core.Data.Entities
{
    public class Task
    {
        public int Id { get; set; }

        public int MilestoneId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public Enum.TaskStatus Status { get; set; }

        public TaskType Type { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EstimatedCompletionDate { get; set; }

        public DateTime? ActualCompletionDate { get; set; }

        [Range(0, 100)]
        public decimal ProgressPercentage { get; set; }

        [StringLength(100)]
        public string? AssignedDeveloper { get; set; }

        [StringLength(200)]
        public string? GitHubIssueUrl { get; set; }

        [StringLength(200)]
        public string? PullRequestUrl { get; set; }

        public int SortOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Milestone Milestone { get; set; } = null!;
    }
}
  