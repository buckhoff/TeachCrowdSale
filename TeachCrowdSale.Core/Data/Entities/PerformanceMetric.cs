using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Data.Entities
{
    public class PerformanceMetric
    {
        public int Id { get; set; }

        [Required]
        public string MetricName { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty; // Token, Presale, Platform, Treasury

        public decimal Value { get; set; }

        public string? Unit { get; set; }

        public DateTime Timestamp { get; set; }

        public DateTime Date => Timestamp.Date;

        // For percentage changes
        public decimal? PreviousValue { get; set; }
        public decimal? ChangePercentage { get; set; }

        // Metadata
        public string? Description { get; set; }
        public bool IsPublic { get; set; } = true; // Some metrics might be internal only
    }
}
