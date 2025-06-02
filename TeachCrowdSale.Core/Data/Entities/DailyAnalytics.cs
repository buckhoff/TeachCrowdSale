using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Data.Entities
{
    public class DailyAnalytics
    {
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        // Daily aggregated metrics
        [Range(0, double.MaxValue)]
        public decimal DailyVolume { get; set; }

        [Range(0, int.MaxValue)]
        public int DailyTransactions { get; set; }

        [Range(0, int.MaxValue)]
        public int NewHolders { get; set; }

        [Range(0, int.MaxValue)]
        public int NewParticipants { get; set; }

        [Range(0, double.MaxValue)]
        public decimal DailyTokensSold { get; set; }

        [Range(0, double.MaxValue)]
        public decimal DailyUsdRaised { get; set; }

        // Price metrics
        public decimal OpenPrice { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal HighPrice { get; set; }
        public decimal LowPrice { get; set; }

        // Platform activity (future)
        [Range(0, double.MaxValue)]
        public decimal DailyRewardsDistributed { get; set; }

        [Range(0, int.MaxValue)]
        public int ActiveEducators { get; set; }

        [Range(0, double.MaxValue)]
        public decimal EducationFundingAmount { get; set; }
    }
}
