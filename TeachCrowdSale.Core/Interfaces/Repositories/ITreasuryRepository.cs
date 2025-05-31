using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Data.Entities;

namespace TeachCrowdSale.Core.Interfaces.Repositories
{
    public interface ITreasuryRepository
    {
        Task<TreasurySnapshot?> GetLatestTreasurySnapshotAsync();
        Task<List<TreasurySnapshot>> GetTreasuryHistoryAsync(DateTime fromDate, DateTime toDate);
        Task<TreasurySnapshot> SaveTreasurySnapshotAsync(TreasurySnapshot snapshot);
        Task<List<TreasuryAllocation>> GetTreasuryAllocationsAsync(int? snapshotId = null);
        Task<TreasuryAllocation> SaveTreasuryAllocationAsync(TreasuryAllocation allocation);
        Task<List<TreasuryTransaction>> GetTreasuryTransactionsAsync(DateTime? fromDate = null, string? category = null);
        Task<TreasuryTransaction> SaveTreasuryTransactionAsync(TreasuryTransaction transaction);
    }
}
