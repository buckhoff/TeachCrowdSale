using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Treasury
{
    public class TreasuryAllocationModel
    {
        public string Category { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal Percentage { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public decimal MonthlyUtilization { get; set; }
        public decimal ProjectedDuration { get; set; }
        public string Color { get; set; } = string.Empty;
    }
}
