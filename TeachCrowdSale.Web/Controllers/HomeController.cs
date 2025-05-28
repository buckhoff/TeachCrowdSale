using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Web.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<HomeController> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        // Cache keys and durations
        private const string CACHE_KEY_HOME_DATA = "home_page_data";
        private const string CACHE_KEY_LIVE_STATS = "live_stats";
        private const string CACHE_KEY_TIERS = "tier_data";
        private readonly TimeSpan _shortCacheDuration = TimeSpan.FromMinutes(2);
        private readonly TimeSpan _liveCacheDuration = TimeSpan.FromMinutes(1);
        private readonly TimeSpan _mediumCacheDuration = TimeSpan.FromMinutes(10);

        public HomeController(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            ILogger<HomeController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("TeachAPI");
            _cache = cache;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Main home page route
        /// </summary>
        [HttpGet("")]
        [HttpGet("home")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var homeData = await GetHomePageDataAsync();

                ViewBag.InitialData = homeData;
                ViewBag.JsonData = JsonSerializer.Serialize(homeData, _jsonOptions);

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page");

                ViewBag.InitialData = GetFallbackHomeData();
                ViewBag.JsonData = JsonSerializer.Serialize(GetFallbackHomeData(), _jsonOptions);

                return View();
            }
        }

        /// <summary>
        /// API endpoint for getting aggregated home page data
        /// </summary>
        [HttpGet("api/home/data")]
        [ResponseCache(Duration = 120)]
        public async Task<ActionResult<HomePageDataModel>> GetHomeData()
        {
            try
            {
                var homeData = await GetHomePageDataAsync();
                return Ok(homeData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving home page data");
                return StatusCode(500, new { message = "Error retrieving data", error = ex.Message });
            }
        }

        /// <summary>
        /// API endpoint for live statistics
        /// </summary>
        [HttpGet("api/home/live-stats")]
        [ResponseCache(Duration = 30)]
        public async Task<ActionResult<LiveStatsModel>> GetLiveStats()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_LIVE_STATS, out LiveStatsModel cachedStats))
                {
                    return Ok(cachedStats);
                }

                var stats = new LiveStatsModel();

                // Get presale stats
                var presaleResponse = await _httpClient.GetAsync("/api/presale/status");
                if (presaleResponse.IsSuccessStatusCode)
                {
                    var presaleContent = await presaleResponse.Content.ReadAsStringAsync();
                    var presaleStats = JsonSerializer.Deserialize<PresaleStatusModel>(presaleContent, _jsonOptions);

                    if (presaleStats != null)
                    {
                        stats.TotalRaised = presaleStats.TotalRaised;
                        stats.TokensSold = presaleStats.TokensSold;
                        stats.ParticipantsCount = presaleStats.ParticipantsCount;
                        stats.IsPresaleActive = true;
                    }
                }

                // Get current tier
                var tierResponse = await _httpClient.GetAsync("/api/presale/current-tier");
                if (tierResponse.IsSuccessStatusCode)
                {
                    var tierContent = await tierResponse.Content.ReadAsStringAsync();
                    var currentTier = JsonSerializer.Deserialize<TierModel>(tierContent, _jsonOptions);

                    if (currentTier != null)
                    {
                        stats.CurrentTierPrice = currentTier.Price;
                        stats.CurrentTierName = currentTier.Name;
                        stats.CurrentTierProgress = currentTier.TotalAllocation > 0 ?
                            (currentTier.Sold / currentTier.TotalAllocation) * 100 : 0;
                    }
                }

                // Get token info
                var tokenResponse = await _httpClient.GetAsync("/api/tokeninfo");
                if (tokenResponse.IsSuccessStatusCode)
                {
                    var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
                    var tokenInfo = JsonSerializer.Deserialize<TokenInfoModel>(tokenContent, _jsonOptions);

                    if (tokenInfo != null)
                    {
                        stats.TokenPrice = tokenInfo.CurrentPrice;
                        stats.MarketCap = tokenInfo.MarketCap;
                        stats.HoldersCount = tokenInfo.HoldersCount;
                    }
                }

                stats.UpdatedAt = DateTime.UtcNow;
                _cache.Set(CACHE_KEY_LIVE_STATS, stats, _liveCacheDuration);

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving live statistics");
                return StatusCode(500, new { message = "Error retrieving live stats" });
            }
        }

        /// <summary>
        /// API endpoint for tier information
        /// </summary>
        [HttpGet("api/home/tiers")]
        [ResponseCache(Duration = 300)]
        public async Task<ActionResult<List<TierDisplayModel>>> GetTiersInfo()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_TIERS, out List<TierDisplayModel> cachedTiers))
                {
                    return Ok(cachedTiers);
                }

                var response = await _httpClient.GetAsync("/api/presale/tiers");
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(500, new { message = "Error retrieving tier data from API" });
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
                return Ok(tierDisplayModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tier information");
                return StatusCode(500, new { message = "Error retrieving tier data" });
            }
        }

        /// <summary>
        /// API endpoint for contract addresses
        /// </summary>
        [HttpGet("api/home/contracts")]
        [ResponseCache(Duration = 3600)]
        public async Task<ActionResult<ContractInfoModel>> GetContractInfo()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/presale/contracts");
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(500, new { message = "Error retrieving contract data from API" });
                }

                var content = await response.Content.ReadAsStringAsync();
                var contractInfo = JsonSerializer.Deserialize<ContractInfoModel>(content, _jsonOptions);

                return Ok(contractInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contract information");
                return StatusCode(500, new { message = "Error retrieving contract data" });
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("api/home/health")]
        public async Task<ActionResult> HealthCheck()
        {
            try
            {
                var response = await _httpClient.GetAsync("/health");
                if (response.IsSuccessStatusCode)
                {
                    return Ok(new { status = "healthy", timestamp = DateTime.UtcNow, version = "1.0.0" });
                }
                else
                {
                    return StatusCode(503, new { status = "degraded", message = "API unavailable", timestamp = DateTime.UtcNow });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Health check failed");
                return StatusCode(503, new { status = "degraded", message = "API connection failed", timestamp = DateTime.UtcNow });
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Get comprehensive home page data
        /// </summary>
        private async Task<HomePageDataModel> GetHomePageDataAsync()
        {
            if (_cache.TryGetValue(CACHE_KEY_HOME_DATA, out HomePageDataModel cachedData))
            {
                return cachedData;
            }

            var homeData = new HomePageDataModel();

            try
            {
                // Get presale statistics
                var presaleResponse = await _httpClient.GetAsync("/api/presale/status");
                if (presaleResponse.IsSuccessStatusCode)
                {
                    var presaleContent = await presaleResponse.Content.ReadAsStringAsync();
                    var presaleStats = JsonSerializer.Deserialize<PresaleStatusModel>(presaleContent, _jsonOptions);

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
                }

                // Get current tier
                var tierResponse = await _httpClient.GetAsync("/api/presale/current-tier");
                if (tierResponse.IsSuccessStatusCode)
                {
                    var tierContent = await tierResponse.Content.ReadAsStringAsync();
                    var currentTier = JsonSerializer.Deserialize<TierModel>(tierContent, _jsonOptions);

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
                }

                // Get token information
                var tokenResponse = await _httpClient.GetAsync("/api/tokeninfo");
                if (tokenResponse.IsSuccessStatusCode)
                {
                    var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
                    var tokenInfo = JsonSerializer.Deserialize<TokenInfoModel>(tokenContent, _jsonOptions);

                    if (tokenInfo != null)
                    {
                        homeData.TokenInfo = tokenInfo;
                    }
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

        /// <summary>
        /// Get fallback data when API calls fail
        /// </summary>
        private HomePageDataModel GetFallbackHomeData()
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

        /// <summary>
        /// Get investment highlight data
        /// </summary>
        private List<InvestmentHighlightModel> GetInvestmentHighlights()
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

        #endregion
    }
}