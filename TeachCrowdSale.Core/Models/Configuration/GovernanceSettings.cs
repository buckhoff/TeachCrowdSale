using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Configuration
{
    public class GovernanceSettings
    {
        public string MinTokensForProposal { get; set; } = string.Empty;
        public int VotingPeriodDays { get; set; } = 7;
        public decimal QuorumPercentage { get; set; } = 4.0m;
        public int ExecutionDelayDays { get; set; } = 2;
        public string ProposalFeeTokens { get; set; } = string.Empty;
    }
}
