using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Data.Enum;

namespace TeachCrowdSale.Core.Data.Entities
{
    public class Release
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Version { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public ReleaseType Type { get; set; }

        public ReleaseStatus Status { get; set; }

        public DateTime? PlannedReleaseDate { get; set; }

        public DateTime? ActualReleaseDate { get; set; }

        [StringLength(2000)]
        public string? ReleaseNotes { get; set; }

        [StringLength(200)]
        public string? GitHubReleaseUrl { get; set; }

        [StringLength(200)]
        public string? DocumentationUrl { get; set; }

        public bool IsPublic { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public List<Milestone> Milestones { get; set; } = new();
    }
}
