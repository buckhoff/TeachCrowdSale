using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Data.Entities
{
    public class VestingMilestone
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public DateTime Date { get; set; }
        public long TokensUnlocked { get; set; }
        public long CumulativeUnlocked { get; set; }
        public decimal PercentageUnlocked { get; set; }
        public bool IsExecuted { get; set; }
        public string? TransactionHash { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string FormattedDate { get; set; }
        public decimal Amount { get; set; }
    }
}
