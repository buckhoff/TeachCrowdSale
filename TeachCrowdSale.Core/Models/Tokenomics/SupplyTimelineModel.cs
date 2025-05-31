using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Tokenomics
{
    public class SupplyTimelineModel
    {
        public DateTime Date { get; set; }
        public string Event { get; set; } = string.Empty;
        public long TokensUnlocked { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
