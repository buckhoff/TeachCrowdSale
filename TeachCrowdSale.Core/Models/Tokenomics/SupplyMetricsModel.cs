using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Tokenomics
{
    public class SupplyMetricsModel
    {
        public long MaxSupply { get; set; } = 5_000_000_000;
        public long CurrentSupply { get; set; }
        public long CirculatingSupply { get; set; }
        public long LockedSupply { get; set; }
        public long BurnedSupply { get; set; }
        public decimal CirculatingPercent { get; set; }
        public decimal LockedPercent { get; set; }
        public decimal BurnedPercent { get; set; }
    }
}
