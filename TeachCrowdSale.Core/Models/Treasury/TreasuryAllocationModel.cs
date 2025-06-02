using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Treasury
{
    /// <summary>
    /// Treasury allocation model for web consumption
    /// Maps from TreasuryAllocation entity
    /// </summary>
    public class TreasuryAllocationModel
    {
        public int Id { get; set; }

        public int SnapshotId { get; set; }

        public string Category { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Value { get; set; }

        [Range(0, 100)]
        public decimal Percentage { get; set; }

        public string Purpose { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal MonthlyUtilization { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ProjectedDuration { get; set; }

        public string Color { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
