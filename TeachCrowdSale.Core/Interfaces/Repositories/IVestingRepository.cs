using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Data.Entities;

namespace TeachCrowdSale.Core.Interfaces.Repositories
{
    public interface IVestingRepository
    {
        Task<List<VestingCategory>> GetVestingCategoriesAsync();
        Task<VestingCategory> SaveVestingCategoryAsync(VestingCategory category);
        Task<List<VestingMilestone>> GetVestingMilestonesAsync(int? categoryId = null);
        Task<VestingMilestone> SaveVestingMilestoneAsync(VestingMilestone milestone);
        Task<List<VestingEvent>> GetVestingEventsAsync(DateTime? fromDate = null);
        Task<VestingEvent> SaveVestingEventAsync(VestingEvent vestingEvent);
        Task<List<VestingMilestone>> GetUpcomingMilestonesAsync(int daysAhead = 90);
    }
}
