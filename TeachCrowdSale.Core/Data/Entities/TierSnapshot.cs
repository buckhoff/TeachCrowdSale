using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Data.Entities
{
    public class TierSnapshot
    {
        public int Id { get; set; }

        public int AnalyticsSnapshotId { get; set; }
        public AnalyticsSnapshot AnalyticsSnapshot { get; set; }

        [Range(0, int.MaxValue)]
        public int TierId { get; set; }

        [Required]
        public string TierName { get; set; } = string.Empty;

        [Range(0.00000001, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Allocation { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Sold { get; set; }

        [Range(0, double.MaxValue)]
        public decimal SoldChange24h { get; set; }

        public bool IsActive { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
