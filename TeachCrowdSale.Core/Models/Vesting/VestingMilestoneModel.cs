using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }
        public string FormattedDate { get; set; }
    }
}
