using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Burning
{
    public class BurnProjectionModel
    {
        public long ProjectedAnnualBurn { get; set; }
        public long ProjectedTotalBurn5Years { get; set; }
        public decimal EstimatedSupplyReduction { get; set; }
        public List<BurnProjectionDataPoint> ProjectionData { get; set; } = new();
    }
}
