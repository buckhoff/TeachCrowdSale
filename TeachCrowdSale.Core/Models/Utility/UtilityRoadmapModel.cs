using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Utility
{
    public class UtilityRoadmapModel
    {
        public string Feature { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EstimatedLaunch { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public List<string> Benefits { get; set; } = new();
    }
}
