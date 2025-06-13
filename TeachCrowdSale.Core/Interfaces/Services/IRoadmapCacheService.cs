using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Interfaces.Services
{
    /// <summary>
    /// Interface for roadmap-specific caching operations
    /// </summary>
    public interface IRoadmapCacheService
    {
        Task<T?> GetAsync<T>(string key) where T : class;
        Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class;
        Task SetShortTermAsync<T>(string key, T value) where T : class;
        Task SetMediumTermAsync<T>(string key, T value) where T : class;
        Task SetLongTermAsync<T>(string key, T value) where T : class;
        Task RemoveAsync(string key);
        Task RemoveByPatternAsync(string pattern);
        Task ClearAsync();
        bool IsEnabled { get; }
    }
}
