using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Error handling model for graceful degradation
    /// </summary>
    public class RoadmapErrorModel
    {
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorType { get; set; } = string.Empty; // api, network, data, validation
        public DateTime ErrorTime { get; set; }
        public bool ShowFallbackData { get; set; } = true;
        public string UserFriendlyMessage { get; set; } = "Unable to load latest data. Showing cached information.";
        public string RetryAction { get; set; } = string.Empty;
    }
}
