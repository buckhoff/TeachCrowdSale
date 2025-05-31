using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Governance
{
    public class GovernanceStatsModel
    {
        public long TotalVoters { get; set; }
        public decimal AverageParticipation { get; set; }
        public long TotalVotesCast { get; set; }
        public decimal ProposalSuccessRate { get; set; }
        public int DaysToNextProposal { get; set; }
        public long LargestVotingPower { get; set; }
    }
}
