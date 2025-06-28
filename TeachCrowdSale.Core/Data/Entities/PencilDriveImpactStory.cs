using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Data.Entities
{
    /// <summary>
    /// Success stories from previous pencil drives
    /// </summary>
    public class PencilDriveImpactStory
    {
        public Guid Id { get; set; }

        [Required]
        public Guid PencilDriveId { get; set; }

        [Required]
        [MaxLength(200)]
        public string SchoolName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string TeacherName { get; set; } = string.Empty;

        [MaxLength(2)]
        public string State { get; set; } = string.Empty;

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        public int PencilsReceived { get; set; }

        [Required]
        public int StudentsImpacted { get; set; }

        [Required]
        public string StoryText { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [MaxLength(100)]
        public string? VideoUrl { get; set; }

        public bool IsFeatured { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual PencilDrive PencilDrive { get; set; } = null!;
    }
}