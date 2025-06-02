using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    public class PresaleAnalyticsResponse
    {
        [Range(0, double.MaxValue)]
        public decimal TotalRaised { get; set; }

        [Range(0, double.MaxValue)]
        public decimal FundingGoal { get; set; }

        public decimal FundingProgress => FundingGoal > 0 ? (TotalRaised / FundingGoal) * 100 : 0;

        [Range(0, double.MaxValue)]
        public decimal TokensSold { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TokensRemaining { get; set; }

        [Range(0, int.MaxValue)]
        public int ParticipantsCount { get; set; }

        public decimal AverageInvestment => ParticipantsCount > 0 ? TotalRaised / ParticipantsCount : 0;

        // Growth metrics
        public decimal RaisedChange24h { get; set; }
        public decimal RaisedChange7d { get; set; }
        public int NewParticipants24h { get; set; }
        public int NewParticipants7d { get; set; }

        // Current tier info
        public CurrentTierAnalyticsResponse CurrentTier { get; set; } = new();

        // Timeline metrics
        public DateTime PresaleStartDate { get; set; }
        public DateTime? PresaleEndDate { get; set; }
        public int DaysActive => (DateTime.UtcNow - PresaleStartDate).Days;
        public int? DaysRemaining => PresaleEndDate?.Subtract(DateTime.UtcNow).Days;

        // Performance metrics
        public decimal DailyAverageRaised => DaysActive > 0 ? TotalRaised / DaysActive : 0;
        public decimal WeeklyRunRate { get; set; }
        public decimal MonthlyProjection { get; set; }
    }
}
