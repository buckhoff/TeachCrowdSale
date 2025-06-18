using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models.Response;
using TeachCrowdSale.Core.Models.Liquidity;
using TeachCrowdSale.Core.Models;
using TeachCrowdSale.Core.Helper;

namespace TeachCrowdSale.Web.Services
{
    /// <summary>
    /// Web service for liquidity dashboard operations
    /// Handles HTTP calls to API and transforms Response models to Web Models
    /// Follows established TeachToken patterns for caching and error handling
    /// </summary>
    public class LiquidityDashboardService : ILiquidityDashboardService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<LiquidityDashboardService> _logger;

        // Cache duration constants following established TeachToken patterns
        private readonly TimeSpan _shortCacheDuration = TimeSpan.FromSeconds(30);    // User data
        private readonly TimeSpan _mediumCacheDuration = TimeSpan.FromMinutes(2);     // Pool data
        private readonly TimeSpan _longCacheDuration = TimeSpan.FromMinutes(10);      // Static data

        // Cache key constants
        private const string LIQUIDITY_STATS_CACHE_KEY = "liquidity_stats";
        private const string ACTIVE_POOLS_CACHE_KEY = "active_liquidity_pools";
        private const string DEX_CONFIGURATIONS_CACHE_KEY = "dex_configurations";
        private const string USER_LIQUIDITY_INFO_PREFIX = "user_liquidity_info_";
        private const string USER_POSITIONS_PREFIX = "user_liquidity_positions_";
        private const string USER_TRANSACTIONS_PREFIX = "user_liquidity_transactions_";
        private const string POOL_DETAILS_PREFIX = "liquidity_pool_details_";
        private const string DASHBOARD_DATA_CACHE_KEY = "liquidity_dashboard_data";
        private const string LIQUIDITY_ANALYTICS_CACHE_KEY = "liquidity_analytics";
        private const string GUIDE_STEPS_CACHE_KEY = "liquidity_guide_steps";

        // Retry configuration
        private const int MAX_RETRY_ATTEMPTS = 3;
        private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(1);

        public LiquidityDashboardService(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            ILogger<LiquidityDashboardService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("TeachAPI");
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Dashboard Overview

        /// <summary>
        /// Get comprehensive liquidity dashboard data
        /// </summary>
        public async Task<LiquidityPageDataModel?> GetLiquidityPageDataAsync()
        {
            return await GetDataWithCacheAndFallbackAsync<LiquidityPageDataResponse, LiquidityPageDataModel>(
                "api/liquidity/data",
                DASHBOARD_DATA_CACHE_KEY,
                _mediumCacheDuration,
                MapToLiquidityPageDataModel,
                GetFallbackLiquidityPageData
            );
        }

        /// <summary>
        /// Get liquidity statistics overview
        /// </summary>
        public async Task<LiquidityStatsModel?> GetLiquidityStatsAsync()
        {
            return await GetDataWithCacheAndFallbackAsync<LiquidityStatsResponse, LiquidityStatsModel>(
                "api/liquidity/stats",
                LIQUIDITY_STATS_CACHE_KEY,
                _mediumCacheDuration,
                MapToLiquidityStatsModel,
                GetFallbackLiquidityStats
            );
        }

        /// <summary>
        /// Get liquidity analytics data
        /// </summary>
        public async Task<LiquidityAnalyticsModel?> GetLiquidityAnalyticsAsync()
        {
            return await GetDataWithCacheAndFallbackAsync<LiquidityAnalyticsResponse, LiquidityAnalyticsModel>(
                "api/liquidity/analytics",
                LIQUIDITY_ANALYTICS_CACHE_KEY,
                _mediumCacheDuration,
                MapToLiquidityAnalyticsModel,
                GetFallbackLiquidityAnalytics
            );
        }

        #endregion

        #region Pool Management

        /// <summary>
        /// Get active liquidity pools
        /// </summary>
        public async Task<List<LiquidityPoolModel>?> GetActiveLiquidityPoolsAsync()
        {
            return await GetDataWithCacheAndFallbackAsync<List<LiquidityPoolResponse>, List<LiquidityPoolModel>>(
                "api/liquidity/pools?includeInactive=false",
                ACTIVE_POOLS_CACHE_KEY,
                _mediumCacheDuration,
                MapToLiquidityPoolModels,
                GetFallbackLiquidityPools
            );
        }

        /// <summary>
        /// Get specific pool details
        /// </summary>
        public async Task<LiquidityPoolModel?> GetLiquidityPoolDetailsAsync(int poolId)
        {
            var cacheKey = $"{POOL_DETAILS_PREFIX}{poolId}";

            return await GetDataWithCacheAndFallbackAsync<LiquidityPoolResponse, LiquidityPoolModel>(
                $"api/liquidity/pool/{poolId}",
                cacheKey,
                _mediumCacheDuration,
                MapToLiquidityPoolModel,
                () => GetFallbackPoolDetails(poolId)
            );
        }

        /// <summary>
        /// Get DEX configuration options
        /// </summary>
        public async Task<List<DexConfigurationModel>?> GetDexConfigurationsAsync()
        {
            return await GetDataWithCacheAndFallbackAsync<List<DexConfigurationResponse>, List<DexConfigurationModel>>(
                "api/liquidity/dex-configs",
                DEX_CONFIGURATIONS_CACHE_KEY,
                _longCacheDuration,
                MapToDexConfigurationModels,
                GetFallbackDexConfigurations
            );
        }

        #endregion

        #region User-Specific Data

        /// <summary>
        /// Get user's liquidity positions
        /// </summary>
        public async Task<List<UserLiquidityPositionModel>?> GetUserLiquidityPositionsAsync(string walletAddress)
        {
            if (string.IsNullOrWhiteSpace(walletAddress))
            {
                return new List<UserLiquidityPositionModel>();
            }

            var cacheKey = $"{USER_POSITIONS_PREFIX}{walletAddress.ToLowerInvariant()}";

            return await GetDataWithCacheAndFallbackAsync<List<UserLiquidityPositionResponse>, List<UserLiquidityPositionModel>>(
                $"api/liquidity/user/{walletAddress}/positions",
                cacheKey,
                _shortCacheDuration,
                MapToUserLiquidityPositionModels,
                () => new List<UserLiquidityPositionModel>()
            );
        }

        /// <summary>
        /// Get user's comprehensive liquidity information
        /// </summary>
        public async Task<UserLiquidityInfoModel?> GetUserLiquidityInfoAsync(string walletAddress)
        {
            if (string.IsNullOrWhiteSpace(walletAddress))
            {
                return null;
            }

            var cacheKey = $"{USER_LIQUIDITY_INFO_PREFIX}{walletAddress.ToLowerInvariant()}";

            return await GetDataWithCacheAndFallbackAsync<UserLiquidityInfoResponse, UserLiquidityInfoModel>(
                $"api/liquidity/user/{walletAddress}",
                cacheKey,
                _shortCacheDuration,
                MapToUserLiquidityInfoModel,
                () => GetFallbackUserInfo(walletAddress)
            );
        }

        /// <summary>
        /// Get user's transaction history
        /// </summary>
        public async Task<List<LiquidityTransactionHistoryModel>?> GetUserTransactionHistoryAsync(string walletAddress, int page = 1, int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(walletAddress))
            {
                return new List<LiquidityTransactionHistoryModel>();
            }

            var cacheKey = $"{USER_TRANSACTIONS_PREFIX}{walletAddress.ToLowerInvariant()}_{page}_{pageSize}";

            return await GetDataWithCacheAndFallbackAsync<List<LiquidityTransactionHistoryResponse>, List<LiquidityTransactionHistoryModel>>(
                $"api/liquidity/user/{walletAddress}/transactions?page={page}&pageSize={pageSize}",
                cacheKey,
                _shortCacheDuration,
                MapToLiquidityTransactionHistoryModels,
                () => new List<LiquidityTransactionHistoryModel>()
            );
        }

