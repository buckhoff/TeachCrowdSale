using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Data.Entities;

namespace TeachCrowdSale.Core.Interfaces.Repositories
{
    public interface ISupplyRepository
    {
        Task<SupplySnapshot?> GetLatestSupplySnapshotAsync();
        Task<List<SupplySnapshot>> GetSupplyHistoryAsync(DateTime fromDate, DateTime toDate);
        Task<SupplySnapshot> SaveSupplySnapshotAsync(SupplySnapshot snapshot);
        Task<List<TokenAllocation>> GetTokenAllocationsAsync();
        Task<TokenAllocation> SaveTokenAllocationAsync(TokenAllocation allocation);
        Task<TokenAllocation> UpdateTokenAllocationAsync(TokenAllocation allocation);
    }
}
