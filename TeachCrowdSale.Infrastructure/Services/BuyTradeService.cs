using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Models.Request;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Infrastructure.Services;

/// <summary>
/// Buy/Trade service implementation for aggregating buy/trade page data
/// </summary>
public class BuyTradeService : IBuyTradeService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<BuyTradeService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    // Cache keys and durations
    private const string CACHE_KEY_BUYTRADE_DATA = "buytrade_page_data";
    private const string CACHE_KEY_DEX_INFO = "dex_info";
    private const string CACHE_KEY_USER_PREFIX = "user_trade_info_";

    private readonly TimeSpan _shortCacheDuration = TimeSpan.FromMinutes(2);
    private readonly TimeSpan _mediumCacheDuration = TimeSpan.FromMinutes(10);
    private readonly TimeSpan _longCacheDuration = TimeSpan.FromMinutes(30);

    public BuyTradeService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<BuyTradeService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("TeachAPI");
        _cache = cache;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<BuyTradeDataModel> GetBuyTradeDataAsync()
    {
        try
        {
            if (_cache.TryGetValue(CACHE_KEY_BUYTRADE_DATA, out BuyTradeDataModel? cachedData) && cachedData != null)
            {
                return cachedData;
            }

            var buyTradeData = new BuyTradeDataModel();

            // Get presale data
            var presaleTask = GetPresaleDataAsync();
            var tokenInfoTask = GetTokenInfoAsync();
            var contractInfoTask = GetContractInfoAsync();

            await Task.WhenAll(presaleTask, tokenInfoTask, contractInfoTask);

            var presaleData = await presaleTask;
            if (presaleData != null)
            {
                buyTradeData.CurrentTier = presaleData.CurrentTier;
                buyTradeData.AllTiers = presaleData.AllTiers;
                buyTradeData.PresaleStats = presaleData.PresaleStats;
            }

            buyTradeData.TokenInfo = await tokenInfoTask;
            buyTradeData.ContractInfo = await contractInfoTask;

            // Add static data
            buyTradeData.PurchaseOptions = GetPurchaseOptions();
            buyTradeData.DexIntegrations = GetDexIntegrations();

            _cache.Set(CACHE_KEY_BUYTRADE_DATA, buyTradeData, _shortCacheDuration);
            return buyTradeData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading comprehensive buy/trade data");
            return GetFallbackBuyTradeData();
        }
    }

    public async Task<PriceCalculationModel> CalculatePriceAsync(string address, int tierId, decimal usdAmount)
    {
        try
        {
            var request = new PriceCalculationRequest
            {
                Address = address,
                TierId = tierId,
                UsdAmount = usdAmount
            };

            var response = await _httpClient.PostAsJsonAsync("/api/buytrade/calculate-price", request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Price calculation failed with status: {StatusCode}", response.StatusCode);
                return GetFallbackPriceCalculation(tierId, usdAmount);
            }

            var content = await response.Content.ReadAsStringAsync();
            var calculation = JsonSerializer.Deserialize<PriceCalculationModel>(content, _jsonOptions);

            return calculation ?? GetFallbackPriceCalculation(tierId, usdAmount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating price for tier {TierId}, amount {Amount}", tierId, usdAmount);
            return GetFallbackPriceCalculation(tierId, usdAmount);
        }
    }

    public async Task<UserTradeInfoModel> GetUserTradeInfoAsync(string address)
    {
        try
        {
            var cacheKey = CACHE_KEY_USER_PREFIX + address;
            if (_cache.TryGetValue(cacheKey, out UserTradeInfoModel? cachedInfo) && cachedInfo != null)
            {
                return cachedInfo;
            }

            var response = await _httpClient.GetAsync($"/api/buytrade/user/{address}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("User trade info request failed for {Address} with status: {StatusCode}", address, response.StatusCode);
                return GetFallbackUserTradeInfo(address);
            }

            var content = await response.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<UserTradeInfoModel>(content, _jsonOptions);

            if (userInfo != null)
            {
                _cache.Set(cacheKey, userInfo, TimeSpan.FromMinutes(5));
                return userInfo;
            }

            return GetFallbackUserTradeInfo(address);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user trade info for {Address}", address);
            return GetFallbackUserTradeInfo(address);
        }
    }

    public async Task<PurchaseValidationModel> ValidatePurchaseAsync(string address, int tierId, decimal amount)
    {
        try
        {
            var request = new
            {
                Address = address,
                TierId = tierId,
                Amount = amount
            };

            var response = await _httpClient.PostAsJsonAsync("/api/presale/purchase-validation", request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Purchase validation failed: {Error}", errorContent);

                return new PurchaseValidationModel
                {
                    IsValid = false,
                    Address = address,
                    TierId = tierId,
                    Amount = amount,
                    // Add error details from response if needed
                };
            }

            var content = await response.Content.ReadAsStringAsync();
            var validation = JsonSerializer.Deserialize<PurchaseValidationModel>(content, _jsonOptions);

            return validation ?? new PurchaseValidationModel { IsValid = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating purchase for {Address}, tier {TierId}", address, tierId);
            return new PurchaseValidationModel { IsValid = false };
        }
    }

    public async Task<WalletInfoModel> GetWalletInfoAsync(string address)
    {
        try
        {
            // Get user trade info and claimable tokens
            var userInfoTask = GetUserTradeInfoAsync(address);
            var claimableTask = GetClaimableTokensAsync(address);

            await Task.WhenAll(userInfoTask, claimableTask);

            var userInfo = await userInfoTask;
            var claimableInfo = await claimableTask;

            return new WalletInfoModel
            {
                UserInfo = userInfo,
                ClaimableInfo = claimableInfo,
                IsConnected = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving wallet info for {Address}", address);
            return new WalletInfoModel
            {
                UserInfo = GetFallbackUserTradeInfo(address),
                IsConnected = false
            };
        }
    }

    public async Task<DexInfoModel> GetDexInfoAsync()
    {
        try
        {
            if (_cache.TryGetValue(CACHE_KEY_DEX_INFO, out DexInfoModel? cachedInfo) && cachedInfo != null)
            {
                return cachedInfo;
            }

            var response = await _httpClient.GetAsync("/api/buytrade/dex-info");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("DEX info request failed with status: {StatusCode}", response.StatusCode);
                return GetFallbackDexInfo();
            }

            var content = await response.Content.ReadAsStringAsync();
            var dexInfo = JsonSerializer.Deserialize<DexInfoModel>(content, _jsonOptions);

            if (dexInfo != null)
            {
                _cache.Set(CACHE_KEY_DEX_INFO, dexInfo, _longCacheDuration);
                return dexInfo;
            }

            return GetFallbackDexInfo();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving DEX information");
            return GetFallbackDexInfo();
        }
    }

    public BuyTradeDataModel GetFallbackBuyTradeData()
    {
        return new BuyTradeDataModel
        {
            CurrentTier = GetFallbackCurrentTier(),
            AllTiers = GetFallbackTiers(),
            PresaleStats = GetFallbackPresaleStats(),
            TokenInfo = GetFallbackTokenInfo(),
            ContractInfo = GetFallbackContractInfo(),
            PurchaseOptions = GetPurchaseOptions(),
            DexIntegrations = GetDexIntegrations()
        };
    }

    public async Task<bool> CheckServiceHealthAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/health");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Service health check failed");
            return false;
        }
    }

    #region Private Helper Methods

    private async Task<PresaleDataModel?> GetPresaleDataAsync()
    {
        try
        {
            var tasks = new[]
            {
                _httpClient.GetAsync("/api/presale/current-tier"),
                _httpClient.GetAsync("/api/presale/tiers"),
                _httpClient.GetAsync("/api/presale/status")
            };

            var responses = await Task.WhenAll(tasks);
            var contents = await Task.WhenAll(responses.Select(r => r.Content.ReadAsStringAsync()));

            var currentTier = responses[0].IsSuccessStatusCode ?
                JsonSerializer.Deserialize<TierModel>(contents[0], _jsonOptions) : null;
            var allTiers = responses[1].IsSuccessStatusCode ?
                JsonSerializer.Deserialize<List<TierModel>>(contents[1], _jsonOptions) : null;
            var presaleStats = responses[2].IsSuccessStatusCode ?
                JsonSerializer.Deserialize<PresaleStatusModel>(contents[2], _jsonOptions) : null;

            if (currentTier != null || allTiers != null || presaleStats != null)
            {
                return new PresaleDataModel
                {
                    CurrentTier = MapToTierDisplayModel(currentTier),
                    AllTiers = allTiers?.Select(MapToTierDisplayModel).ToList() ?? new List<TierDisplayModel>(),
                    PresaleStats = MapToPresaleStatsModel(presaleStats)
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting presale data");
            return null;
        }
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
            _logger.LogWarning(ex, "Error getting token info");
        }
        return GetFallbackTokenInfo();
    }

    private async Task<ContractInfoModel> GetContractInfoAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/presale/contracts");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var contractInfo = JsonSerializer.Deserialize<ContractAddressesModel>(content, _jsonOptions);

                if (contractInfo != null)
                {
                    return new ContractInfoModel
                    {
                        PresaleAddress = contractInfo.PresaleAddress,
                        TokenAddress = contractInfo.TokenAddress,
                        PaymentTokenAddress = contractInfo.PaymentTokenAddress,
                        NetworkId = contractInfo.NetworkId,
                        ChainName = contractInfo.ChainName
                    };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting contract info");
        }
        return GetFallbackContractInfo();
    }

    private async Task<ClaimableTokensModel?> GetClaimableTokensAsync(string address)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/presale/claimable/{address}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ClaimableTokensModel>(content, _jsonOptions);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting claimable tokens for {Address}", address);
        }
        return null;
    }

    private TierDisplayModel? MapToTierDisplayModel(TierModel? tier)
    {
        if (tier == null) return null;

        return new TierDisplayModel
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
            EndTime = tier.EndTime,
            Status = tier.Sold >= tier.TotalAllocation ? "SOLD OUT" :
                     tier.IsActive ? "ACTIVE" : "UPCOMING",
            StatusClass = tier.Sold >= tier.TotalAllocation ? "sold-out" :
                          tier.IsActive ? "active" : "upcoming"
        };
    }

    private PresaleStatsModel? MapToPresaleStatsModel(PresaleStatusModel? presaleStatus)
    {
        if (presaleStatus == null) return null;

        return new PresaleStatsModel
        {
            TotalRaised = presaleStatus.TotalRaised,
            FundingGoal = presaleStatus.FundingGoal,
            TokensSold = presaleStatus.TokensSold,
            TokensRemaining = 5000000000m - presaleStatus.TokensSold, // Total supply - sold
            ParticipantsCount = presaleStatus.ParticipantsCount,
            IsActive = true,
            FundingProgress = presaleStatus.FundingGoal > 0 ?
                (presaleStatus.TotalRaised / presaleStatus.FundingGoal) * 100 : 0
        };
    }

    private List<PurchaseOptionModel> GetPurchaseOptions()
    {
        return new List<PurchaseOptionModel>
        {
            new PurchaseOptionModel
            {
                Method = "USDC",
                Name = "USD Coin",
                Logo = "/images/usdc-logo.png",
                Description = "Purchase directly with USDC",
                IsRecommended = true,
                MinAmount = 100,
                MaxAmount = 100000,
                ProcessingTime = "Instant",
                Fees = "2.5% + Network fees"
            },
            new PurchaseOptionModel
            {
                Method = "ETH",
                Name = "Ethereum",
                Logo = "/images/eth-logo.png",
                Description = "Purchase with ETH (converted to USDC)",
                IsRecommended = false,
                MinAmount = 0.05m,
                MaxAmount = 50,
                ProcessingTime = "1-2 minutes",
                Fees = "3% + Network fees + Slippage"
            },
            new PurchaseOptionModel
            {
                Method = "CARD",
                Name = "Credit/Debit Card",
                Logo = "/images/card-logo.png",
                Description = "Purchase with credit or debit card",
                IsRecommended = false,
                MinAmount = 50,
                MaxAmount = 5000,
                ProcessingTime = "5-10 minutes",
                Fees = "5% + Processing fees"
            }
        };
    }

    private List<DexIntegrationModel> GetDexIntegrations()
    {
        return new List<DexIntegrationModel>
        {
            new DexIntegrationModel
            {
                Name = "1inch",
                Logo = "/images/1inch-logo.png",
                Description = "Best price aggregation across multiple DEXs",
                WidgetUrl = "https://app.1inch.io/#/1/simple/swap/USDC/",
                IsActive = false,
                LaunchDate = DateTime.UtcNow.AddDays(90)
            },
            new DexIntegrationModel
            {
                Name = "0x Protocol",
                Logo = "/images/0x-logo.png",
                Description = "Professional trading interface",
                WidgetUrl = "https://matcha.xyz/markets/1/",
                IsActive = false,
                LaunchDate = DateTime.UtcNow.AddDays(90)
            }
        };
    }

    #endregion

    #region Fallback Data Methods

    private PriceCalculationModel GetFallbackPriceCalculation(int tierId, decimal usdAmount)
    {
        var tierPrice = GetTierPrice(tierId);
        var tokensToReceive = usdAmount / tierPrice;
        var platformFee = usdAmount * 0.025m; // 2.5%
        var networkFee = 5.0m;

        return new PriceCalculationModel
        {
            TierId = tierId,
            TierName = GetTierName(tierId),
            UsdAmount = usdAmount,
            TokenPrice = tierPrice,
            TokensToReceive = tokensToReceive,
            PlatformFee = platformFee,
            NetworkFee = networkFee,
            TotalFees = platformFee + networkFee,
            TotalCost = usdAmount + platformFee + networkFee,
            MinPurchase = 100,
            MaxPurchase = 25000,
            UserExistingTokens = 0,
            VestingInfo = new VestingInfoModel
            {
                TgePercentage = 20,
                VestingMonths = 6,
                TgeTokens = tokensToReceive * 0.2m,
                VestedTokens = tokensToReceive * 0.8m
            },
            IsValid = usdAmount >= 100 && usdAmount <= 25000
        };
    }

    private UserTradeInfoModel GetFallbackUserTradeInfo(string address)
    {
        return new UserTradeInfoModel
        {
            Address = address,
            TotalTokensPurchased = 0,
            TotalUsdSpent = 0,
            ClaimableTokens = 0,
            LastClaimTime = null,
            NextVestingDate = null,
            NextVestingAmount = 0,
            TierPurchases = new List<TierPurchaseInfo>()
        };
    }

    private TierDisplayModel GetFallbackCurrentTier()
    {
        return new TierDisplayModel
        {
            Id = 2,
            Name = "Community Round",
            Price = 0.06m,
            TotalAllocation = 375000000m,
            Sold = 168750000m,
            Remaining = 206250000m,
            Progress = 45.0m,
            IsActive = true,
            IsSoldOut = false,
            MinPurchase = 100m,
            MaxPurchase = 25000m,
            VestingTGE = 20,
            VestingMonths = 6,
            EndTime = DateTime.UtcNow.AddDays(30),
            Status = "ACTIVE",
            StatusClass = "active"
        };
    }

    private List<TierDisplayModel> GetFallbackTiers()
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
                Progress = 45.0m,
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
                Status = "UPCOMING",
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
                Status = "UPCOMING",
                StatusClass = "upcoming",
                VestingTGE = 20,
                VestingMonths = 6
            }
        };
    }

    private PresaleStatsModel GetFallbackPresaleStats()
    {
        return new PresaleStatsModel
        {
            TotalRaised = 12500000m,
            FundingGoal = 87500000m,
            TokensSold = 750000000m,
            TokensRemaining = 4250000000m,
            ParticipantsCount = 2847,
            IsActive = true,
            FundingProgress = 14.3m
        };
    }

    private TokenInfoModel GetFallbackTokenInfo()
    {
        return new TokenInfoModel
        {
            TotalSupply = 5000000000m,
            CirculatingSupply = 1000000000m,
            CurrentPrice = 0.06m,
            MarketCap = 60000000m,
            HoldersCount = 2847
        };
    }

    private ContractInfoModel GetFallbackContractInfo()
    {
        return new ContractInfoModel
        {
            PresaleAddress = "0x1234567890123456789012345678901234567890",
            TokenAddress = "0x0987654321098765432109876543210987654321",
            PaymentTokenAddress = "0xA0b86a33E6441d0B0d7FAB4D4b6bBB8a4d8fC6c8",
            NetworkId = 1,
            ChainName = "Ethereum Mainnet"
        };
    }

    private DexInfoModel GetFallbackDexInfo()
    {
        return new DexInfoModel
        {
            IsLive = false,
            LaunchDate = DateTime.UtcNow.AddDays(90),
            TradingPairs = new List<TradingPairModel>
            {
                new TradingPairModel
                {
                    PairName = "TEACH/USDC",
                    DexName = "Uniswap V3",
                    DexLogo = "/images/uniswap-logo.png",
                    Liquidity = 0,
                    Volume24h = 0,
                    Price = 0.06m,
                    TradingUrl = "https://app.uniswap.org/swap"
                },
                new TradingPairModel
                {
                    PairName = "TEACH/ETH",
                    DexName = "SushiSwap",
                    DexLogo = "/images/sushiswap-logo.png",
                    Liquidity = 0,
                    Volume24h = 0,
                    Price = 0.06m,
                    TradingUrl = "https://app.sushi.com/swap"
                }
            },
            UpcomingListings = new List<ExchangeListingModel>
            {
                new ExchangeListingModel
                {
                    ExchangeName = "CoinGecko",
                    ExchangeLogo = "/images/coingecko-logo.png",
                    ListingType = "Tracking",
                    EstimatedDate = DateTime.UtcNow.AddDays(95),
                    Status = "Pending"
                }
            }
        };
    }

    private decimal GetTierPrice(int tierId)
    {
        return tierId switch
        {
            1 => 0.04m,
            2 => 0.06m,
            3 => 0.08m,
            4 => 0.10m,
            _ => 0.06m
        };
    }

    private string GetTierName(int tierId)
    {
        return tierId switch
        {
            1 => "Seed Round",
            2 => "Community Round",
            3 => "Growth Round",
            4 => "Final Round",
            _ => $"Tier {tierId}"
        };
    }

    #endregion

    #region Helper Models

    private class PresaleDataModel
    {
        public TierDisplayModel? CurrentTier { get; set; }
        public List<TierDisplayModel> AllTiers { get; set; } = new();
        public PresaleStatsModel? PresaleStats { get; set; }
    }

    #endregion
}