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
    public class VestingRepository : IVestingRepository
    {
        private readonly TeachCrowdSaleDbContext _context;

        public VestingRepository(TeachCrowdSaleDbContext context)
        {
            _context = context;
        }

        public async Task<List<VestingCategory>> GetVestingCategoriesAsync()
        {
            return await _context.VestingCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Category)
                .ToListAsync();
        }

        public async Task<VestingCategory> SaveVestingCategoryAsync(VestingCategory category)
        {
            _context.VestingCategories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<List<VestingMilestone>> GetVestingMilestonesAsync(int? categoryId = null)
        {
            var query = _context.VestingMilestones.AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(m => m.CategoryId == categoryId.Value);

            return await query.OrderBy(m => m.Date).ToListAsync();
        }

        public async Task<VestingMilestone> SaveVestingMilestoneAsync(VestingMilestone milestone)
        {
            _context.VestingMilestones.Add(milestone);
            await _context.SaveChangesAsync();
            return milestone;
        }

        public async Task<List<VestingEvent>> GetVestingEventsAsync(DateTime? fromDate = null)
        {
            var query = _context.VestingEvents.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(e => e.UnlockDate >= fromDate.Value);

            return await query.OrderBy(e => e.UnlockDate).ToListAsync();
        }

        public async Task<VestingEvent> SaveVestingEventAsync(VestingEvent vestingEvent)
        {
            _context.VestingEvents.Add(vestingEvent);
            await _context.SaveChangesAsync();
            return vestingEvent;
        }

        public async Task<List<VestingMilestone>> GetUpcomingMilestonesAsync(int daysAhead = 90)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);
            return await _context.VestingMilestones
                .Where(m => m.Date >= DateTime.UtcNow && m.Date <= cutoffDate && !m.IsExecuted)
                .OrderBy(m => m.Date)
                .ToListAsync();
        }
    }
}
