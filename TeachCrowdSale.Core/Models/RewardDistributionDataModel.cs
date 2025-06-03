using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Reward distribution data
    /// </summary>
    public class RewardDistributionDataModel
    {
        public string Category { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Range(0, 100)]
        public decimal Percentage { get; set; }

        public string Color { get; set; } = string.Empty;
    }
}
