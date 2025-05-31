using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Utility
{
    public class UtilityFeatureModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public decimal UsageVolume { get; set; }
        public string UsageUnit { get; set; } = string.Empty;
        public DateTime? LaunchDate { get; set; }
    }
}
