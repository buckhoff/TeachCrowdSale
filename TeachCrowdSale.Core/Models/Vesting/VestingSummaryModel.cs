using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Vesting
{
    public class VestingSummaryModel
    {
        public long TotalVestedTokens { get; set; }
        public long CurrentlyUnlocked { get; set; }
        public long RemainingLocked { get; set; }
        public DateTime NextUnlockDate { get; set; }
        public long NextUnlockAmount { get; set; }
        public decimal AverageVestingPeriod { get; set; }
    }
}
