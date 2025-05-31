using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Treasury
{
    public class TreasuryScenarioModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Probability { get; set; }
        public decimal ImpactOnRunway { get; set; }
        public decimal ProjectedRunway { get; set; }
        public string Severity { get; set; } = string.Empty;
        public List<string> Mitigations { get; set; } = new();
    }
}
