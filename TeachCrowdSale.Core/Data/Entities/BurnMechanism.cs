using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Data.Entities
{
    public class BurnMechanism
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal TriggerPercentage { get; set; }
        public string Frequency { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public long HistoricalBurns { get; set; }
        public string Icon { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastTriggered { get; set; }
    }
}
