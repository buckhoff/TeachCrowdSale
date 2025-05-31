using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Core.Models.Vesting
{
    public class VestingCategoryModel
    {
        public string Category { get; set; } = string.Empty;
        public long TotalTokens { get; set; }
        public decimal TgePercentage { get; set; }
        public int VestingMonths { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Color { get; set; } = string.Empty;
        public List<VestingMilestoneModel> Milestones { get; set; } = new();
    }
}
