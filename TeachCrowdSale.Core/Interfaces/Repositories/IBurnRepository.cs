using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Data.Entities;

namespace TeachCrowdSale.Core.Interfaces.Repositories
{
    public interface IBurnRepository
    {
        Task<BurnSnapshot?> GetLatestBurnSnapshotAsync();
        Task<List<BurnSnapshot>> GetBurnHistoryAsync(DateTime fromDate, DateTime toDate);
        Task<BurnSnapshot> SaveBurnSnapshotAsync(BurnSnapshot snapshot);
        Task<List<BurnEvent>> GetBurnEventsAsync(DateTime? fromDate = null, string? mechanism = null);
        Task<BurnEvent> SaveBurnEventAsync(BurnEvent burnEvent);
        Task<List<BurnMechanism>> GetBurnMechanismsAsync();
        Task<BurnMechanism> SaveBurnMechanismAsync(BurnMechanism mechanism);
        Task<BurnMechanism> UpdateBurnMechanismAsync(BurnMechanism mechanism);
    }
}
