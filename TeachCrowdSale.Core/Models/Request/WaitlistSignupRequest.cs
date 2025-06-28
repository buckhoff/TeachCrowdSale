using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Request
{
    /// <summary>
    /// Request model for platform waitlist signup
    /// </summary>
    public class WaitlistSignupRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(255, ErrorMessage = "Email must be less than 255 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "User type is required")]
        [StringLength(50)]
        public string UserType { get; set; } = string.Empty; // Educator, Donor, Partner

        [StringLength(200)]
        public string? SchoolDistrict { get; set; }

        [StringLength(100)]
        public string? TeachingSubject { get; set; }

        public bool InterestedInTEACHTokens { get; set; } = false;
        public bool SubscribeToUpdates { get; set; } = true;

        // Optional analytics data
        public string? ReferrerUrl { get; set; }
        public string? SessionId { get; set; }
    }
}
