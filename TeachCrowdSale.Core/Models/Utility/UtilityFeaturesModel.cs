using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Utility
{
    public class UtilityFeaturesModel
    {
        public List<UtilityCategoryModel> Categories { get; set; } = new();
        public UtilityMetricsModel Metrics { get; set; } = new();
        public List<UtilityRoadmapModel> Roadmap { get; set; } = new();
    }
}
