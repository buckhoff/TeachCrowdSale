using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Request
{
    /// <summary>
    /// Request model for syncing pool data
    /// </summary>
    public class PoolSyncRequest
    {
        [Range(1, int.MaxValue)]
        public int? PoolId { get; set; }

        [StringLength(50)]
        public string? DexName { get; set; }

        public bool ForceSync { get; set; } = false;

        public bool SyncPricesOnly { get; set; } = false;

        public bool SyncHistoricalData { get; set; } = false;

        [Range(1, 30)]
        public int? HistoricalDays { get; set; } = 7;
    }
}
