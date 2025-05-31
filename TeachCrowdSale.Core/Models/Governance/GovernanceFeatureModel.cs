using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Governance
{
    public class GovernanceFeatureModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsImplemented { get; set; }
        public DateTime? ImplementationDate { get; set; }
        public string Impact { get; set; } = string.Empty;
    }
}
