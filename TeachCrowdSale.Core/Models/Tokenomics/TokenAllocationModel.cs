using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Tokenomics
{
    /// <summary>
    /// Token allocation model for web consumption
    /// Maps from TokenAllocation entity
    /// </summary>
    public class TokenAllocationModel
    {
        public int Id { get; set; }

        public string Category { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal TokenAmount { get; set; }

        [Range(0, 100)]
        public decimal Percentage { get; set; }

        public string Color { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime UnlockDate { get; set; }

        [Range(0, 60)]
        public int VestingMonths { get; set; }

        public bool IsLocked { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
