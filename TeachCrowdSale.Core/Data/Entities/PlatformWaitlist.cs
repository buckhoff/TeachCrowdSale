using System.ComponentModel.DataAnnotations;

namespace TeachCrowdSale.Core.Data.Entities
{
    /// <summary>
    /// Platform waitlist signup entity for PencilImpact concept site
    /// </summary>
    public class PlatformWaitlist
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string UserType { get; set; } = string.Empty; // Educator, Donor, Partner

        [MaxLength(200)]
        public string? SchoolDistrict { get; set; }

        [MaxLength(100)]
        public string? TeachingSubject { get; set; }

        public bool InterestedInTEACHTokens { get; set; }
        public bool SubscribeToUpdates { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}