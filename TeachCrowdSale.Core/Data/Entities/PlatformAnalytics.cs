using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Data.Entities
{
    /// <summary>
    /// Platform analytics tracking for concept site
    /// </summary>
    public class PlatformAnalytics
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string PageUrl { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? SessionId { get; set; }

        [MaxLength(1000)]
        public string? UserAgent { get; set; }

        [MaxLength(500)]
        public string? ReferrerUrl { get; set; }

        [MaxLength(45)]
        public string? UserIP { get; set; }

        public int TimeOnPage { get; set; } = 0; // seconds

        [MaxLength(100)]
        public string? ConversionAction { get; set; } // waitlist_signup, demo_completion, etc.

        [MaxLength(1000)]
        public string? ConversionData { get; set; } // JSON data

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}