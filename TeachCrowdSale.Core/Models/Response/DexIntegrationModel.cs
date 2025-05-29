using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// DEX integration model
    /// </summary>
    public class DexIntegrationModel
    {
        public string Name { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string WidgetUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime LaunchDate { get; set; }
    }
}
