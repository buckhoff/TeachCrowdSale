using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Data.Entities
{
    public class AnalyticsSnapshot
    {
        public int Id { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        // Token Metrics
        [Range(0, double.MaxValue)]
        public decimal TokenPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MarketCap { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Volume24h { get; set; }

        public decimal PriceChange24h { get; set; }

        [Range(0, int.MaxValue)]
        public int HoldersCount { get; set; }

        // Presale Metrics
        [Range(0, double.MaxValue)]
        public decimal TotalRaised { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TokensSold { get; set; }

        [Range(0, int.MaxValue)]
        public int ParticipantsCount { get; set; }

        [Range(0, int.MaxValue)]
        public int ActiveTierId { get; set; }

        // Platform Metrics (when platform launches)
        [Range(0, double.MaxValue)]
        public decimal TotalValueLocked { get; set; }

        [Range(0, double.MaxValue)]
        public decimal StakedTokens { get; set; }

        [Range(0, double.MaxValue)]
        public decimal RewardsDistributed { get; set; }

        [Range(0, int.MaxValue)]
        public int ActiveStakers { get; set; }

        // Treasury Metrics
        [Range(0, double.MaxValue)]
        public decimal TreasuryBalance { get; set; }

        [Range(0, double.MaxValue)]
        public decimal StabilityFundBalance { get; set; }

        [Range(0, double.MaxValue)]
        public decimal BurnedTokens { get; set; }

        // Network Activity
        [Range(0, int.MaxValue)]
        public int TransactionsCount24h { get; set; }

        [Range(0, int.MaxValue)]
        public int UniqueUsers24h { get; set; }

        // Create index for efficient time-series queries
        [NotMapped]
        public DateTime Date => Timestamp.Date;

        // Navigation properties
        public List<TierSnapshot> TierSnapshots { get; set; } = new();
    }
}
