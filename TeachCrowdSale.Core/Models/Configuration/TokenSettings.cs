using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Configuration
{
    public class TokenSettings
    {
        public string Name { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public int Decimals { get; set; } = 18;
        public string TotalSupply { get; set; } = string.Empty;
        public string MaxSupply { get; set; } = string.Empty;
        public string PresaleAllocation { get; set; } = string.Empty;
        public string LiquidityAllocation { get; set; } = string.Empty;
        public string StakingAllocation { get; set; } = string.Empty;
        public string TeamAllocation { get; set; } = string.Empty;
        public string EcosystemAllocation { get; set; } = string.Empty;
        public string EducationalPartnersAllocation { get; set; } = string.Empty;
        public string ReserveAllocation { get; set; } = string.Empty;
        public bool BurnMechanism { get; set; } = true;
        public bool GovernanceEnabled { get; set; } = true;
    }
}
