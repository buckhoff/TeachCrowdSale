﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Reward projection model
    /// </summary>
    public class RewardProjectionModel
    {
        public DateTime Date { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CumulativeRewards { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PeriodRewards { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CompoundedAmount { get; set; }
        [Range(0, double.MaxValue)]
        public decimal UserShare { get; set; }
        [Range(0, double.MaxValue)]
        public decimal SchoolShare { get; set; }
        public string FormattedRewards { get; set; } = string.Empty;
        public string FormattedCompounded { get; set; } = string.Empty;
    }

}
