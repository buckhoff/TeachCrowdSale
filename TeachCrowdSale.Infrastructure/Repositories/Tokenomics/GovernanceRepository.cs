using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Interfaces.Repositories;
using TeachCrowdSale.Infrastructure.Data.Context;

namespace TeachCrowdSale.Infrastructure.Repositories.Tokenomics
{
    public class GovernanceRepository : IGovernanceRepository
    {
        private readonly TeachCrowdSaleDbContext _context;

        public GovernanceRepository(TeachCrowdSaleDbContext context)
        {
            _context = context;
        }

        public async Task<List<GovernanceProposal>> GetProposalsAsync(string? status = null, int? limit = null)
        {
            var query = _context.GovernanceProposals.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            query = query.OrderByDescending(p => p.CreatedAt);

            if (limit.HasValue)
                query = query.Take(limit.Value);

            return await query.ToListAsync();
        }

        public async Task<GovernanceProposal?> GetProposalByIdAsync(int proposalId)
        {
            return await _context.GovernanceProposals
                .FirstOrDefaultAsync(p => p.Id == proposalId);
        }

        public async Task<GovernanceProposal> SaveProposalAsync(GovernanceProposal proposal)
        {
            _context.GovernanceProposals.Add(proposal);
            await _context.SaveChangesAsync();
            return proposal;
        }

        public async Task<GovernanceProposal> UpdateProposalAsync(GovernanceProposal proposal)
        {
            _context.GovernanceProposals.Update(proposal);
            await _context.SaveChangesAsync();
            return proposal;
        }

        public async Task<List<GovernanceVote>> GetVotesForProposalAsync(int proposalId)
        {
            return await _context.GovernanceVotes
                .Where(v => v.ProposalId == proposalId)
                .OrderByDescending(v => v.VoteDate)
                .ToListAsync();
        }

        public async Task<GovernanceVote> SaveVoteAsync(GovernanceVote vote)
        {
            _context.GovernanceVotes.Add(vote);
            await _context.SaveChangesAsync();
            return vote;
        }

        public async Task<UtilityMetricsSnapshot?> GetLatestUtilityMetricsAsync()
        {
            return await _context.UtilityMetricsSnapshots
                .Where(s => s.IsLatest)
                .OrderByDescending(s => s.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<UtilityMetricsSnapshot> SaveUtilityMetricsAsync(UtilityMetricsSnapshot metrics)
        {
            await _context.UtilityMetricsSnapshots
                .Where(s => s.IsLatest)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsLatest, false));

            metrics.IsLatest = true;
            _context.UtilityMetricsSnapshots.Add(metrics);
            await _context.SaveChangesAsync();
            return metrics;
        }
    }
}
