using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Vesting
{/// <summary>
 /// Vesting milestone model for web consumption
 /// Maps from VestingMilestone entity
 /// </summary>
    public class VestingMilestoneModel
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }

        public DateTime Date { get; set; }

        public DateTime Timestamp { get; set; }

        [Range(0, double.MaxValue)]
        public long TokensUnlocked { get; set; }

        [Range(0, double.MaxValue)]
        public long CumulativeUnlocked { get; set; }

        [Range(0, 100)]
        public decimal PercentageUnlocked { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        public bool IsExecuted { get; set; }

        public string? TransactionHash { get; set; }

        public string FormattedDate { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
