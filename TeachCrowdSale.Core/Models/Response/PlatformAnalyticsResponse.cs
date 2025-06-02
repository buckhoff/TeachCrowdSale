using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    public class PlatformAnalyticsResponse
    {
        [Range(0, double.MaxValue)]
        public decimal TotalValueLocked { get; set; }

        [Range(0, double.MaxValue)]
        public decimal StakedTokens { get; set; }

        [Range(0, double.MaxValue)]
        public decimal RewardsDistributed { get; set; }

        [Range(0, int.MaxValue)]
        public int ActiveStakers { get; set; }

        public decimal AverageStakeAmount => ActiveStakers > 0 ? StakedTokens / ActiveStakers : 0;

        // Education impact metrics
        [Range(0, double.MaxValue)]
        public decimal TotalEducationFunding { get; set; }

        [Range(0, int.MaxValue)]
        public int EducatorsSupported { get; set; }

        [Range(0, int.MaxValue)]
        public int StudentsImpacted { get; set; }

        [Range(0, int.MaxValue)]
        public int SchoolsPartnered { get; set; }

        // Activity metrics
        [Range(0, int.MaxValue)]
        public int TransactionsToday { get; set; }

        [Range(0, int.MaxValue)]
        public int ActiveUsers24h { get; set; }

        [Range(0, int.MaxValue)]
        public int ActiveUsers7d { get; set; }

        // Platform growth
        public decimal PlatformGrowthRate { get; set; }
        public decimal UserRetentionRate { get; set; }
        public decimal AverageSessionDuration { get; set; }
    }
}
