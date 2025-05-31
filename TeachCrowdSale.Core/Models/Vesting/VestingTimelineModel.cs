using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Vesting
{
    public class VestingTimelineModel
    {
        public DateTime Date { get; set; }
        public long TotalUnlocked { get; set; }
        public Dictionary<string, long> CategoryBreakdown { get; set; } = new();
    }
}
