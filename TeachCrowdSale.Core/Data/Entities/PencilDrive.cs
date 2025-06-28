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
    /// Annual pencil drive tracking entity
    /// </summary>
    public class PencilDrive
    {
        public Guid Id { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public int PencilGoal { get; set; } = 2_000_000; // Default 2M pencils

        [Precision(18, 8)]
        public decimal TokensRaised { get; set; } = 0;

        public int PencilsDistributed { get; set; } = 0;
        public int SchoolsApplied { get; set; } = 0;
        public int SchoolsApproved { get; set; } = 0;

        [MaxLength(200)]
        public string? PartnerName { get; set; }

        [MaxLength(500)]
        public string? PartnerLogoUrl { get; set; }

        public int PartnerPencilsCommitted { get; set; } = 500_000; // Default partner contribution
        public int PlatformPencilsCommitted { get; set; } = 500_000; // Default platform contribution

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