        #endregion

        #region Calculations and Previews

        /// <summary>
        /// Calculate liquidity addition preview
        /// </summary>
        public async Task<LiquidityCalculatorModel?> CalculateAddLiquidityPreviewAsync(
            string walletAddress,
            int poolId,
            decimal token0Amount,
            decimal? token1Amount = null,
            decimal slippageTolerance = 0.5m)
        {
            if (string.IsNullOrWhiteSpace(walletAddress))
            {
                return null;
            }

            var queryParams = $"walletAddress={walletAddress}&poolId={poolId}&token0Amount={token0Amount}&slippageTolerance={slippageTolerance}";
            if (token1Amount.HasValue)
            {
                queryParams += $"&token1Amount={token1Amount.Value}";
            }

            return await GetDataWithCacheAndFallbackAsync<LiquidityCalculationResponse, LiquidityCalculatorModel>(
                $"api/liquidity/calculate/add?{queryParams}",
                $"calc_add_{walletAddress}_{poolId}_{token0Amount}",
                TimeSpan.FromSeconds(30), // Short cache for calculations
                MapToLiquidityCalculatorModel,
                GetFallbackCalculatorModel
            );
        }

        /// <summary>
        /// Calculate liquidity removal preview
        /// </summary>
        public async Task<LiquidityCalculatorModel?> CalculateRemoveLiquidityPreviewAsync(
            string walletAddress,
            int positionId,
            decimal percentageToRemove)
        {
            if (string.IsNullOrWhiteSpace(walletAddress))
            {
                return null;
            }

            var queryParams = $"walletAddress={walletAddress}&positionId={positionId}&percentage={percentageToRemove}";

            return await GetDataWithCacheAndFallbackAsync<LiquidityCalculationResponse, LiquidityCalculatorModel>(
                $"api/liquidity/calculate/remove?{queryParams}",
                $"calc_remove_{walletAddress}_{positionId}_{percentageToRemove}",
                TimeSpan.FromSeconds(30), // Short cache for calculations
                MapToLiquidityCalculatorModel,
                GetFallbackCalculatorModel
            );
        }

        #endregion

        #region Add Liquidity Wizard Support

        /// <summary>
        /// Get wizard step data for add liquidity flow
        /// </summary>
        public async Task<AddLiquidityModel?> GetAddLiquidityWizardDataAsync(string? walletAddress = null)
        {
            var pools = await GetActiveLiquidityPoolsAsync() ?? new List<LiquidityPoolModel>();
            var dexes = await GetDexConfigurationsAsync() ?? new List<DexConfigurationModel>();

            return new AddLiquidityModel
            {
                WalletAddress = walletAddress,
                AvailablePools = pools,
                RecommendedPools = pools.Where(p => p.IsFeatured).ToList(),
                AvailableDexes = dexes.Where(d => d.IsActive).OrderBy(d => d.SortOrder).ToList()
            };
        }

