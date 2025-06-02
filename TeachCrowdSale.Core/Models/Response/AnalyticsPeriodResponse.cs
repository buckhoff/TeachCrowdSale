using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    public class AnalyticsPeriodResponse
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Label { get; set; } = string.Empty;

        // Key metrics for comparison
        public decimal TotalRaised { get; set; }
        public decimal TokensSold { get; set; }
        public int NewParticipants { get; set; }
        public decimal Volume { get; set; }
        public decimal AveragePrice { get; set; }
        public int TransactionsCount { get; set; }
    }
}
