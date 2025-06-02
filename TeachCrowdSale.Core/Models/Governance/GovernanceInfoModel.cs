using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Governance
{
    public class GovernanceInfoModel
    {
        public GovernanceOverviewModel Overview { get; set; } = new();
        public List<GovernanceProposalModel> ActiveProposals { get; set; } = new();
        public List<GovernanceProposalModel> RecentProposals { get; set; } = new();
        public GovernanceStatsModel Stats { get; set; } = new();
        public List<GovernanceFeatureModel> Features { get; set; } = new();
    }
}
