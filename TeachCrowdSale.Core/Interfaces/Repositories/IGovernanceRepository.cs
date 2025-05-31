using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Data.Entities;

namespace TeachCrowdSale.Core.Interfaces.Repositories
{
    public interface IGovernanceRepository
    {
        Task<List<GovernanceProposal>> GetProposalsAsync(string? status = null, int? limit = null);
        Task<GovernanceProposal?> GetProposalByIdAsync(int proposalId);
        Task<GovernanceProposal> SaveProposalAsync(GovernanceProposal proposal);
        Task<GovernanceProposal> UpdateProposalAsync(GovernanceProposal proposal);
        Task<List<GovernanceVote>> GetVotesForProposalAsync(int proposalId);
        Task<GovernanceVote> SaveVoteAsync(GovernanceVote vote);
        Task<UtilityMetricsSnapshot?> GetLatestUtilityMetricsAsync();
        Task<UtilityMetricsSnapshot> SaveUtilityMetricsAsync(UtilityMetricsSnapshot metrics);
    }
}
