using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    public class AnalyticsChangesReponse
    {
        public decimal TotalRaisedChange { get; set; }
        public decimal TokensSoldChange { get; set; }
        public decimal ParticipantsChange { get; set; }
        public decimal VolumeChange { get; set; }
        public decimal PriceChange { get; set; }
        public decimal TransactionsChange { get; set; }

        // Percentage changes
        public decimal TotalRaisedChangePercent { get; set; }
        public decimal TokensSoldChangePercent { get; set; }
        public decimal ParticipantsChangePercent { get; set; }
        public decimal VolumeChangePercent { get; set; }
        public decimal PriceChangePercent { get; set; }
        public decimal TransactionsChangePercent { get; set; }
    }
}
