using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Data.Entities;

namespace TeachCrowdSale.Core.Interfaces.Repositories
{
    public interface ITokenMetricsRepository
    {
        Task<TokenMetricsSnapshot?> GetLatestMetricsAsync();
        Task<List<TokenMetricsSnapshot>> GetMetricsHistoryAsync(DateTime fromDate, DateTime toDate);
        Task<TokenMetricsSnapshot> SaveMetricsSnapshotAsync(TokenMetricsSnapshot snapshot);
        Task<List<PriceHistoryEntry>> GetPriceHistoryAsync(DateTime fromDate, DateTime toDate, string? source = null);
        Task<PriceHistoryEntry> SavePriceHistoryAsync(PriceHistoryEntry entry);
        Task<List<VolumeHistoryEntry>> GetVolumeHistoryAsync(DateTime fromDate, DateTime toDate);
        Task<VolumeHistoryEntry> SaveVolumeHistoryAsync(VolumeHistoryEntry entry);
    }
}
