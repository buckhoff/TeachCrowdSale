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
    public class BurnRepository : IBurnRepository
    {
        private readonly TeachCrowdSaleDbContext _context;

        public BurnRepository(TeachCrowdSaleDbContext context)
        {
            _context = context;
        }

        public async Task<BurnSnapshot?> GetLatestBurnSnapshotAsync()
        {
            return await _context.BurnSnapshots
                .Where(s => s.IsLatest)
                .OrderByDescending(s => s.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<List<BurnSnapshot>> GetBurnHistoryAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.BurnSnapshots
                .Where(s => s.Timestamp >= fromDate && s.Timestamp <= toDate)
                .OrderByDescending(s => s.Timestamp)
                .ToListAsync();
        }

        public async Task<BurnSnapshot> SaveBurnSnapshotAsync(BurnSnapshot snapshot)
        {
            await _context.BurnSnapshots
                .Where(s => s.IsLatest)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsLatest, false));

            snapshot.IsLatest = true;
            _context.BurnSnapshots.Add(snapshot);
            await _context.SaveChangesAsync();
            return snapshot;
        }

        public async Task<List<BurnEvent>> GetBurnEventsAsync(DateTime? fromDate = null, string? mechanism = null)
        {
            var query = _context.BurnEvents.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(e => e.Date >= fromDate.Value);

            if (!string.IsNullOrEmpty(mechanism))
                query = query.Where(e => e.Mechanism == mechanism);

            return await query.OrderByDescending(e => e.Date).ToListAsync();
        }

        public async Task<BurnEvent> SaveBurnEventAsync(BurnEvent burnEvent)
        {
            _context.BurnEvents.Add(burnEvent);
            await _context.SaveChangesAsync();
            return burnEvent;
        }

        public async Task<List<BurnMechanism>> GetBurnMechanismsAsync()
        {
            return await _context.BurnMechanisms
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<BurnMechanism> SaveBurnMechanismAsync(BurnMechanism mechanism)
        {
            _context.BurnMechanisms.Add(mechanism);
            await _context.SaveChangesAsync();
            return mechanism;
        }

        public async Task<BurnMechanism> UpdateBurnMechanismAsync(BurnMechanism mechanism)
        {
            _context.BurnMechanisms.Update(mechanism);
            await _context.SaveChangesAsync();
            return mechanism;
        }
    }
}
