using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Data.Entities
{
    /// <summary>
    /// Demo project entity for interactive platform showcase
    /// </summary>
    public class DemoProject
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty; // STEM, Arts, Literacy, etc.

        [Required]
        [Precision(10, 2)]
        public decimal FundingGoal { get; set; }

        [Precision(10, 2)]
        public decimal CurrentFunding { get; set; } = 0;

        [Required]
        [MaxLength(200)]
        public string SchoolName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string TeacherName { get; set; } = string.Empty;

        [MaxLength(2)]
        public string State { get; set; } = string.Empty;

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        public int StudentsImpacted { get; set; }

        [MaxLength(50)]
        public string GradeLevel { get; set; } = string.Empty; // K-2, 3-5, 6-8, 9-12

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public DateTime Deadline { get; set; }
        public bool IsUrgent { get; set; } = false; // Deadline within 30 days
        public bool IsFeatured { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
