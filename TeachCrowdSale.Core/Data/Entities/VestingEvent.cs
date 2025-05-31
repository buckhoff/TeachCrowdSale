using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Data.Entities
{
    public class VestingEvent
    {
        public int Id { get; set; }
        public DateTime UnlockDate { get; set; }
        public long TokensUnlocked { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? TransactionHash { get; set; }
        public bool IsProcessed { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
