using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models;
using TeachCrowdSale.Core.Models.Request;
using TeachCrowdSale.Core.Models.Response;

public class StakingDashboardService : IStakingDashboardService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<StakingDashboardService> _logger;

    // Cache duration constants following established TeachToken patterns
    private readonly TimeSpan _shortCacheDuration = TimeSpan.FromSeconds(30);    // User data
    private readonly TimeSpan _mediumCacheDuration = TimeSpan.FromMinutes(2);     // Pool data
    private readonly TimeSpan _longCacheDuration = TimeSpan.FromMinutes(10);      // School data

    // Cache key constants
    private const string STAKING_STATS_CACHE_KEY = "staking_stats";
    private const string ACTIVE_POOLS_CACHE_KEY = "active_staking_pools";
    private const string AVAILABLE_SCHOOLS_CACHE_KEY = "available_schools";
    private const string USER_STAKING_INFO_PREFIX = "user_staking_info_";
    private const string USER_POSITIONS_PREFIX = "user_positions_";
    private const string USER_TRANSACTIONS_PREFIX = "user_transactions_";
    private const string POOL_DETAILS_PREFIX = "pool_details_";
    private const string SCHOOL_DETAILS_PREFIX = "school_details_";
    private const string DASHBOARD_DATA_PREFIX = "dashboard_data_";

    public StakingDashboardService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<StakingDashboardService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("TeachAPI");
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Dashboard Overview

    public async Task<StakingStatsModel?> GetStakingStatsAsync()
    {
        if (_cache.TryGetValue(STAKING_STATS_CACHE_KEY, out StakingStatsModel? cachedStats))
        {
            return cachedStats;
        }

        try
        {
            var response = await _httpClient.GetAsync("api/staking/stats");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var stats = JsonSerializer.Deserialize<StakingStatsModel>(json, GetJsonOptions());

                if (stats != null)
                {
                    _cache.Set(STAKING_STATS_CACHE_KEY, stats, _mediumCacheDuration);
                }

                return stats;
            }

            _logger.LogWarning("Failed to fetch staking stats. Status: {StatusCode}", response.StatusCode);
            return GetFallbackStakingStats();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching staking stats");
            return GetFallbackStakingStats();
        }
    }

    public async Task<StakingDashboardDataModel?> GetDashboardDataAsync(string? walletAddress = null)
    {
        var cacheKey = $"{DASHBOARD_DATA_PREFIX}{walletAddress ?? "anonymous"}";

        if (_cache.TryGetValue(cacheKey, out StakingDashboardDataModel? cachedData))
        {
            return cachedData;
        }

        try
        {
            var queryParams = string.IsNullOrEmpty(walletAddress) ? "" : $"?walletAddress={walletAddress}";
            var response = await _httpClient.GetAsync($"api/staking/dashboard{queryParams}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var dashboardData = JsonSerializer.Deserialize<StakingDashboardDataModel>(json, GetJsonOptions());

                if (dashboardData != null)
                {
                    var cacheDuration = string.IsNullOrEmpty(walletAddress) ? _mediumCacheDuration : _shortCacheDuration;
                    _cache.Set(cacheKey, dashboardData, cacheDuration);
                }

                return dashboardData;
            }

            _logger.LogWarning("Failed to fetch dashboard data. Status: {StatusCode}", response.StatusCode);
            return await GetFallbackDashboardDataAsync(walletAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard data");
            return await GetFallbackDashboardDataAsync(walletAddress);
        }
    }

    #endregion

    #region Pool Management

    public async Task<List<StakingPool>?> GetActiveStakingPoolsAsync()
    {
        if (_cache.TryGetValue(ACTIVE_POOLS_CACHE_KEY, out List<StakingPool>? cachedPools))
        {
            return cachedPools;
        }

        try
        {
            var response = await _httpClient.GetAsync("api/staking/pools");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var pools = JsonSerializer.Deserialize<List<StakingPool>>(json, GetJsonOptions());

                if (pools != null)
                {
                    _cache.Set(ACTIVE_POOLS_CACHE_KEY, pools, _mediumCacheDuration);
                }

                return pools;
            }

            _logger.LogWarning("Failed to fetch staking pools. Status: {StatusCode}", response.StatusCode);
            return GetFallbackStakingPools();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching staking pools");
            return GetFallbackStakingPools();
        }
    }

    public async Task<StakingPool?> GetStakingPoolAsync(int poolId)
    {
        var cacheKey = $"{POOL_DETAILS_PREFIX}{poolId}";

        if (_cache.TryGetValue(cacheKey, out StakingPool? cachedPool))
        {
            return cachedPool;
        }

        try
        {
            var response = await _httpClient.GetAsync($"api/staking/pools/{poolId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var pool = JsonSerializer.Deserialize<StakingPool>(json, GetJsonOptions());

                if (pool != null)
                {
                    _cache.Set(cacheKey, pool, _mediumCacheDuration);
                }

                return pool;
            }

            _logger.LogWarning("Failed to fetch pool {PoolId}. Status: {StatusCode}", poolId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pool {PoolId}", poolId);
            return null;
        }
    }

    #endregion

    #region User Data

    public async Task<UserStakingInfoModel?> GetUserStakingInfoAsync(string walletAddress)
    {
        if (string.IsNullOrWhiteSpace(walletAddress))
        {
            return null;
        }

        var cacheKey = $"{USER_STAKING_INFO_PREFIX}{walletAddress.ToLowerInvariant()}";

        if (_cache.TryGetValue(cacheKey, out UserStakingInfoModel? cachedInfo))
        {
            return cachedInfo;
        }

        try
        {
            var response = await _httpClient.GetAsync($"api/staking/user/{walletAddress}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<UserStakingInfoModel>(json, GetJsonOptions());

                if (userInfo != null)
                {
                    _cache.Set(cacheKey, userInfo, _shortCacheDuration);
                }

                return userInfo;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return GetEmptyUserStakingInfo(walletAddress);
            }

            _logger.LogWarning("Failed to fetch user staking info for {WalletAddress}. Status: {StatusCode}",
                walletAddress, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user staking info for {WalletAddress}", walletAddress);
            return null;
        }
    }

    public async Task<List<UserStakePositionModel>?> GetUserActivePositionsAsync(string walletAddress)
    {
        if (string.IsNullOrWhiteSpace(walletAddress))
        {
            return null;
        }

        var cacheKey = $"{USER_POSITIONS_PREFIX}{walletAddress.ToLowerInvariant()}";

        if (_cache.TryGetValue(cacheKey, out List<UserStakePositionModel>? cachedPositions))
        {
            return cachedPositions;
        }

        try
        {
            var response = await _httpClient.GetAsync($"api/staking/user/{walletAddress}/positions");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var positions = JsonSerializer.Deserialize<List<UserStakePositionModel>>(json, GetJsonOptions());

                if (positions != null)
                {
                    _cache.Set(cacheKey, positions, _shortCacheDuration);
                }

                return positions;
            }

            _logger.LogWarning("Failed to fetch user positions for {WalletAddress}. Status: {StatusCode}",
                walletAddress, response.StatusCode);
            return new List<UserStakePositionModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user positions for {WalletAddress}", walletAddress);
            return new List<UserStakePositionModel>();
        }
    }

    public async Task<List<StakingTransactionResponse>?> GetUserTransactionHistoryAsync(string walletAddress, int page = 1, int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(walletAddress))
        {
            return null;
        }

        var cacheKey = $"{USER_TRANSACTIONS_PREFIX}{walletAddress.ToLowerInvariant()}_{page}_{pageSize}";

        if (_cache.TryGetValue(cacheKey, out List<StakingTransactionResponse>? cachedTransactions))
        {
            return cachedTransactions;
        }

        try
        {
            var queryParams = $"?page={page}&pageSize={pageSize}";
            var response = await _httpClient.GetAsync($"api/staking/user/{walletAddress}/transactions{queryParams}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var transactions = JsonSerializer.Deserialize<List<StakingTransactionResponse>>(json, GetJsonOptions());

                if (transactions != null)
                {
                    _cache.Set(cacheKey, transactions, _shortCacheDuration);
                }

                return transactions;
            }

            _logger.LogWarning("Failed to fetch transaction history for {WalletAddress}. Status: {StatusCode}",
                walletAddress, response.StatusCode);
            return new List<StakingTransactionResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching transaction history for {WalletAddress}", walletAddress);
            return new List<StakingTransactionResponse>();
        }
    }

    #endregion

    #region Calculations

    public async Task<StakingCalculationModel?> CalculateStakingPreviewAsync(string walletAddress, int poolId, decimal amount, int lockPeriodDays)
    {
        if (string.IsNullOrWhiteSpace(walletAddress) || amount <= 0)
        {
            return null;
        }

        try
        {
            var queryParams = $"?walletAddress={walletAddress}&poolId={poolId}&amount={amount}&lockPeriodDays={lockPeriodDays}";
            var response = await _httpClient.GetAsync($"api/staking/calculate{queryParams}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<StakingCalculationModel>(json, GetJsonOptions());
            }

            _logger.LogWarning("Failed to calculate staking preview. Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating staking preview for {WalletAddress}", walletAddress);
            return null;
        }
    }

    public async Task<List<RewardProjectionModel>?> GetRewardProjectionsAsync(string walletAddress, int poolId, decimal amount, int days = 365)
    {
        if (string.IsNullOrWhiteSpace(walletAddress) || amount <= 0)
        {
            return null;
        }

        try
        {
            var queryParams = $"?walletAddress={walletAddress}&poolId={poolId}&amount={amount}&days={days}";
            var response = await _httpClient.GetAsync($"api/staking/projections{queryParams}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<RewardProjectionModel>>(json, GetJsonOptions());
            }

            _logger.LogWarning("Failed to get reward projections. Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reward projections for {WalletAddress}", walletAddress);
            return null;
        }
    }

    public async Task<decimal?> CalculateStakingRewardsAsync(string walletAddress, int poolId)
    {
        if (string.IsNullOrWhiteSpace(walletAddress))
        {
            return null;
        }

        try
        {
            var queryParams = $"?walletAddress={walletAddress}&poolId={poolId}";
            var response = await _httpClient.GetAsync($"api/staking/rewards{queryParams}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var rewardData = JsonSerializer.Deserialize<StakingRewardResponse>(json, GetJsonOptions());
                return rewardData?.TotalRewards;
            }

            _logger.LogWarning("Failed to calculate staking rewards. Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating staking rewards for {WalletAddress}", walletAddress);
            return null;
        }
    }

    #endregion

    #region School Management

    public async Task<List<SchoolBeneficiaryModel>?> GetAvailableSchoolsAsync()
    {
        if (_cache.TryGetValue(AVAILABLE_SCHOOLS_CACHE_KEY, out List<SchoolBeneficiaryModel>? cachedSchools))
        {
            return cachedSchools;
        }

        try
        {
            var response = await _httpClient.GetAsync("api/staking/schools");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var schools = JsonSerializer.Deserialize<List<SchoolBeneficiaryModel>>(json, GetJsonOptions());

                if (schools != null)
                {
                    _cache.Set(AVAILABLE_SCHOOLS_CACHE_KEY, schools, _longCacheDuration);
                }

                return schools;
            }

            _logger.LogWarning("Failed to fetch available schools. Status: {StatusCode}", response.StatusCode);
            return GetFallbackSchools();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching available schools");
            return GetFallbackSchools();
        }
    }

    public async Task<SchoolBeneficiaryModel?> GetSchoolDetailsAsync(int schoolId)
    {
        var cacheKey = $"{SCHOOL_DETAILS_PREFIX}{schoolId}";

        if (_cache.TryGetValue(cacheKey, out SchoolBeneficiaryModel? cachedSchool))
        {
            return cachedSchool;
        }

        try
        {
            var response = await _httpClient.GetAsync($"api/staking/schools/{schoolId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var school = JsonSerializer.Deserialize<SchoolBeneficiaryModel>(json, GetJsonOptions());

                if (school != null)
                {
                    _cache.Set(cacheKey, school, _longCacheDuration);
                }

                return school;
            }

            _logger.LogWarning("Failed to fetch school {SchoolId}. Status: {StatusCode}", schoolId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching school {SchoolId}", schoolId);
            return null;
        }
    }

    #endregion

    #region Transaction Operations

    public async Task<StakingTransactionResponse?> StakeTokensAsync(StakeTokensRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.WalletAddress) || request.Amount <= 0)
        {
            return null;
        }

        try
        {
            var json = JsonSerializer.Serialize(request, GetJsonOptions());
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/staking/stake", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<StakingTransactionResponse>(responseJson, GetJsonOptions());

                // Clear user cache on successful transaction
                if (result?.IsSuccess == true)
                {
                    ClearUserCache(request.WalletAddress);
                }

                return result;
            }

            _logger.LogWarning("Failed to stake tokens. Status: {StatusCode}", response.StatusCode);
            return new StakingTransactionResponse
            {
                IsSuccess = false,
                ErrorMessage = $"API request failed with status: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error staking tokens for {WalletAddress}", request.WalletAddress);
            return new StakingTransactionResponse
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<StakingTransactionResponse?> ClaimRewardsAsync(ClaimRewardsRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.WalletAddress))
        {
            return null;
        }

        try
        {
            var json = JsonSerializer.Serialize(request, GetJsonOptions());
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/staking/claim", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<StakingTransactionResponse>(responseJson, GetJsonOptions());

                // Clear user cache on successful transaction
                if (result?.IsSuccess == true)
                {
                    ClearUserCache(request.WalletAddress);
                }

                return result;
            }

            _logger.LogWarning("Failed to claim rewards. Status: {StatusCode}", response.StatusCode);
            return new StakingTransactionResponse
            {
                IsSuccess = false,
                ErrorMessage = $"API request failed with status: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error claiming rewards for {WalletAddress}", request.WalletAddress);
            return new StakingTransactionResponse
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<StakingTransactionResponse?> UnstakeTokensAsync(UnstakeTokensRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.WalletAddress))
        {
            return null;
        }

        try
        {
            var json = JsonSerializer.Serialize(request, GetJsonOptions());
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/staking/unstake", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<StakingTransactionResponse>(responseJson, GetJsonOptions());

                // Clear user cache on successful transaction
                if (result?.IsSuccess == true)
                {
                    ClearUserCache(request.WalletAddress);
                }
                return result;
            }

            _logger.LogWarning("Failed to unstake tokens. Status: {StatusCode}", response.StatusCode);
            return new StakingTransactionResponse
            {
                IsSuccess = false,
                ErrorMessage = $"API request failed with status: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unstaking tokens for {WalletAddress}", request.WalletAddress);
            return new StakingTransactionResponse
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    #endregion

    #region Cache Management

    public void ClearUserCache(string walletAddress)
    {
        if (string.IsNullOrWhiteSpace(walletAddress))
            return;

        var lowerAddress = walletAddress.ToLowerInvariant();
        var keysToRemove = new[]
        {
                $"{USER_STAKING_INFO_PREFIX}{lowerAddress}",
                $"{USER_POSITIONS_PREFIX}{lowerAddress}",
                $"{DASHBOARD_DATA_PREFIX}{lowerAddress}"
            };

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
        }

        // Remove transaction history cache (multiple pages)
        for (int page = 1; page <= 10; page++) // Clear first 10 pages
        {
            _cache.Remove($"{USER_TRANSACTIONS_PREFIX}{lowerAddress}_{page}_20");
        }

        _logger.LogDebug("Cleared cache for user {WalletAddress}", walletAddress);
    }

    public void ClearAllCache()
    {
        var keysToRemove = new[]
        {
                STAKING_STATS_CACHE_KEY,
                ACTIVE_POOLS_CACHE_KEY,
                AVAILABLE_SCHOOLS_CACHE_KEY
            };

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
        }

        _logger.LogDebug("Cleared all staking cache");
    }

    #endregion

    #region Private Helper Methods

    private JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    private StakingStatsModel GetFallbackStakingStats()
    {
        return new StakingStatsModel
        {
            TotalValueLocked = 0m,
            TotalRewardsDistributed = 0m,
            ActiveStakers = 0,
            AverageAPY = 12.5m,
            TotalSchoolFunding = 0m,
            SchoolsSupported = 0,
            StakingParticipation = 0m
        };
    }

    private List<StakingPool> GetFallbackStakingPools()
    {
        return new List<StakingPool>
            {
                new StakingPool
                {
                    Id = 1,
                    Name = "Flexible Pool",
                    Description = "30-day flexible staking with daily rewards",
                    LockPeriodDays = 30,
                    BaseAPY = 8.0m,
                    BonusAPY = 2.0m,
                    MinStakeAmount = 100m,
                    MaxStakeAmount = 50000m,
                    TotalStaked = 0m,
                    MaxCapacity = 1000000m,
                    IsActive = true,
                    EarlyWithdrawalPenalty = 5.0m
                },
                new StakingPool
                {
                    Id = 2,
                    Name = "Standard Pool",
                    Description = "90-day staking with enhanced rewards",
                    LockPeriodDays = 90,
                    BaseAPY = 12.0m,
                    BonusAPY = 3.0m,
                    MinStakeAmount = 1000m,
                    MaxStakeAmount = 100000m,
                    TotalStaked = 0m,
                    MaxCapacity = 2000000m,
                    IsActive = true,
                    EarlyWithdrawalPenalty = 8.0m
                }
            };
    }

    private List<SchoolBeneficiaryModel> GetFallbackSchools()
    {
        return new List<SchoolBeneficiaryModel>
            {
                new SchoolBeneficiaryModel
                {
                    Id = 1,
                    Name = "Demo Elementary School",
                    Description = "Supporting local education initiatives",
                    Location = "Demo City, Demo State",
                    TotalFundingReceived = 0m,
                    ActiveStakers = 0,
                    VerificationStatus = "Verified",
                    SchoolWebsite = "https://demo-school.edu",
                    ContactEmail = "info@demo-school.edu"
                }
            };
    }

    private UserStakingInfoModel GetEmptyUserStakingInfo(string walletAddress)
    {
        return new UserStakingInfoModel
        {
            WalletAddress = walletAddress,
            TotalStaked = 0m,
            TotalRewards = 0m,
            ClaimableRewards = 0m,
            ProjectedMonthlyRewards = 0m,
            StakePositions = new List<UserStakePositionModel>(),
            SelectedSchool = null,
            RewardHistory = new List<RewardClaimHistoryModel>()
        };
    }

    private async Task<StakingDashboardDataModel?> GetFallbackDashboardDataAsync(string? walletAddress)
    {
        _logger.LogWarning("Using fallback dashboard data");

        return new StakingDashboardDataModel
        {
            Stats = GetFallbackStakingStats(),
            Pools = GetFallbackStakingPools(),
            Schools = GetFallbackSchools(),
            UserInfo = string.IsNullOrEmpty(walletAddress) ? null : GetEmptyUserStakingInfo(walletAddress),
            LastUpdated = DateTime.UtcNow,
            IsFallbackData = true
        };
    }

    #endregion
}