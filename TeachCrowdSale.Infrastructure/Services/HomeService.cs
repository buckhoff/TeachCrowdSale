using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Infrastructure.Services;

/// <summary>
/// Home service implementation for aggregating home page data
/// </summary>
public class HomeService : IHomeService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<HomeService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    // Cache keys and durations
    private const string CACHE_KEY_HOME_DATA = "home_page_data";
    private const string CACHE_KEY_LIVE_STATS = "live_stats";
    private const string CACHE_KEY_TIERS = "tier_data";
    private const string CACHE_KEY_CONTRACT_INFO = "contract_info";

    private readonly TimeSpan _shortCacheDuration = TimeSpan.FromMinutes(2);
    private readonly TimeSpan _liveCacheDuration = TimeSpan.FromMinutes(1);
    private readonly TimeSpan _mediumCacheDuration = TimeSpan.FromMinutes(10);
    private readonly TimeSpan _longCacheDuration = TimeSpan.FromHours(1);

    public HomeService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<HomeService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("TeachAPI");
        _cache = cache;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<HomePageDataModel> GetHomePageDataAsync()
    {
        try
        {
            if (_cache.TryGetValue(CACHE_KEY_HOME_DATA, out HomePageDataModel? cachedData) && cachedData != null)
            {
                return cachedData;
            }

            var homeData = new HomePageDataModel();

            // Get presale statistics
            var presaleStats = await GetPresaleStatsAsync();
            if (presaleStats != null)
            {
                homeData.PresaleStats = new PresaleStatsModel
                {
                    TotalRaised = presaleStats.TotalRaised,
                    FundingGoal = presaleStats.FundingGoal,
                    TokensSold = presaleStats.TokensSold,
                    TokensRemaining = 5000000000m - presaleStats.TokensSold, // Total supply - sold
                    ParticipantsCount = presaleStats.ParticipantsCount,
                    IsActive = true,
                    FundingProgress = presaleStats.FundingGoal > 0 ?
                        (presaleStats.TotalRaised / presaleStats.FundingGoal) * 100 : 0
                };
            }

            // Get current tier
            var currentTier = await GetCurrentTierAsync();
            if (currentTier != null)
            {
                homeData.CurrentTier = new CurrentTierModel
                {
                    Id = currentTier.Id,
                    Name = currentTier.Name,
                    Price = currentTier.Price,
                    Progress = currentTier.TotalAllocation > 0 ?
                        (currentTier.Sold / currentTier.TotalAllocation) * 100 : 0,
                    Sold = currentTier.Sold,
                    Remaining = currentTier.TotalAllocation - currentTier.Sold,
                    IsActive = currentTier.IsActive
                };
            }

            // Get token information
            var tokenInfo = await GetTokenInfoAsync();
            if (tokenInfo != null)
            {
                homeData.TokenInfo = tokenInfo;
            }
            else
            {
                // Use fallback token info
                homeData.TokenInfo = new TokenInfoModel
                {
                    TotalSupply = 5000000000m,
                    CirculatingSupply = 1000000000m,
                    CurrentPrice = homeData.CurrentTier?.Price ?? 0.06m,
                    MarketCap = (homeData.CurrentTier?.Price ?? 0.06m) * 1000000000m,
                    HoldersCount = 2847
                };
            }

            // Get investment highlights
            homeData.InvestmentHighlights = GetInvestmentHighlights();

            _cache.Set(CACHE_KEY_HOME_DATA, homeData, _shortCacheDuration);
            return homeData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading comprehensive home page data");
            return GetFallbackHomeData();
        }
    }

    public async Task<LiveStatsModel> GetLiveStatsAsync()
    {
        try
        {
            if (_cache.TryGetValue(CACHE_KEY_LIVE_STATS, out LiveStatsModel? cachedStats) && cachedStats != null)
            {
                return cachedStats;
            }

            var stats = new LiveStatsModel();

            // Get presale stats
            var presaleStats = await GetPresaleStatsAsync();
            if (presaleStats != null)
            {
                stats.TotalRaised = presaleStats.TotalRaised;
                stats.TokensSold = presaleStats.TokensSold;
                stats.ParticipantsCount = presaleStats.ParticipantsCount;
                stats.IsPresaleActive = true;
            }

            // Get current tier
            var currentTier = await GetCurrentTierAsync();
            if (currentTier != null)
            {
                stats.CurrentTierPrice = currentTier.Price;
                stats.CurrentTierName = currentTier.Name;
                stats.CurrentTierProgress = currentTier.TotalAllocation > 0 ?
                    (currentTier.Sold / currentTier.TotalAllocation) * 100 : 0;
            }

            // Get token info
            var tokenInfo = await GetTokenInfoAsync();
            if (tokenInfo != null)
            {
                stats.TokenPrice = tokenInfo.CurrentPrice;
                stats.MarketCap = tokenInfo.MarketCap;
                stats.HoldersCount = tokenInfo.HoldersCount;
            }

            stats.UpdatedAt = DateTime.UtcNow;
            _cache.Set(CACHE_KEY_LIVE_STATS, stats, _liveCacheDuration);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving live statistics");

            // Return fallback stats
            return new LiveStatsModel
            {
                TotalRaised = 12500000m,
                TokensSold = 750000000m,
                ParticipantsCount = 2847,
                TokenPrice = 0.06m,
                MarketCap = 60000000m,
                HoldersCount = 2847,
                CurrentTierPrice = 0.06m,
                CurrentTierName = "Community Round",
                CurrentTierProgress = 45.0m,
                IsPresaleActive = true,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }

    public async Task<List<TierDisplayModel>> GetTierDisplayDataAsync()
    {
        try
        {
            if (_cache.TryGetValue(CACHE_KEY_TIERS, out List<TierDisplayModel>? cachedTiers) && cachedTiers != null)
            {
                return cachedTiers;
            }

            var response = await _httpClient.GetAsync("/api/presale/tiers");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to retrieve tier data from API. Status: {StatusCode}", response.StatusCode);
                return GetFallbackTierData();
            }

            var content = await response.Content.ReadAsStringAsync();
            var tiers = JsonSerializer.Deserialize<List<TierModel>>(content, _jsonOptions);

            var tierDisplayModels = new List<TierDisplayModel>();

            if (tiers != null && tiers.Any())
            {
                foreach (var tier in tiers)
                {
                    var displayModel = new TierDisplayModel
                    {
                        Id = tier.Id,
                        Name = tier.Name,
                        Price = tier.Price,
                        TotalAllocation = tier.TotalAllocation,
                        Sold = tier.Sold,
                        Remaining = tier.TotalAllocation - tier.Sold,
                        Progress = tier.TotalAllocation > 0 ? (tier.Sold / tier.TotalAllocation) * 100 : 0,
                        IsActive = tier.IsActive,
                        IsSoldOut = tier.Sold >= tier.TotalAllocation,
                        MinPurchase = tier.MinPurchase,
                        MaxPurchase = tier.MaxPurchase,
                        VestingTGE = tier.VestingTGE,
                        VestingMonths = tier.VestingMonths,
                        EndTime = tier.EndTime
                    };

                    // Calculate tier status
                    if (displayModel.IsSoldOut)
                    {
                        displayModel.Status = "SOLD OUT";
                        displayModel.StatusClass = "sold-out";
                    }
                    else if (displayModel.IsActive)
                    {
                        displayModel.Status = "ACTIVE";
                        displayModel.StatusClass = "active";
                    }
                    else
                    {
                        displayModel.Status = "COMING SOON";
                        displayModel.StatusClass = "upcoming";
                    }

                    tierDisplayModels.Add(displayModel);
                }
            }

            _cache.Set(CACHE_KEY_TIERS, tierDisplayModels, _mediumCacheDuration);
            return tierDisplayModels;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tier information");
            return GetFallbackTierData();
        }
    }

    public async Task<ContractInfoModel> GetContractInfoAsync()
    {
        try
        {
            if (_cache.TryGetValue(CACHE_KEY_CONTRACT_INFO, out ContractInfoModel? cachedInfo) && cachedInfo != null)
            {
                return cachedInfo;
            }

            var response = await _httpClient.GetAsync("/api/presale/contracts");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to retrieve contract data from API. Status: {StatusCode}", response.StatusCode);
                return GetFallbackContractInfo();
            }

            var content = await response.Content.ReadAsStringAsync();
            var contractInfo = JsonSerializer.Deserialize<ContractInfoModel>(content, _jsonOptions);

            if (contractInfo != null)
            {
                _cache.Set(CACHE_KEY_CONTRACT_INFO, contractInfo, _longCacheDuration);
                return contractInfo;
            }

            return GetFallbackContractInfo();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contract information");
            return GetFallbackContractInfo();
        }
    }

    public List<InvestmentHighlightModel> GetInvestmentHighlights()
    {
        return new List<InvestmentHighlightModel>
        {
            new InvestmentHighlightModel
            {
                Icon = "🔒",
                Title = "Treasury Security",
                Description = "Multi-layer funding ensures 10+ year operational runway regardless of market conditions",
                Category = "Security"
            },
            new InvestmentHighlightModel
            {
                Icon = "📈",
                Title = "Deflationary Mechanics",
                Description = "Token burns and limited supply create natural price appreciation pressure",
                Category = "Tokenomics"
            },
            new InvestmentHighlightModel
            {
                Icon = "🎯",
                Title = "Real Utility",
                Description = "Direct use case in $6T education market with measurable impact",
                Category = "Utility"
            },
            new InvestmentHighlightModel
            {
                Icon = "⚡",
                Title = "First Mover Advantage",
                Description = "First blockchain solution specifically designed for education funding",
                Category = "Market"
            }
        };
    }

    public HomePageDataModel GetFallbackHomeData()
    {
        return new HomePageDataModel
        {
            PresaleStats = new PresaleStatsModel
            {
                TotalRaised = 12500000m,
                FundingGoal = 87500000m,
                TokensSold = 750000000m,
                TokensRemaining = 4250000000m,
                ParticipantsCount = 2847,
                IsActive = true,
                FundingProgress = 14.3m
            },
            CurrentTier = new CurrentTierModel
            {
                Id = 2,
                Name = "Community Round",
                Price = 0.06m,
                Progress = 45.0m,
                Sold = 168750000m,
                Remaining = 206250000m,
                IsActive = true
            },
            TokenInfo = new TokenInfoModel
            {
                TotalSupply = 5000000000m,
                CirculatingSupply = 1000000000m,
                CurrentPrice = 0.06m,
                MarketCap = 60000000m,
                HoldersCount = 2847
            },
            InvestmentHighlights = GetInvestmentHighlights()
        };
    }

    public async Task<bool> CheckApiHealthAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/health");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "API health check failed");
            return false;
        }
    }

    #region Private Helper Methods

    private async Task<PresaleStatusModel?> GetPresaleStatsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/presale/status");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PresaleStatusModel>(content, _jsonOptions);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error retrieving presale stats");
        }
        return null;
    }

    private async Task<TierModel?> GetCurrentTierAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/presale/current-tier");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TierModel>(content, _jsonOptions);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error retrieving current tier");
        }
        return null;
    }

    private async Task<TokenInfoModel?> GetTokenInfoAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/tokeninfo");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TokenInfoModel>(content, _jsonOptions);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error retrieving token info");
        }
        return null;
    }

    private List<TierDisplayModel> GetFallbackTierData()
    {
        return new List<TierDisplayModel>
        {
            new TierDisplayModel
            {
                Id = 1,
                Name = "Seed Round",
                Price = 0.04m,
                TotalAllocation = 250000000m,
                Sold = 250000000m,
                Remaining = 0m,
                Progress = 100m,
                IsActive = false,
                IsSoldOut = true,
                Status = "SOLD OUT",
                StatusClass = "sold-out",
                VestingTGE = 20,
                VestingMonths = 6
            },
            new TierDisplayModel
            {
                Id = 2,
                Name = "Community Round",
                Price = 0.06m,
                TotalAllocation = 375000000m,
                Sold = 168750000m,
                Remaining = 206250000m,
                Progress = 45m,
                IsActive = true,
                IsSoldOut = false,
                Status = "ACTIVE",
                StatusClass = "active",
                VestingTGE = 20,
                VestingMonths = 6
            },
            new TierDisplayModel
            {
                Id = 3,
                Name = "Growth Round",
                Price = 0.08m,
                TotalAllocation = 375000000m,
                Sold = 0m,
                Remaining = 375000000m,
                Progress = 0m,
                IsActive = false,
                IsSoldOut = false,
                Status = "COMING SOON",
                StatusClass = "upcoming",
                VestingTGE = 20,
                VestingMonths = 6
            },
            new TierDisplayModel
            {
                Id = 4,
                Name = "Final Round",
                Price = 0.10m,
                TotalAllocation = 250000000m,
                Sold = 0m,
                Remaining = 250000000m,
                Progress = 0m,
                IsActive = false,
                IsSoldOut = false,
                Status = "COMING SOON",
                StatusClass = "upcoming",
                VestingTGE = 20,
                VestingMonths = 6
            }
        };
    }

    private ContractInfoModel GetFallbackContractInfo()
    {
        return new ContractInfoModel
        {
            PresaleAddress = "0x1234567890123456789012345678901234567890",
            TokenAddress = "0x0987654321098765432109876543210987654321",
            PaymentTokenAddress = "0xA0b86a33E6441b8FAe87b0E0E53A7C7A0B868C8B", // USDC
            NetworkId = 137,
            ChainName = "Polygon Mainnet"
        };
    }

    #endregion
}