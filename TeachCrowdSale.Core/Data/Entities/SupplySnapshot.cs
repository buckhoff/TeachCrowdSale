using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Data.Entities
{
    public class SupplySnapshot
    {
        public int Id { get; set; }
        public decimal TotalSupply { get; set; } = 5_000_000_000;
        public decimal CirculatingSupply { get; set; }
        public decimal LockedSupply { get; set; }
        public decimal BurnedSupply { get; set; }
        public decimal CirculatingPercent { get; set; }
        public decimal LockedPercent { get; set; }
        public decimal BurnedPercent { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsLatest { get; set; }
    }
}
