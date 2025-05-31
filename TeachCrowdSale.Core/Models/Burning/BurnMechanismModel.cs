using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Burning
{
    public class BurnMechanismModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal TriggerPercentage { get; set; }
        public string Frequency { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public long HistoricalBurns { get; set; }
        public string Icon { get; set; } = string.Empty;
    }
}
