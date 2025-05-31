using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Data.Entities
{
    public class VestingCategory
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public long TotalTokens { get; set; }
        public decimal TgePercentage { get; set; }
        public int VestingMonths { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Color { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
