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
    public class TreasuryRepository : ITreasuryRepository
    {
        private readonly TeachCrowdSaleDbContext _context;

        public TreasuryRepository(TeachCrowdSaleDbContext context)
        {
            _context = context;
        }

        public async Task<TreasurySnapshot?> GetLatestTreasurySnapshotAsync()
        {
            return await _context.TreasurySnapshots
                .Where(s => s.IsLatest)
                .OrderByDescending(s => s.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<List<TreasurySnapshot>> GetTreasuryHistoryAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.TreasurySnapshots
                .Where(s => s.Timestamp >= fromDate && s.Timestamp <= toDate)
                .OrderByDescending(s => s.Timestamp)
                .ToListAsync();
        }

        public async Task<TreasurySnapshot> SaveTreasurySnapshotAsync(TreasurySnapshot snapshot)
        {
            await _context.TreasurySnapshots
                .Where(s => s.IsLatest)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsLatest, false));

            snapshot.IsLatest = true;
            _context.TreasurySnapshots.Add(snapshot);
            await _context.SaveChangesAsync();
            return snapshot;
        }

        public async Task<List<TreasuryAllocation>> GetTreasuryAllocationsAsync(int? snapshotId = null)
        {
            var query = _context.TreasuryAllocations.AsQueryable();

            if (snapshotId.HasValue)
                query = query.Where(a => a.SnapshotId == snapshotId.Value);
            else
            {
                // Get allocations for latest snapshot
                var latestSnapshot = await GetLatestTreasurySnapshotAsync();
                if (latestSnapshot != null)
                    query = query.Where(a => a.SnapshotId == latestSnapshot.Id);
            }

            return await query.OrderBy(a => a.Category).ToListAsync();
        }

        public async Task<TreasuryAllocation> SaveTreasuryAllocationAsync(TreasuryAllocation allocation)
        {
            _context.TreasuryAllocations.Add(allocation);
            await _context.SaveChangesAsync();
            return allocation;
        }

        public async Task<List<TreasuryTransaction>> GetTreasuryTransactionsAsync(DateTime? fromDate = null, string? category = null)
        {
            var query = _context.TreasuryTransactions.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(t => t.Timestamp >= fromDate.Value);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(t => t.Category == category);

            return await query.OrderByDescending(t => t.Timestamp).ToListAsync();
        }

        public async Task<TreasuryTransaction> SaveTreasuryTransactionAsync(TreasuryTransaction transaction)
        {
            _context.TreasuryTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }
    }
}
