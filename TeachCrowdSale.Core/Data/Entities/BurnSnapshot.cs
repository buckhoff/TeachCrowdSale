using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Data.Entities
{
    public class BurnSnapshot
    {
        public int Id { get; set; }
        public decimal TotalBurned { get; set; }
        public decimal BurnedPercentage { get; set; }
        public decimal BurnedLast30Days { get; set; }
        public decimal BurnRate { get; set; }
        public decimal EstimatedAnnualBurn { get; set; }
        public DateTime LastBurnDate { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsLatest { get; set; }
    }
}
