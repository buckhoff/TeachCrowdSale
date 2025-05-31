using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Data.Entities
{
    public class VolumeHistoryEntry
    {
        public int Id { get; set; }
        public decimal Volume { get; set; }
        public decimal VolumeUsd { get; set; }
        public DateTime Timestamp { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Timeframe { get; set; } = "1h"; // 1h, 1d, 1w
    }
}
