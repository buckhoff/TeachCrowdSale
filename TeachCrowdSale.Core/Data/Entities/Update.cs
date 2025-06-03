using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Data.Enum;

namespace TeachCrowdSale.Core.Data.Entities
{
    public class Update
    {
        public int Id { get; set; }

        public int MilestoneId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        public UpdateType Type { get; set; }

        [Range(0, 100)]
        public decimal? ProgressChange { get; set; }

        [StringLength(100)]
        public string? AuthorName { get; set; }

        public bool IsPublic { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Milestone Milestone { get; set; } = null!;
    }
}
  