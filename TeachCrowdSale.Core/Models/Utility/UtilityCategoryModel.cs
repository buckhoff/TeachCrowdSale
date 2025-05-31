using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Utility
{
    public class UtilityCategoryModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<UtilityFeatureModel> Features { get; set; } = new();
        public string Icon { get; set; } = string.Empty;
        public bool IsLive { get; set; }
        public DateTime? LaunchDate { get; set; }
    }
}
