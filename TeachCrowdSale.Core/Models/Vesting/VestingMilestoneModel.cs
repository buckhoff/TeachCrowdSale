using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Vesting
{
    public class VestingMilestoneModel
    {
        public DateTime Date { get; set; }
        public long TokensUnlocked { get; set; }
        public long CumulativeUnlocked { get; set; }
        public decimal PercentageUnlocked { get; set; }
    }
}