        /// <summary>
        /// Get liquidity guide steps for specific DEX
        /// </summary>
        public async Task<List<LiquidityGuideStepModel>?> GetLiquidityGuideStepsAsync(string? dexName = null)
        {
            var queryParam = !string.IsNullOrEmpty(dexName) ? $"?dexName={dexName}" : "";
            var cacheKey = $"{GUIDE_STEPS_CACHE_KEY}_{dexName ?? "all"}";

            return await GetDataWithCacheAndFallbackAsync<List<LiquidityGuideStepResponse>, List<LiquidityGuideStepModel>>(
                $"api/liquidity/guide{queryParam}",
                cacheKey,
                _longCacheDuration,
                MapToLiquidityGuideStepModels,
                () => GetFallbackGuideSteps()
            );
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Generic method for API calls with caching and fallback
        /// Follows established TeachToken error handling patterns
        /// </summary>
        private async Task<TModel?> GetDataWithCacheAndFallbackAsync<TResponse, TModel>(
            string endpoint,
            string cacheKey,
            TimeSpan cacheDuration,
            Func<TResponse, TModel> mapper,
            Func<TModel> fallbackProvider)
            where TModel : class
        {
            // Check cache first
            if (_cache.TryGetValue(cacheKey, out TModel? cachedData) && cachedData != null)
            {
                return cachedData;
            }

            // Try API with retry logic
            for (int attempt = 1; attempt <= MAX_RETRY_ATTEMPTS; attempt++)
            {
                try
                {
                    var response = await _httpClient.GetAsync(endpoint);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var apiData = JsonSerializer.Deserialize<TResponse>(content, GetJsonOptions());

                        if (apiData != null)
                        {
                            var mappedData = mapper(apiData);

                            // Cache successful response
                            _cache.Set(cacheKey, mappedData, cacheDuration);

                            if (attempt > 1)
                            {
                                _logger.LogInformation("Successfully retrieved {Endpoint} after {Attempt} attempts", endpoint, attempt);
                            }

                            return mappedData;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("API request failed for {Endpoint} with status {StatusCode} on attempt {Attempt}",
                            endpoint, response.StatusCode, attempt);
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning(ex, "HTTP request exception for {Endpoint} on attempt {Attempt}", endpoint, attempt);
                }
                catch (TaskCanceledException ex)
                {
                    _logger.LogWarning(ex, "Request timeout for {Endpoint} on attempt {Attempt}", endpoint, attempt);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error for {Endpoint} on attempt {Attempt}", endpoint, attempt);
                }

                // Wait before retry (except on last attempt)
                if (attempt < MAX_RETRY_ATTEMPTS)
                {
                    await Task.Delay(_retryDelay * attempt); // Exponential backoff
                }
            }

            // All attempts failed, use fallback
            _logger.LogWarning("All API attempts failed for {Endpoint}, using fallback data", endpoint);

            var fallback = fallbackProvider();

            // Cache fallback data with shorter duration
            _cache.Set(cacheKey, fallback, TimeSpan.FromMinutes(1));

            return fallback;
        }

        /// <summary>
        /// JSON serialization options for API responses
        /// </summary>
        private static JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        #endregion

        #region Response to Model Mapping

        /// <summary>
        /// Map LiquidityPageDataResponse to LiquidityPageDataModel
        /// </summary>
        private LiquidityPageDataModel MapToLiquidityPageDataModel(LiquidityPageDataResponse response)
        {
            return new LiquidityPageDataModel
            {
                LiquidityPools = response.LiquidityPools?.Select(MapToLiquidityPoolModel).ToList() ?? new(),
                DexOptions = response.DexOptions?.Select(MapToDexConfigurationModel).ToList() ?? new(),
                Stats = response.Stats != null ? MapToLiquidityStatsModel(response.Stats) : new(),
                Analytics = response.Analytics != null ? MapToLiquidityAnalyticsModel(response.Analytics) : new(),
                GuideSteps = response.GuideSteps?.Select(MapToLiquidityGuideStepModel).ToList() ?? new(),
                LoadedAt = response.LoadedAt
            };
        }

        /// <summary>
        /// Map LiquidityStatsResponse to LiquidityStatsModel
        /// Maps from API Response model to Web Display model
        /// </summary>
        private LiquidityStatsModel MapToLiquidityStatsModel(LiquidityStatsResponse response)
        {
            return new LiquidityStatsModel
            {
                // Map basic volume and TVL metrics
                TotalValueLocked = response.TotalValueLocked,
                Volume24h = response.TotalVolume24h,

                // Set default values for properties not in response
                Volume7d = 0m, // Not available in response
                Volume30d = 0m, // Not available in response

                // Map user and pool counts
                TotalUsers = response.TotalLiquidityProviders,
                ActiveUsers = response.TotalLiquidityProviders, // Assume same as total for now
                TotalPools = response.ActivePools,
                ActivePools = response.ActivePools,

                // Map TEACH-specific metrics
                TeachTotalLiquidity = 0m, // Not available in response
                TeachPoolCount = 0, // Not available in response  
                TeachAverageAPY = response.AverageAPY,
                TeachCurrentPrice = response.TeachPrice,
                TeachPriceChange24h = ParsePriceChange(response.PriceChangeDisplay),
                TeachVolume24h = 0m, // Not available in response

                // Top performing pools - not available in response, set to null
                HighestAPYPool = null,
                HighestTVLPool = null,
                HighestVolumePool = null
            };
        }


        ////// <summary>
        /// Map LiquidityAnalyticsResponse to LiquidityAnalyticsModel
        /// Direct mapping from API response lists to web model aggregated metrics
        /// </summary>
        private LiquidityAnalyticsModel MapToLiquidityAnalyticsModel(LiquidityAnalyticsResponse response)
        {
            // Extract summary metrics from the response lists
            var totalTvl = response.TvlTrends?.LastOrDefault()?.TotalValueLocked ?? 0m;
            var previousTvl = response.TvlTrends?.SkipLast(1).LastOrDefault()?.TotalValueLocked ?? 0m;
            var tvlChange = previousTvl > 0 ? ((totalTvl - previousTvl) / previousTvl) * 100 : 0m;

            var totalVolume = response.VolumeTrends?.Sum(v => v.Volume) ?? 0m;
            var lastVolume = response.VolumeTrends?.LastOrDefault()?.Volume ?? 0m;
            var previousVolume = response.VolumeTrends?.SkipLast(1).LastOrDefault()?.Volume ?? 0m;
            var volumeChange = previousVolume > 0 ? ((lastVolume - previousVolume) / previousVolume) * 100 : 0m;

            var avgApy = response.PoolPerformance?.Where(p => p.APY > 0).Average(p => p.APY) ?? 0m;
            var teachPools = response.PoolPerformance?.Where(p => p.Token0Address == "TEACH" || p.Token1Address == "TEACH").ToList() ?? new List<PoolPerformanceDataResponse>();
            var teachTvl = teachPools.Sum(p => p.TotalValueLocked);
            var teachVolume = teachPools.Sum(p => p.Volume24h);

            return new LiquidityAnalyticsModel
            {
                // Time range - use current month as default
                AnalysisPeriod = "30d",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,

                // Market overview metrics extracted from response lists
                TotalMarketTVL = totalTvl,
                MarketTVLChange = tvlChange,
                TotalMarketVolume = totalVolume,
                MarketVolumeChange = volumeChange,
                AverageMarketAPY = avgApy,
                MarketAPYChange = 0m, // Not available from trends

                // TEACH token specific analytics from filtered pool performance
                TeachTotalTVL = teachTvl,
                TeachTVLMarketShare = totalTvl > 0 ? (teachTvl / totalTvl) * 100 : 0m,
                TeachVolume = teachVolume,
                TeachVolumeShare = totalVolume > 0 ? (teachVolume / totalVolume) * 100 : 0m,
                TeachPrice = 0m, // Not available in analytics response
                TeachPriceChange = 0m, // Not available in analytics response
                TeachVolatility = 0m, // Not available in analytics response

                // Pool performance analytics - top lists from response
                TopPerformingPools = response.PoolPerformance?
                    .OrderByDescending(p => p.APY)
                    .Take(5)
                    .Select(p => new PoolPerformanceModel
                    {
                        PoolId = p.PoolId,
                        PoolName = p.PoolName,
                        APY = p.APY,
                        TVL = p.TotalValueLocked,
                        Volume24h = p.Volume24h
                    }).ToList() ?? new List<PoolPerformanceModel>(),

                UnderperformingPools = response.PoolPerformance?
                    .OrderBy(p => p.APY)
                    .Take(5)
                    .Select(p => new PoolPerformanceModel
                    {
                        PoolId = p.PoolId,
                        PoolName = p.PoolName,
                        APY = p.APY,
                        TVL = p.TotalValueLocked,
                        Volume24h = p.Volume24h
                    }).ToList() ?? new List<PoolPerformanceModel>(),

                NewPools = response.PoolPerformance?
                    .OrderByDescending(p => p.LastUpdated)
                    .Take(5)
                    .Select(p => new PoolPerformanceModel
                    {
                        PoolId = p.PoolId,
                        PoolName = p.PoolName,
                        APY = p.APY,
                        TVL = p.TotalValueLocked,
                        Volume24h = p.Volume24h
                    }).ToList() ?? new List<PoolPerformanceModel>(),

                TrendingPools = response.PoolPerformance?
                    .OrderByDescending(p => p.Volume24h)
                    .Take(5)
                    .Select(p => new PoolPerformanceModel
                    {
                        PoolId = p.PoolId,
                        PoolName = p.PoolName,
                        APY = p.APY,
                        TVL = p.TotalValueLocked,
                        Volume24h = p.Volume24h
                    }).ToList() ?? new List<PoolPerformanceModel>(),

                // DEX analytics - aggregated from pool performance by DEX
                DexPerformance = response.PoolPerformance?
                    .GroupBy(p => p.DexName)
                    .Select(g => new DexPerformanceModel
                    {
                        DexName = g.Key,
                        TotalTVL = g.Sum(p => p.TotalValueLocked),
                         Volume24h = g.Sum(p => p.Volume24h),
                        AverageAPY = g.Average(p => p.APY),
                        PoolCount = g.Count(),
                        MarketShare = totalTvl > 0 ? (g.Sum(p => p.TotalValueLocked) / totalTvl) * 100 : 0m
                    }).ToList() ?? new List<DexPerformanceModel>(),

                DexMarketShare = response.PoolPerformance?
                    .GroupBy(p => p.DexName)
                    .ToDictionary(
                        g => g.Key,
                        g => totalTvl > 0 ? (g.Sum(p => p.TotalValueLocked) / totalTvl) * 100 : 0m
                    ) ?? new Dictionary<string, decimal>(),

                DexGrowthRates = response.PoolPerformance?
                    .GroupBy(p => p.DexName)
                    .ToDictionary(
                        g => g.Key,
                        g => 0m // Growth rates not available from current response
                    ) ?? new Dictionary<string, decimal>(),

                // User behavior analytics from top providers
                TotalUniqueUsers = response.TopProviders?.Count ?? 0,
                NewUsers = 0, // Not available in current response
                ActiveUsers = response.TopProviders?.Count(p => p.ActivePositions > 0) ?? 0,
                UserRetentionRate = 0m, // Not available in current response
                AveragePositionSize = response.TopProviders?.Where(p => p.ActivePositions > 0).Average(p => p.TotalValueProvided) ?? 0m,
                AverageHoldDuration = 0m, // Not available in current response

                // Risk analytics - calculated from pool performance variance
                MarketRiskScore = response.PoolPerformance?.Any() == true ?
                    response.PoolPerformance.Select(p => p.APY).StandardDeviation() : 0m,
                AverageImpermanentLoss = 0m, // Not available in current response
                HighRiskPoolsCount = response.PoolPerformance?.Count(p => p.APY > 100) ?? 0,

                RiskDistribution = new Dictionary<string, int>
                {
                    ["Low"] = response.PoolPerformance?.Count(p => p.APY <= 20) ?? 0,
                    ["Medium"] = response.PoolPerformance?.Count(p => p.APY > 20 && p.APY <= 50) ?? 0,
                    ["High"] = response.PoolPerformance?.Count(p => p.APY > 50) ?? 0
                }
            };
        }

        /// <summary>
        /// Map LiquidityPoolResponse to LiquidityPoolModel
        /// </summary>
        private LiquidityPoolModel MapToLiquidityPoolModel(LiquidityPoolResponse response)
        {
            return new LiquidityPoolModel
            {
                Id = response.Id,
                Name = response.Name,
                DexName = response.DexName,
                TokenPair = response.TokenPair,
                PoolAddress = response.PoolAddress,
                Token0Symbol = response.Token0Symbol,
                Token1Symbol = response.Token1Symbol,
                TotalValueLocked = response.TotalValueLocked,
                Volume24h = response.Volume24h,
                FeePercentage = response.FeePercentage,
                APY = response.CurrentAPY,
                IsActive = response.IsActive,
                IsFeatured = response.IsRecommended
            };
        }

        /// <summary>
        /// Map List<LiquidityPoolResponse> to List<LiquidityPoolModel>
        /// </summary>
        private List<LiquidityPoolModel> MapToLiquidityPoolModels(List<LiquidityPoolResponse> responses)
        {
            return responses?.Select(MapToLiquidityPoolModel).ToList() ?? new List<LiquidityPoolModel>();
        }

        /// <summary>
        /// Map DexConfigurationResponse to DexConfigurationModel
        /// </summary>
        private DexConfigurationModel MapToDexConfigurationModel(DexConfigurationResponse response)
        {
            return new DexConfigurationModel
            {
                Id = response.Id,
                Name = response.Name,
                DisplayName = response.DisplayName,
                Description = response.Description,
                LogoUrl = response.LogoUrl,
                BaseUrl = response.BaseUrl,
                IsRecommended = response.IsRecommended,
                IsActive = response.IsActive,
                DefaultFeePercentage = response.DefaultFeePercentage,
                Network = response.Network,
                RouterAddress = response.RouterAddress,
                FactoryAddress = response.FactoryAddress,
                SortOrder = response.SortOrder
            };
        }

        /// <summary>
        /// Map List<DexConfigurationResponse> to List<DexConfigurationModel>
        /// </summary>
        private List<DexConfigurationModel> MapToDexConfigurationModels(List<DexConfigurationResponse> responses)
        {
            return responses?.Select(MapToDexConfigurationModel).ToList() ?? new List<DexConfigurationModel>();
        }

        /// <summary>
        /// Map List<UserLiquidityPositionResponse> to List<UserLiquidityPositionModel>
        /// </summary>
        private List<UserLiquidityPositionModel> MapToUserLiquidityPositionModels(List<UserLiquidityPositionResponse> responses)
        {
            return responses?.Select(MapToUserLiquidityPositionModel).ToList() ?? new List<UserLiquidityPositionModel>();
        }

        /// <summary>
        /// Map UserLiquidityPositionResponse to UserLiquidityPositionModel
        /// </summary>
        private UserLiquidityPositionModel MapToUserLiquidityPositionModel(UserLiquidityPositionResponse response)
        {
            return new UserLiquidityPositionModel
            {
                Id = response.Id,
                PoolId = response.PoolId,
                PoolName = response.PoolName,
                TokenPair = response.TokenPair,
                DexName = response.DexName,
                WalletAddress = response.WalletAddress,
                LpTokenAmount = response.LpTokenAmount,
                Token0Amount = response.Token0Amount,
                Token1Amount = response.Token1Amount,
                InitialValueUsd = response.InitialValueUsd,
                CurrentValueUsd = response.CurrentValueUsd,
                FeesEarnedUsd = response.FeesEarnedUsd,
                ImpermanentLoss = response.ImpermanentLoss,
                NetPnL = response.NetPnL,
                PnLPercentage = response.PnLPercentage,
                Token0Symbol = response.Token0Symbol,
                Token1Symbol = response.Token1Symbol,
                IsActive = response.IsActive,
                AddedAt = response.AddedAt,
                LastUpdatedAt = response.LastUpdatedAt
            };
        }

        /// <summary>
        /// Map UserLiquidityInfoResponse to UserLiquidityInfoModel
        /// Note: UserLiquidityInfoModel is currently in Response namespace, but should be in Liquidity
        /// </summary>
        private UserLiquidityInfoModel MapToUserLiquidityInfoModel(UserLiquidityInfoResponse response)
        {
            return new UserLiquidityInfoModel
            {
                WalletAddress = response.WalletAddress,
                TotalLiquidityValue = response.TotalLiquidityValue,
                TotalFeesEarned = response.TotalFeesEarned,
                TotalPnL = response.TotalPnL,
                TotalPnLPercentage = response.TotalPnLPercentage,
                ActivePositions = response.ActivePositions,
                TotalPositions = response.TotalPositions,
                FirstPositionDate = response.FirstPositionDate,
                Positions = response.Positions?.Select(MapToUserLiquidityPositionModel).ToList() ?? new(),
                RecentTransactions = response.RecentTransactions?.Select(MapToLiquidityTransactionHistoryModel).ToList() ?? new(),
                Stats = response.Stats != null ? MapToUserLiquidityStatsModel(response.Stats) : new()
            };
        }

        /// <summary>
        /// Map List<LiquidityTransactionHistoryResponse> to List<LiquidityTransactionHistoryModel>
        /// </summary>
        private List<LiquidityTransactionHistoryModel> MapToLiquidityTransactionHistoryModels(List<LiquidityTransactionHistoryResponse> responses)
        {
            return responses?.Select(MapToLiquidityTransactionHistoryModel).ToList() ?? new List<LiquidityTransactionHistoryModel>();
        }

        /// <summary>
        /// Map LiquidityTransactionHistoryResponse to LiquidityTransactionHistoryModel
        /// </summary>
        private LiquidityTransactionHistoryModel MapToLiquidityTransactionHistoryModel(LiquidityTransactionHistoryResponse response)
        {
            return new LiquidityTransactionHistoryModel
            {
                Id = response.Id,
                TransactionType = response.TransactionType,
                PoolName = response.PoolName,
                TokenPair = response.TokenPair,
                ValueUsd = response.ValueUsd,
                Timestamp = response.Timestamp,
                TransactionHash = response.TransactionHash,
                Status = response.Status,
               StatusClass = response.Status
            };
        }

        /// <summary>
        /// Map UserLiquidityStatsResponse to UserLiquidityStatsModel
        /// </summary>
        private UserLiquidityStatsModel MapToUserLiquidityStatsModel(UserLiquidityStatsResponse response)
        {
            return new UserLiquidityStatsModel
            {
                WalletAddress = response.WalletAddress,
                DisplayAddress = response.DisplayAddress,
                TotalValueProvided = response.TotalValueProvided,
                TotalFeesEarned = response.TotalFeesEarned,
                ActivePositions = response.ActivePositions,
                FirstPositionDate = response.FirstPositionDate
            };
        }

        /// <summary>
        /// Map LiquidityCalculationResponse to LiquidityCalculatorModel
        /// </summary>
        private LiquidityCalculatorModel MapToLiquidityCalculatorModel(LiquidityCalculationResponse response)
        {
            return new LiquidityCalculatorModel
            {
                PoolId = response.PoolId,
                Token0Amount = response.Token0Amount,
                Token1Amount = response.Token1Amount,
                EstimatedLpTokens = response.EstimatedLpTokens,
                EstimatedValueUsd = response.EstimatedValueUsd,
                EstimatedDailyEarnings = response.EstimatedDailyFees,
                EstimatedAPY = response.EstimatedAPY,
                PriceImpact = response.PriceImpact,
                SlippageTolerance = response.SlippageTolerance
            };
        }

        /// <summary>
        /// Map List<LiquidityGuideStepResponse> to List<LiquidityGuideStepModel>
        /// </summary>
        private List<LiquidityGuideStepModel> MapToLiquidityGuideStepModels(List<LiquidityGuideStepResponse> responses)
        {
            return responses?.Select(MapToLiquidityGuideStepModel).ToList() ?? new List<LiquidityGuideStepModel>();
        }

        /// <summary>
        /// Map LiquidityGuideStepResponse to LiquidityGuideStepModel
        /// </summary>
        private LiquidityGuideStepModel MapToLiquidityGuideStepModel(LiquidityGuideStepResponse response)
        {
            return new LiquidityGuideStepModel
            {
                StepNumber = response.StepNumber,
                Title = response.Title,
                Description = response.Description,
                Icon = response.Icon,
                IsCompleted = response.IsCompleted,
                ActionText = string.Join(", ", response.ActionItems),
                ActionUrl = response.ExternalUrl
            };
        }

        #endregion

        #region Fallback Data Methods

        /// <summary>
        /// Fallback data for liquidity page when API is unavailable
        /// </summary>
        private LiquidityPageDataModel GetFallbackLiquidityPageData()
        {
            return new LiquidityPageDataModel
            {
                LiquidityPools = GetFallbackLiquidityPools(),
                DexOptions = GetFallbackDexConfigurations(),
                Stats = GetFallbackLiquidityStats(),
                Analytics = GetFallbackLiquidityAnalytics(),
                GuideSteps = GetFallbackGuideSteps(),
                LoadedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Fallback liquidity statistics
        /// </summary>
        private LiquidityStatsModel GetFallbackLiquidityStats()
        {
            return new LiquidityStatsModel
            {
                TotalValueLocked = 125_000m,
                Volume24h = 15_000m,
                Volume7d = 89_000m,
                Volume30d = 350_000m,
                TotalPools = 8,
                ActivePools = 6,
                TotalUsers = 234,
                ActiveUsers = 89,
                TeachTotalLiquidity = 45_000m,
                TeachPoolCount = 3,
                TeachAverageAPY = 24.5m,
                TeachCurrentPrice = 0.125m,
                TeachPriceChange24h = 2.3m,
                TeachVolume24h = 8_500m
            };
        }

        /// <summary>
        /// Fallback liquidity pools
        /// </summary>
        private List<LiquidityPoolModel> GetFallbackLiquidityPools()
        {
            return new List<LiquidityPoolModel>
            {
                new LiquidityPoolModel
                {
                    Id = 1,
                    Name = "TEACH/ETH",
                    DexName = "Uniswap V3",
                    TokenPair = "TEACH/ETH",
                    Token0Symbol = "TEACH",
                    Token1Symbol = "ETH",
                    TotalValueLocked = 45_000m,
                    Volume24h = 8_500m,
                    FeePercentage = 0.3m,
                    APY = 28.5m,
                    IsActive = true,
                    IsFeatured = true
                },
                new LiquidityPoolModel
                {
                    Id = 2,
                    Name = "TEACH/USDC",
                    DexName = "Uniswap V3",
                    TokenPair = "TEACH/USDC",
                    Token0Symbol = "TEACH",
                    Token1Symbol = "USDC",
                    TotalValueLocked = 32_000m,
                    Volume24h = 4_200m,
                    FeePercentage = 0.3m,
                    APY = 22.1m,
                    IsActive = true,
                    IsFeatured = true
                }
            };
        }

        /// <summary>
        /// Fallback DEX configurations
        /// </summary>
        private List<DexConfigurationModel> GetFallbackDexConfigurations()
        {
            return new List<DexConfigurationModel>
            {
                new DexConfigurationModel
                {
                    Id = 1,
                    Name = "uniswap",
                    DisplayName = "Uniswap V3",
                    Description = "Leading decentralized exchange",
                    BaseUrl = "https://app.uniswap.org",
                    IsRecommended = true,
                    IsActive = true,
                    DefaultFeePercentage = 0.3m,
                    Network = "Ethereum",
                    SortOrder = 1
                },
                new DexConfigurationModel
                {
                    Id = 2,
                    Name = "sushiswap",
                    DisplayName = "SushiSwap",
                    Description = "Community-driven DEX",
                    BaseUrl = "https://app.sushi.com",
                    IsRecommended = false,
                    IsActive = true,
                    DefaultFeePercentage = 0.3m,
                    Network = "Ethereum",
                    SortOrder = 2
                }
            };
        }

        /// <summary>
        /// Fallback guide steps
        /// </summary>
        private List<LiquidityGuideStepModel> GetFallbackGuideSteps()
        {
            return new List<LiquidityGuideStepModel>
            {
                new LiquidityGuideStepModel
                {
                    StepNumber = 1,
                    Title = "Connect Wallet",
                    Description = "Connect your wallet to get started",
                    Icon = "wallet",
                    ActionText = "Connect",
                    ActionUrl = "#"
                },
                new LiquidityGuideStepModel
                {
                    StepNumber = 2,
                    Title = "Select Pool",
                    Description = "Choose a liquidity pool",
                    Icon = "pool",
                    ActionText = "Browse Pools",
                    ActionUrl = "#"
                },
                new LiquidityGuideStepModel
                {
                    StepNumber = 3,
                    Title = "Add Liquidity",
                    Description = "Deposit tokens to earn fees",
                    Icon = "plus",
                    ActionText = "Add Liquidity",
                    ActionUrl = "#"
                }
            };
        }

        /// <summary>
        /// Fallback calculator model
        /// </summary>
        private LiquidityCalculatorModel GetFallbackCalculatorModel()
        {
            return new LiquidityCalculatorModel
            {
                PoolId = 0,
                Token0Amount = 0,
                Token1Amount = 0,
                EstimatedLpTokens = 0,
                EstimatedValueUsd = 0,
                EstimatedDailyEarnings = 0,
                EstimatedAPY = 0,
                PriceImpact = 0,
                SlippageTolerance = 0.5m
            };
        }

        /// <summary>
        /// Provides fallback liquidity analytics data when API calls fail
        /// Returns LiquidityAnalyticsModel with realistic demo data
        /// </summary>
        public static LiquidityAnalyticsModel GetFallbackLiquidityAnalytics()
        {
            var random = new Random(42);

            return new LiquidityAnalyticsModel
            {
                AnalysisPeriod = "30d",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,

                // Market overview metrics
                TotalMarketTVL = 2_490_000m,
                MarketTVLChange = 75_000m,
                TotalMarketVolume = 408_000m,
                MarketVolumeChange = -12_500m,
                AverageMarketAPY = 42.1m,
                MarketAPYChange = 2.3m,

                // TEACH token specific analytics
                TeachTotalTVL = 1_200_000m,
                TeachTVLMarketShare = 48.2m,
                TeachVolume = 185_000m,
                TeachVolumeShare = 45.3m,
                TeachPrice = 0.065m,
                TeachPriceChange = 4.8m,
                TeachVolatility = 12.5m,

                // Pool performance analytics
                TopPerformingPools = new List<PoolPerformanceModel>
                {
                    new PoolPerformanceModel { PoolName = "TEACH/USDC LP", APY = 45.2m, TVL = 850_000m },
                    new PoolPerformanceModel { PoolName = "TEACH/WETH LP", APY = 38.7m, TVL = 1_200_000m }
                },

                UnderperformingPools = new List<PoolPerformanceModel>(),
                NewPools = new List<PoolPerformanceModel>(),
                TrendingPools = new List<PoolPerformanceModel>(),

                // DEX analytics
                DexPerformance = new List<DexPerformanceModel>
                {
                    new DexPerformanceModel { DexName = "QuickSwap", TotalTVL = 1_200_000m, MarketShare = 48.2m },
                    new DexPerformanceModel { DexName = "Uniswap V3", TotalTVL= 950_000m, MarketShare = 38.1m }
                },

                DexMarketShare = new Dictionary<string, decimal>
                {
                    ["QuickSwap"] = 48.2m,
                    ["Uniswap V3"] = 38.1m,
                    ["SushiSwap"] = 13.7m
                },

                DexGrowthRates = new Dictionary<string, decimal>
                {
                    ["QuickSwap"] = 15.2m,
                    ["Uniswap V3"] = 8.7m,
                    ["SushiSwap"] = -2.1m
                },

                // User behavior analytics
                TotalUniqueUsers = 279,
                NewUsers = 23,
                ActiveUsers = 187,
                UserRetentionRate = 67.0m,
                AveragePositionSize = 8_930m,
                AverageHoldDuration = 28.5m,

                // Risk analytics
                MarketRiskScore = 6.2m,
                AverageImpermanentLoss = 2.1m,
                HighRiskPoolsCount = 2,
                RiskDistribution = new Dictionary<string, int>
                {
                    ["Low"] = 60,
                    ["Medium"] = 25,
                    ["High"] = 12,
                    ["Very High"] = 3
                }
            };
        }

        /// <summary>
        /// Generates fallback DEX comparison data
        /// </summary>
        private static List<DexPerformanceResponse> GetFallbackDexComparison()
        {
            return new List<DexPerformanceResponse>
            {
                new DexPerformanceResponse
                {
                    DexName = "QuickSwap",
                    TotalValueLocked = 1_200_000m,
                    Volume24h = 185_000m,
                    AverageAPY = 42.5m,
                    PoolsCount = 4,
                    LogoUrl = "/images/dex/quickswap.png",
                    MarketShare = 48.2m,
                    CalculatedAt = DateTime.UtcNow
                },
                new DexPerformanceResponse
                {
                    DexName = "Uniswap V3",
                    TotalValueLocked = 950_000m,
                    Volume24h = 145_000m,
                    AverageAPY = 38.9m,
                    PoolsCount = 3,
                    LogoUrl = "/images/dex/uniswap.png",
                    MarketShare = 38.1m,
                    CalculatedAt = DateTime.UtcNow
                },
                new DexPerformanceResponse
                {
                    DexName = "SushiSwap",
                    TotalValueLocked = 340_000m,
                    Volume24h = 78_000m,
                    AverageAPY = 45.1m,
                    PoolsCount = 2,
                    LogoUrl = "/images/dex/sushiswap.png",
                    MarketShare = 13.7m,
                    CalculatedAt = DateTime.UtcNow
                }
            };
        }

        /// <summary>
        /// Fallback user info
        /// </summary>
        private UserLiquidityInfoModel GetFallbackUserInfo(string walletAddress)
        {
            return new UserLiquidityInfoModel
            {
                WalletAddress = walletAddress,
                TotalLiquidityValue = 0m,
                TotalFeesEarned = 0m,
                TotalPnL = 0m,
                TotalPnLPercentage = 0m,
                ActivePositions = 0,
                TotalPositions = 0,
                FirstPositionDate = DateTime.UtcNow,
                Positions = new List<UserLiquidityPositionModel>(),
                RecentTransactions = new List<LiquidityTransactionHistoryModel>(),
                Stats = new UserLiquidityStatsModel
                {
                    WalletAddress = walletAddress,
                    DisplayAddress = walletAddress.Length > 10 ? $"{walletAddress[..6]}...{walletAddress[^4..]}" : walletAddress,
                    TotalValueProvided = 0m,
                    TotalFeesEarned = 0m,
                    ActivePositions = 0,
                    FirstPositionDate = DateTime.UtcNow
                }
            };
        }

        /// <summary>
        /// Fallback pool details
        /// </summary>
        private LiquidityPoolModel GetFallbackPoolDetails(int poolId)
        {
            return new LiquidityPoolModel
            {
                Id = poolId,
                Name = "Pool Not Available",
                DexName = "Unknown",
                TokenPair = "N/A",
                IsActive = false
            };
        }

        #endregion

        #region Cache Management

        /// <summary>
        /// Clear user-specific cache data
        /// </summary>
        public void ClearUserCache(string walletAddress)
        {
            if (string.IsNullOrWhiteSpace(walletAddress)) return;

            var userCacheKeys = new[]
            {
                $"{USER_LIQUIDITY_INFO_PREFIX}{walletAddress.ToLowerInvariant()}",
                $"{USER_POSITIONS_PREFIX}{walletAddress.ToLowerInvariant()}",
                $"{USER_TRANSACTIONS_PREFIX}{walletAddress.ToLowerInvariant()}"
            };

            foreach (var key in userCacheKeys)
            {
                _cache.Remove(key);
            }

            _logger.LogInformation("Cleared cache for user {WalletAddress}", walletAddress);
        }

        /// <summary>
        /// Helper method to parse price change percentage from display string
        /// </summary>
        private decimal ParsePriceChange(string priceChangeDisplay)
        {
            if (string.IsNullOrEmpty(priceChangeDisplay))
                return 0m;

            // Remove percentage sign and any other characters, parse as decimal
            var cleanString = priceChangeDisplay.Replace("%", "").Replace("+", "").Trim();

            if (decimal.TryParse(cleanString, out decimal result))
            {
                return result;
            }

            return 0m;
        }

        /// <summary>
        /// Clear all cached data
        /// </summary>
        public void ClearAllCache()
        {
            var cacheKeys = new[]
            {
                LIQUIDITY_STATS_CACHE_KEY,
                ACTIVE_POOLS_CACHE_KEY,
                DEX_CONFIGURATIONS_CACHE_KEY,
                DASHBOARD_DATA_CACHE_KEY,
                LIQUIDITY_ANALYTICS_CACHE_KEY,
                GUIDE_STEPS_CACHE_KEY
            };

            foreach (var key in cacheKeys)
            {
                _cache.Remove(key);
            }

            _logger.LogInformation("Cleared all liquidity dashboard cache");
        }

        #endregion
    }
}