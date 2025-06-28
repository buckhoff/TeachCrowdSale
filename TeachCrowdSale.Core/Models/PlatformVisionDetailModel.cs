using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Display model for PencilImpact vision page
    /// </summary>
    public class PlatformVisionDetailModel
    {
        public string VisionStatement { get; set; } = string.Empty;
        public List<VisionMetric> Metrics { get; set; } = new();
        public List<PlatformFeature> Features { get; set; } = new();
        public TokenIntegrationModel TokenIntegration { get; set; } = new();
        public List<string> Timeline { get; set; } = new();
        public List<string> Roadmap { get; set; } = new();
    }
}
