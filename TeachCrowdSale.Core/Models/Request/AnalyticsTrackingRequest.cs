using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Request
{
    public class AnalyticsTrackingRequest
    {
        public string PageUrl { get; set; } = string.Empty;
        public string? Action { get; set; }
        public object? Data { get; set; }
    }
}
