using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Burning
{
    public class BurnProjectionDataPoint
    {
        public DateTime Date { get; set; }
        public long ProjectedBurn { get; set; }
        public long RemainingSupply { get; set; }
    }
}
