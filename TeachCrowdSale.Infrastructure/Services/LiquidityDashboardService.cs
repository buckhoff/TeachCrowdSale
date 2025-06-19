using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models.Liquidity;
using TeachCrowdSale.Core.Models.Request;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Infrastructure.Services
{
    /// <summary>
    /// Web service for liquidity dashboard operations using HttpClient to call API
    /// Implements ILiquidityDashboardService interface
    /// Maps API Response models to Web Display models
    /// </summary>
    public class LiquidityDashboardService : ILiquidityDashboardService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<LiquidityDashboardService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        // Cache durations following established patterns
        private readonly TimeSpan _shortCache = TimeSpan.FromMinutes(2);
        private readonly TimeSpan _mediumCache = TimeSpan.FromMinutes(10);
        private readonly TimeSpan _longCache = TimeSpan.FromMinutes(30);

        public LiquidityDashboardService(
            HttpClient httpClient,
            IMemoryCache cache,
            ILogger<LiquidityDashboardService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        #region Dashboard Overview

        /// <summary>
        /// Get comprehensive liquidity dashboard data
        /// Maps from LiquidityPageDataResponse to LiquidityPageDataModel
        /// </summary>
        public async Task<LiquidityPageDataModel?> GetLiquidityPageDataAsync()
        {
            const string cacheKey = "liquidity-page-data";

            if (_cache.TryGetValue(cacheKey, out LiquidityPageDataModel? cached))
                return cached;

            try
            {
                var response = await _httpClient.GetAsync("/api/liquidity/data");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<LiquidityPageDataResponse>(json, _jsonOptions);

                var model = MapToLiquidityPageDataModel(apiResponse!);

                _cache.Set(cacheKey, model, _mediumCache);
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load liquidity page data");
                return GetFallbackLiquidityPageData();
            }
        }

        /// <summary>
        /// Get liquidity statistics overview
        /// Maps from LiquidityStatsResponse to LiquidityStatsModel
        /// </summary>
        public async Task<LiquidityStatsModel?> GetLiquidityStatsAsync()
        {
            const string cacheKey = "liquidity-stats";

            if (_cache.TryGetValue(cacheKey, out LiquidityStatsModel? cached))
                return cached;

            try
            {
                var response = await _httpClient.GetAsync("/api/liquidity/stats");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<LiquidityStatsResponse>(json, _jsonOptions);

                var model = MapToLiquidityStatsModel(apiResponse!);

                _cache.Set(cacheKey, model, _shortCache);
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load liquidity stats");
                return GetFallbackLiquidityStats();
            }
        }

        /// <summary>
        /// Get liquidity analytics data
        /// Maps from LiquidityAnalyticsResponse to LiquidityAnalyticsModel
        /// </summary>
        public async Task<LiquidityAnalyticsModel?> GetLiquidityAnalyticsAsync()
        {
            const string cacheKey = "liquidity-analytics";

            if (_cache.TryGetValue(cacheKey, out LiquidityAnalyticsModel? cached))
                return cached;

            try
            {
                var response = await _httpClient.GetAsync("/api/liquidity/analytics");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<LiquidityAnalyticsResponse>(json, _jsonOptions);

                var model = MapToLiquidityAnalyticsModel(apiResponse!);

                _cache.Set(cacheKey, model, _longCache);
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load liquidity analytics");
                return GetFallbackLiquidityAnalytics();
            }
        }

        #endregion

        #region Pool Management

        /// <summary>
        /// Get active liquidity pools
        /// Maps from List<LiquidityPoolResponse> to List<LiquidityPoolModel>
        /// </summary>
        public async Task<List<LiquidityPoolModel>?> GetActiveLiquidityPoolsAsync()
        {
            const string cacheKey = "active-liquidity-pools";

            if (_cache.TryGetValue(cacheKey, out List<LiquidityPoolModel>? cached))
                return cached;

            try
            {
                var response = await _httpClient.GetAsync("/api/liquidity/pools/active");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var apiResponses = JsonSerializer.Deserialize<List<LiquidityPoolResponse>>(json, _jsonOptions);

                var models = apiResponses?.Select(MapToLiquidityPoolModel).ToList();

                _cache.Set(cacheKey, models, _mediumCache);
                return models;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load active liquidity pools");
                return new List<LiquidityPoolModel>();
            }
        }

        /// <summary>
        /// Get specific pool details
        /// Maps from LiquidityPoolResponse to LiquidityPoolModel
        /// </summary>
        public async Task<LiquidityPoolModel?> GetLiquidityPoolDetailsAsync(int poolId)
        {
            var cacheKey = $"liquidity-pool-{poolId}";

            if (_cache.TryGetValue(cacheKey, out LiquidityPoolModel? cached))
                return cached;

            try
            {
                var response = await _httpClient.GetAsync($"/api/liquidity/pools/{poolId}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<LiquidityPoolResponse>(json, _jsonOptions);

                var model = MapToLiquidityPoolModel(apiResponse!);

                _cache.Set(cacheKey, model, _mediumCache);
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load pool details for pool {PoolId}", poolId);
                return null;
            }
        }

        /// <summary>
        /// Get DEX configuration options
        /// Maps from List<DexConfigurationResponse> to List<DexConfigurationModel>
        /// </summary>
        public async Task<List<DexConfigurationModel>?> GetDexConfigurationsAsync()
        {
            const string cacheKey = "dex-configurations";

            if (_cache.TryGetValue(cacheKey, out List<DexConfigurationModel>? cached))
                return cached;

            try
            {
                var response = await _httpClient.GetAsync("/api/liquidity/dex-configs");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var apiResponses = JsonSerializer.Deserialize<List<DexConfigurationResponse>>(json, _jsonOptions);

                var models = apiResponses?.Select(MapToDexConfigurationModel).ToList();

                _cache.Set(cacheKey, models, _longCache);
                return models;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load DEX configurations");
                return new List<DexConfigurationModel>();
            }
        }

        #endregion

        #region User-Specific Data

        /// <summary>
        /// Get user's liquidity positions
        /// Maps from List<UserLiquidityPositionResponse> to List<UserLiquidityPositionModel>
        /// </summary>
        public async Task<List<UserLiquidityPositionModel>?> GetUserLiquidityPositionsAsync(string walletAddress)
        {
            var cacheKey = $"user-positions-{walletAddress}";

            if (_cache.TryGetValue(cacheKey, out List<UserLiquidityPositionModel>? cached))
                return cached;

            try
            {
                var response = await _httpClient.GetAsync($"/api/liquidity/user/{walletAddress}/positions");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var apiResponses = JsonSerializer.Deserialize<List<UserLiquidityPositionResponse>>(json, _jsonOptions);

                var models = apiResponses?.Select(MapToUserLiquidityPositionModel).ToList();

                _cache.Set(cacheKey, models, _shortCache);
                return models;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load user positions for {WalletAddress}", walletAddress);
                return new List<UserLiquidityPositionModel>();
            }
        }

        /// <summary>
        /// Get user's comprehensive liquidity information
        /// Maps from UserLiquidityInfoResponse to UserLiquidityInfoModel
        /// </summary>
        public async Task<UserLiquidityInfoModel?> GetUserLiquidityInfoAsync(string walletAddress)
        {
            var cacheKey = $"user-liquidity-info-{walletAddress}";

            if (_cache.TryGetValue(cacheKey, out UserLiquidityInfoModel? cached))
                return cached;

            try
            {
                var response = await _httpClient.GetAsync($"/api/liquidity/user/{walletAddress}/info");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<UserLiquidityInfoResponse>(json, _jsonOptions);

                var model = MapToUserLiquidityInfoModel(apiResponse!);

                _cache.Set(cacheKey, model, _shortCache);
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load user liquidity info for {WalletAddress}", walletAddress);
                return GetFallbackUserLiquidityInfo(walletAddress);
            }
        }

        /// <summary>
        /// Get user's transaction history
        /// Maps from List<LiquidityTransactionHistoryResponse> to List<LiquidityTransactionHistoryModel>
        /// </summary>
        public async Task<List<LiquidityTransactionHistoryModel>?> GetUserTransactionHistoryAsync(string walletAddress, int page = 1, int pageSize = 20)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/liquidity/user/{walletAddress}/transactions?page={page}&pageSize={pageSize}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var apiResponses = JsonSerializer.Deserialize<List<LiquidityTransactionHistoryResponse>>(json, _jsonOptions);

                return apiResponses?.Select(MapToTransactionHistoryModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load transaction history for {WalletAddress}", walletAddress);
                return new List<LiquidityTransactionHistoryModel>();
            }
        }

        #endregion

        #region Calculations and Previews

        /// <summary>
        /// Calculate liquidity addition preview
        /// Maps from LiquidityCalculationResponse to LiquidityCalculatorModel
        /// </summary>
        public async Task<LiquidityCalculatorModel?> CalculateAddLiquidityPreviewAsync(
            string walletAddress,
            int poolId,
            decimal token0Amount,
            decimal? token1Amount = null,
            decimal slippageTolerance = 0.5m)
        {
            try
            {
                var request = new LiquidityCalculationRequest
                {
                    WalletAddress = walletAddress,
                    PoolId = poolId,
                    Token0Amount = token0Amount,
                    Token1Amount = token1Amount,
                    SlippageTolerance = slippageTolerance,
                    AutoCalculateToken1 = !token1Amount.HasValue
                };

                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/liquidity/calculate", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<LiquidityCalculationResponse>(responseJson, _jsonOptions);

                return MapToLiquidityCalculatorModel(apiResponse!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate add liquidity preview");
                return null;
            }
        }

        /// <summary>
        /// Calculate liquidity removal preview
        /// Maps from LiquidityCalculationResponse to LiquidityCalculatorModel
        /// </summary>
        public async Task<LiquidityCalculatorModel?> CalculateRemoveLiquidityPreviewAsync(
            string walletAddress,
            int positionId,
            decimal percentageToRemove)
        {
            try
            {
                var request = new RemoveLiquidityCalculationRequest
                {
                    WalletAddress = walletAddress,
                    PositionId = positionId,
                    PercentageToRemove = percentageToRemove
                };

                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/liquidity/calculate-remove", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<LiquidityCalculationResponse>(responseJson, _jsonOptions);

                return MapToLiquidityCalculatorModel(apiResponse!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate remove liquidity preview");
                return null;
            }
        }

        #endregion

        #region Add Liquidity Wizard Support

        /// <summary>
        /// Get wizard step data for add liquidity flow
        /// Combines multiple API calls to populate AddLiquidityModel
        /// </summary>
        public async Task<AddLiquidityModel?> GetAddLiquidityWizardDataAsync(string? walletAddress = null)
        {
            try
            {
                // Get pools and DEX options
                var poolsTask = GetActiveLiquidityPoolsAsync();
                var dexesTask = GetDexConfigurationsAsync();

                await Task.WhenAll(poolsTask, dexesTask);

                var pools = await poolsTask ?? new List<LiquidityPoolModel>();
                var dexes = await dexesTask ?? new List<DexConfigurationModel>();

                var model = new AddLiquidityModel
                {
                    WalletAddress = walletAddress,
                    AvailablePools = pools,
                    RecommendedPools = pools.Where(p => p.IsFeatured).ToList(),
                    AvailableDexes = dexes
                };

                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load add liquidity wizard data");
                return GetFallbackAddLiquidityModel();
            }
        }

        /// <summary>
        /// Get liquidity guide steps for specific DEX
        /// Maps from List<LiquidityGuideStepResponse> to List<LiquidityGuideStepModel>
        /// </summary>
        public async Task<List<LiquidityGuideStepModel>?> GetLiquidityGuideStepsAsync(string? dexName = null)
        {
            const string cacheKey = "liquidity-guide-steps";

            if (_cache.TryGetValue(cacheKey, out List<LiquidityGuideStepModel>? cached))
                return cached;

            try
            {
                var url = "/api/liquidity/guide";
                if (!string.IsNullOrEmpty(dexName))
                    url += $"?dexName={Uri.EscapeDataString(dexName)}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var apiResponses = JsonSerializer.Deserialize<List<LiquidityGuideStepResponse>>(json, _jsonOptions);

                var models = apiResponses?.Select(MapToGuideStepModel).ToList();

                _cache.Set(cacheKey, models, _longCache);
                return models;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load liquidity guide steps");
                return GetFallbackGuideSteps();
            }
        }

        #endregion

        #region Cache Management

        /// <summary>
        /// Clear user-specific cache data
        /// </summary>
        public void ClearUserCache(string walletAddress)
        {
            var userCacheKeys = new[]
            {
                $"user-positions-{walletAddress}",
                $"user-liquidity-info-{walletAddress}"
            };

            foreach (var key in userCacheKeys)
            {
                _cache.Remove(key);
            }
        }

        /// <summary>
        /// Clear all cached data
        /// </summary>
        public void ClearAllCache()
        {
            // Note: IMemoryCache doesn't have a clear all method
            // In a real implementation, you might want to track cache keys
            _logger.LogInformation("Cache clear requested - implement key tracking for complete clear");
        }

        #endregion

        #region Mapping Methods

        private LiquidityPageDataModel MapToLiquidityPageDataModel(LiquidityPageDataResponse response)
        {
            return new LiquidityPageDataModel
            {
                LiquidityPools = response.LiquidityPools.Select(MapToLiquidityPoolModel).ToList(),
                DexOptions = response.DexOptions.Select(MapToDexConfigurationModel).ToList(),
                Stats = MapToLiquidityStatsModel(response.Stats),
                Analytics = MapToLiquidityAnalyticsModel(response.Analytics),
                GuideSteps = response.GuideSteps.Select(MapToGuideStepModel).ToList(),
                LoadedAt = response.LoadedAt
            };
        }

        private LiquidityPoolModel MapToLiquidityPoolModel(LiquidityPoolResponse response)
        {
            return new LiquidityPoolModel
            {
                Id = response.Id,
                Name = response.Name,
                TokenPair = response.TokenPair,
                Token0Symbol = response.Token0Symbol,
                Token1Symbol = response.Token1Symbol,
                DexName = response.DexName,
                PoolAddress = response.PoolAddress,
                APY = response.CurrentAPY,
                TotalValueLocked = response.TotalValueLocked,
                Volume24h = response.Volume24h,
                FeePercentage = response.FeePercentage,
                IsActive = response.IsActive,
                IsFeatured = response.IsRecommended,
                CreatedAt = response.CreatedAt,
                UpdatedAt = response.UpdatedAt
            };
        }

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
                ApiUrl = response.ApiUrl,
                IsRecommended = response.IsRecommended,
                IsActive = response.IsActive,
                DefaultFeePercentage = response.DefaultFeePercentage,
                Network = response.Network,
                RouterAddress = response.RouterAddress,
                FactoryAddress = response.FactoryAddress,
                SortOrder = response.SortOrder
            };
        }

        private LiquidityStatsModel MapToLiquidityStatsModel(LiquidityStatsResponse response)
        {
            return new LiquidityStatsModel
            {
                TotalValueLocked = response.TotalValueLocked,
                Volume24h = response.TotalVolume24h,
                TotalPools = response.ActivePools,
                ActivePools = response.ActivePools,
                TotalUsers = response.TotalLiquidityProviders,
                ActiveUsers = response.TotalLiquidityProviders,
                TeachCurrentPrice = response.TeachPrice,
                TeachPriceChange24h = ParsePriceChange(response.PriceChangeDisplay),
                AveragePoolAPY = response.AverageAPY
            };
        }

        private LiquidityAnalyticsModel MapToLiquidityAnalyticsModel(LiquidityAnalyticsResponse response)
        {
            return new LiquidityAnalyticsModel
            {
                AnalysisPeriod = "30d",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
                TotalMarketTVL = 0, //response.TotalValueLocked,
                TotalMarketVolume =0, // response.TotalVolume24h,
                AverageMarketAPY =0 // response.AverageAPY
            };
        }

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
                Positions = response.Positions.Select(MapToUserLiquidityPositionModel).ToList(),
                RecentTransactions = response.RecentTransactions.Select(MapToTransactionHistoryModel).ToList(),
                Stats = MapToUserLiquidityStatsModel(response.Stats)
            };
        }

        private UserLiquidityPositionModel MapToUserLiquidityPositionModel(UserLiquidityPositionResponse response)
        {
            return new UserLiquidityPositionModel
            {
                Id = response.Id,
                PoolId = response.PoolId,
                PoolName = response.PoolName,
                TokenPair = response.TokenPair,
                Token0Symbol = response.Token0Symbol,
                Token1Symbol = response.Token1Symbol,
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
                IsActive = response.IsActive,
                AddedAt = response.AddedAt,
                LastUpdatedAt = response.LastUpdatedAt
            };
        }

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

        private LiquidityTransactionHistoryModel MapToTransactionHistoryModel(LiquidityTransactionHistoryResponse response)
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
                StatusClass = response.Status.ToLower() switch
                {
                    "confirmed" => "status-success",
                    "pending" => "status-pending",
                    "failed" => "status-error",
                    _ => "status-unknown"
                }
            };
        }

        private LiquidityGuideStepModel MapToGuideStepModel(LiquidityGuideStepResponse response)
        {
            return new LiquidityGuideStepModel
            {
                StepNumber = response.StepNumber,
                Title = response.Title,
                Description = response.Description,
                Icon = response.Icon,
                IsCompleted = response.IsCompleted,
                ActionText = response.ActionText,
                ActionUrl = response.ActionUrl
            };
        }

        private LiquidityCalculatorModel MapToLiquidityCalculatorModel(LiquidityCalculationResponse response)
        {
            return new LiquidityCalculatorModel
            {
                PoolId = response.PoolId,
                TokenPair = response.TokenPair,
                Token0Symbol = response.Token0Symbol,
                Token1Symbol = response.Token1Symbol,
                WalletAddress = response.WalletAddress,
                Token0Amount = response.Token0Amount,
                Token1Amount = response.Token1Amount,
                EstimatedLpTokens = response.EstimatedLpTokens,
                EstimatedValueUsd = response.EstimatedValueUsd,
                PriceImpact = response.PriceImpact,
                EstimatedAPY = response.EstimatedAPY,
                EstimatedDailyEarnings = response.EstimatedDailyFees,
                EstimatedMonthlyEarnings = response.EstimatedMonthlyFees,
                EstimatedYearlyEarnings = response.EstimatedYearlyFees,
                MinToken0Amount = response.Token0AmountMin,
                MinToken1Amount = response.Token1AmountMin,
                GasEstimate = response.GasEstimate,
                HasSufficientBalance = response.HasSufficientBalance,
                HasSufficientAllowance = response.HasSufficientAllowance,
                SlippageTolerance = response.SlippageTolerance,
                ValidationMessages = response.ValidationMessages,
                WarningMessages = response.WarningMessages,
                RiskLevel = response.RiskLevel,
                ImpermanentLossEstimate = response.ImpermanentLossEstimate,
                CalculatedAt = response.CalculatedAt
            };
        }

        #endregion

        #region Fallback Methods

        private LiquidityPageDataModel GetFallbackLiquidityPageData()
        {
            return new LiquidityPageDataModel
            {
                LiquidityPools = new List<LiquidityPoolModel>(),
                DexOptions = new List<DexConfigurationModel>(),
                Stats = GetFallbackLiquidityStats(),
                Analytics = new LiquidityAnalyticsModel(),
                GuideSteps = GetFallbackGuideSteps(),
                LoadedAt = DateTime.UtcNow
            };
        }

        private LiquidityStatsModel GetFallbackLiquidityStats()
        {
            return new LiquidityStatsModel
            {
                TotalValueLocked = 0,
                Volume24h = 0,
                TotalPools = 0,
                ActivePools = 0,
                TotalUsers = 0,
                ActiveUsers = 0,
                TeachCurrentPrice = 0.05m,
                TeachPriceChange24h = 0,
                AveragePoolAPY = 0
            };
        }

        private LiquidityAnalyticsModel GetFallbackLiquidityAnalytics()
        {
            return new LiquidityAnalyticsModel
            {
                AnalysisPeriod = "30d",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow
            };
        }

        private AddLiquidityModel GetFallbackAddLiquidityModel()
        {
            return new AddLiquidityModel
            {
                AvailablePools = new List<LiquidityPoolModel>(),
                RecommendedPools = new List<LiquidityPoolModel>(),
                AvailableDexes = new List<DexConfigurationModel>()
            };
        }

        private UserLiquidityInfoModel GetFallbackUserLiquidityInfo(string walletAddress)
        {
            return new UserLiquidityInfoModel
            {
                WalletAddress = walletAddress,
                TotalLiquidityValue = 0,
                TotalFeesEarned = 0,
                TotalPnL = 0,
                TotalPnLPercentage = 0,
                ActivePositions = 0,
                TotalPositions = 0,
                FirstPositionDate = DateTime.UtcNow,
                Positions = new List<UserLiquidityPositionModel>(),
                RecentTransactions = new List<LiquidityTransactionHistoryModel>(),
                Stats = new UserLiquidityStatsModel { WalletAddress = walletAddress }
            };
        }

        private List<LiquidityGuideStepModel> GetFallbackGuideSteps()
        {
            return new List<LiquidityGuideStepModel>
            {
                new() { StepNumber = 1, Title = "Connect Wallet", Description = "Connect your Web3 wallet to begin", Icon = "🔗", ActionText = "Connect", ActionUrl = "#" },
                new() { StepNumber = 2, Title = "Select Pool", Description = "Choose a liquidity pool to join", Icon = "🏊", ActionText = "Browse Pools", ActionUrl = "#" },
                new() { StepNumber = 3, Title = "Add Liquidity", Description = "Deposit your tokens to earn fees", Icon = "💰", ActionText = "Add Liquidity", ActionUrl = "#" }
            };
        }

        #endregion

        #region Helper Methods

        private decimal ParsePriceChange(string priceChangeDisplay)
        {
            if (string.IsNullOrEmpty(priceChangeDisplay))
                return 0;

            // Remove % and + symbols, parse decimal
            var cleanString = priceChangeDisplay.Replace("%", "").Replace("+", "");
            return decimal.TryParse(cleanString, out var result) ? result : 0;
        }

        #endregion
    }
}