using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Tokenomics
{
    public class SupplyMetricsModel
    {
        public decimal MaxSupply { get; set; } = 5_000_000_000;
        public decimal CurrentSupply { get; set; }
        public decimal CirculatingSupply { get; set; }
        public decimal LockedSupply { get; set; }
        public decimal BurnedSupply { get; set; }
        public decimal CirculatingPercent { get; set; }
        public decimal LockedPercent { get; set; }
        public decimal BurnedPercent { get; set; }
    }
}
