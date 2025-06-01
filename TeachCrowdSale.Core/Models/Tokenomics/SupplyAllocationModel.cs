using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Tokenomics
{
    public class SupplyAllocationModel
    {
        public string Category { get; set; } = string.Empty;
        public decimal TokenAmount { get; set; }
        public decimal Percentage { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime UnlockDate { get; set; }
        public int VestingMonths { get; set; }
        public bool IsLocked { get; set; }
    }
}
