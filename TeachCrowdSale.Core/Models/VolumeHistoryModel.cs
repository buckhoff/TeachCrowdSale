using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Volume history model for web consumption
    /// Maps from VolumeHistoryEntry entity
    /// </summary>
    public class VolumeHistoryModel
    {
        public int Id { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Volume { get; set; }

        [Range(0, double.MaxValue)]
        public decimal VolumeUsd { get; set; }

        public DateTime Timestamp { get; set; }

        public string Source { get; set; } = string.Empty;

        public string Timeframe { get; set; } = "1h";
    }
}
