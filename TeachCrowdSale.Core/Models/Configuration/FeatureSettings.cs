using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Configuration
{
    public class FeatureSettings
    {
        public bool StakingEnabled { get; set; } = true;
        public bool LiquidityMiningEnabled { get; set; } = true;
        public bool GovernanceEnabled { get; set; } = true;
        public bool PresaleEnabled { get; set; } = true;
        public bool VestingEnabled { get; set; } = true;
        public bool BurnMechanismEnabled { get; set; } = true;
        public bool CrossChainEnabled { get; set; } = false;
        public bool NFTRewardsEnabled { get; set; } = false;
        public bool ReferralProgramEnabled { get; set; } = true;
    }
}
