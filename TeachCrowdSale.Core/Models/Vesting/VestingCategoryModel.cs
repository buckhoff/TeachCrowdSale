using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Core.Models.Vesting
{
    /// <summary>
    /// Vesting category model for web consumption
    /// Maps from VestingCategory entity
    /// </summary>
    public class VestingCategoryModel
    {
        public int Id { get; set; }

        public string Category { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public long TotalTokens { get; set; }

        [Range(0, 100)]
        public decimal TgePercentage { get; set; }

        [Range(0, 60)]
        public int VestingMonths { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Color { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<VestingMilestoneModel> Milestones { get; set; } = new();
    }
}
