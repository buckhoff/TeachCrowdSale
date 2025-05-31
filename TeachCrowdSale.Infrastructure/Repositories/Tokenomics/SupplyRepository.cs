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
    public class SupplyRepository : ISupplyRepository
    {
        private readonly TeachCrowdSaleDbContext _context;

        public SupplyRepository(TeachCrowdSaleDbContext context)
        {
            _context = context;
        }

        public async Task<SupplySnapshot?> GetLatestSupplySnapshotAsync()
        {
            return await _context.SupplySnapshots
                .Where(s => s.IsLatest)
                .OrderByDescending(s => s.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<List<SupplySnapshot>> GetSupplyHistoryAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.SupplySnapshots
                .Where(s => s.Timestamp >= fromDate && s.Timestamp <= toDate)
                .OrderByDescending(s => s.Timestamp)
                .ToListAsync();
        }

        public async Task<SupplySnapshot> SaveSupplySnapshotAsync(SupplySnapshot snapshot)
        {
            await _context.SupplySnapshots
                .Where(s => s.IsLatest)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsLatest, false));

            snapshot.IsLatest = true;
            _context.SupplySnapshots.Add(snapshot);
            await _context.SaveChangesAsync();
            return snapshot;
        }

        public async Task<List<TokenAllocation>> GetTokenAllocationsAsync()
        {
            return await _context.TokenAllocations
                .OrderBy(a => a.Category)
                .ToListAsync();
        }

        public async Task<TokenAllocation> SaveTokenAllocationAsync(TokenAllocation allocation)
        {
            _context.TokenAllocations.Add(allocation);
            await _context.SaveChangesAsync();
            return allocation;
        }

        public async Task<TokenAllocation> UpdateTokenAllocationAsync(TokenAllocation allocation)
        {
            allocation.UpdatedAt = DateTime.UtcNow;
            _context.TokenAllocations.Update(allocation);
            await _context.SaveChangesAsync();
            return allocation;
        }
    }
}
