using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Governance
{
    public class GovernanceOverviewModel
    {
        public long TotalVotingPower { get; set; }
        public int ActiveProposals { get; set; }
        public int PassedProposals { get; set; }
        public int TotalProposals { get; set; }
        public decimal ParticipationRate { get; set; }
        public long MinimumProposalThreshold { get; set; }
        public int VotingPeriodDays { get; set; }
    }
}
