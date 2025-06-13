using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models;

namespace TeachCrowdSale.Infrastructure.Services
{
    /// <summary>
    /// Implementation of roadmap-specific caching service
    /// </summary>
    public class RoadmapCacheService : IRoadmapCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly RoadmapCacheSettings _settings;
        private readonly ILogger<RoadmapCacheService> _logger;
        private readonly HashSet<string> _cacheKeys;

        public RoadmapCacheService(
            IMemoryCache memoryCache,
            IOptions<RoadmapCacheSettings> settings,
            ILogger<RoadmapCacheService> logger)
        {
            _memoryCache = memoryCache;
            _settings = settings.Value;
            _logger = logger;
            _cacheKeys = new HashSet<string>();
        }

        public bool IsEnabled => _settings.EnableCaching;

        public Task<T?> GetAsync<T>(string key) where T : class
        {
            if (!IsEnabled)
            {
                return Task.FromResult<T?>(null);
            }

            try
            {
                var value = _memoryCache.Get<T>(key);
                if (value != null)
                {
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                }
                return Task.FromResult(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cached value for key: {Key}", key);
                return Task.FromResult<T?>(null);
            }
        }

        public Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
        {
            if (!IsEnabled || value == null)
            {
                return Task.CompletedTask;
            }

            try
            {
                var options = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration,
                    Size = 1 // Each entry counts as 1 toward the size limit
                };

                _memoryCache.Set(key, value, options);
                _cacheKeys.Add(key);

                _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", key, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cached value for key: {Key}", key);
            }

            return Task.CompletedTask;
        }

        public Task SetShortTermAsync<T>(string key, T value) where T : class
        {
            return SetAsync(key, value, TimeSpan.FromMinutes(_settings.ShortCacheDurationMinutes));
        }

        public Task SetMediumTermAsync<T>(string key, T value) where T : class
        {
            return SetAsync(key, value, TimeSpan.FromMinutes(_settings.MediumCacheDurationMinutes));
        }

        public Task SetLongTermAsync<T>(string key, T value) where T : class
        {
            return SetAsync(key, value, TimeSpan.FromMinutes(_settings.LongCacheDurationMinutes));
        }

        public Task RemoveAsync(string key)
        {
            try
            {
                _memoryCache.Remove(key);
                _cacheKeys.Remove(key);
                _logger.LogDebug("Removed cached value for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cached value for key: {Key}", key);
            }

            return Task.CompletedTask;
        }

        public Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                var keysToRemove = _cacheKeys.Where(key => regex.IsMatch(key)).ToList();

                foreach (var key in keysToRemove)
                {
                    _memoryCache.Remove(key);
                    _cacheKeys.Remove(key);
                }

                _logger.LogDebug("Removed {Count} cached values matching pattern: {Pattern}", keysToRemove.Count, pattern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cached values by pattern: {Pattern}", pattern);
            }

            return Task.CompletedTask;
        }

        public Task ClearAsync()
        {
            try
            {
                foreach (var key in _cacheKeys.ToList())
                {
                    _memoryCache.Remove(key);
                }
                _cacheKeys.Clear();

                _logger.LogInformation("Cleared all roadmap cache entries");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache");
            }

            return Task.CompletedTask;
        }
    }
}
