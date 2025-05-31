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
    public class TokenMetricsRepository : ITokenMetricsRepository
    {
        private readonly TeachCrowdSaleDbContext _context;

        public TokenMetricsRepository(TeachCrowdSaleDbContext context)
        {
            _context = context;
        }

        public async Task<TokenMetricsSnapshot?> GetLatestMetricsAsync()
        {
            return await _context.TokenMetricsSnapshots
                .Where(s => s.IsLatest)
                .OrderByDescending(s => s.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<List<TokenMetricsSnapshot>> GetMetricsHistoryAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.TokenMetricsSnapshots
                .Where(s => s.Timestamp >= fromDate && s.Timestamp <= toDate)
                .OrderByDescending(s => s.Timestamp)
                .ToListAsync();
        }

        public async Task<TokenMetricsSnapshot> SaveMetricsSnapshotAsync(TokenMetricsSnapshot snapshot)
        {
            // Mark previous snapshots as not latest
            await _context.TokenMetricsSnapshots
                .Where(s => s.IsLatest)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsLatest, false));

            snapshot.IsLatest = true;
            _context.TokenMetricsSnapshots.Add(snapshot);
            await _context.SaveChangesAsync();
            return snapshot;
        }

        public async Task<List<PriceHistoryEntry>> GetPriceHistoryAsync(DateTime fromDate, DateTime toDate, string? source = null)
        {
            var query = _context.PriceHistory
                .Where(p => p.Timestamp >= fromDate && p.Timestamp <= toDate);

            if (!string.IsNullOrEmpty(source))
                query = query.Where(p => p.Source == source);

            return await query.OrderByDescending(p => p.Timestamp).ToListAsync();
        }

        public async Task<PriceHistoryEntry> SavePriceHistoryAsync(PriceHistoryEntry entry)
        {
            _context.PriceHistory.Add(entry);
            await _context.SaveChangesAsync();
            return entry;
        }

        public async Task<List<VolumeHistoryEntry>> GetVolumeHistoryAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.VolumeHistory
                .Where(v => v.Timestamp >= fromDate && v.Timestamp <= toDate)
                .OrderByDescending(v => v.Timestamp)
                .ToListAsync();
        }

        public async Task<VolumeHistoryEntry> SaveVolumeHistoryAsync(VolumeHistoryEntry entry)
        {
            _context.VolumeHistory.Add(entry);
            await _context.SaveChangesAsync();
            return entry;
        }
    }
}
