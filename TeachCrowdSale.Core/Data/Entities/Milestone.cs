using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Data.Enum;

namespace TeachCrowdSale.Core.Data.Entities
{
    public class Milestone
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        public MilestoneStatus Status { get; set; }

        public MilestonePriority Priority { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EstimatedCompletionDate { get; set; }

        public DateTime? ActualCompletionDate { get; set; }

        [Range(0, 100)]
        public decimal ProgressPercentage { get; set; }

        public int SortOrder { get; set; }

        [StringLength(500)]
        public string? TechnicalDetails { get; set; }

        [StringLength(200)]
        public string? GitHubIssueUrl { get; set; }

        [StringLength(200)]
        public string? DocumentationUrl { get; set; }

        public bool IsPublic { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public List<Task> Tasks { get; set; } = new();
        public List<Dependency> Dependencies { get; set; } = new();
        public List<Update> Updates { get; set; } = new();
    }
}