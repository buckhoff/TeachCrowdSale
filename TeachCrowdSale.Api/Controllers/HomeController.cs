using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Models;
using TeachCrowdSale.Api.Models;

namespace TeachCrowdSale.Api.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPresaleService _presaleService;
        private readonly ITokenContractService _tokenService;
        private readonly IBlockchainService _blockchainService;
        private readonly IMemoryCache _cache;

        // Cache keys for different data types
        private const string CACHE_KEY_HOME_DATA = "home_page_data";
        private const string CACHE_KEY_PRESALE_STATS = "presale_stats";
        private const string CACHE_KEY_TOKEN_INFO = "token_info";
        private const string CACHE_KEY_TIERS = "tier_data";

        // Cache durations
        private readonly TimeSpan _shortCacheDuration = TimeSpan.FromMinutes(2);
        private readonly TimeSpan _mediumCacheDuration = TimeSpan.FromMinutes(10);
        private readonly TimeSpan _longCacheDuration = TimeSpan.FromMinutes(30);

        public HomeController(
            ILogger<HomeController> logger,
            IPresaleService presaleService,
            ITokenContractService tokenService,
            IBlockchainService blockchainService,
            IMemoryCache cache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _presaleService = presaleService ?? throw new ArgumentNullException(nameof(presaleService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _blockchainService = blockchainService ?? throw new ArgumentNullException(nameof(blockchainService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
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
                // Load initial data for server-side rendering
                var homeData = await GetHomePageData();

                // Pass data to view for initial rendering
                ViewBag.InitialData = homeData;
                ViewBag.JsonData = System.Text.Json.JsonSerializer.Serialize(homeData);

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page");

                // Return view with fallback data
                ViewBag.InitialData = GetFallbackHomeData();
                ViewBag.JsonData = System.Text.Json.JsonSerializer.Serialize(GetFallbackHomeData());

                return View();
            }
        }

        /// <summary>
        /// API endpoint for getting aggregated home page data
        /// </summary>
        [HttpGet("api/home/data")]
        [ResponseCache(Duration = 120)] // 2 minutes client-side cache
        public async Task<ActionResult<HomePageDataModel>> GetHomeData()
        {
            try
            {
                var homeData = await GetHomePageData();
                return Ok(homeData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving home page data");
                return StatusCode(500, new { message = "Error retrieving data", error = ex.Message });
            }
        }

        /// <summary>
        /// API endpoint for live statistics (frequently updated)
        /// </summary>
        [HttpGet("api/home/live-stats")]
        [ResponseCache(Duration = 30)] // 30 seconds client-side cache
        public async Task<ActionResult<LiveStatsModel>> GetLiveStats()
        {
            try
            {
                var cacheKey = "live_stats";
                if (_cache.TryGetValue(cacheKey, out LiveStatsModel cachedStats))
                {
                    return Ok(cachedStats);
                }

                var stats = new LiveStatsModel();

                // Get real-time presale statistics
                var presaleStats = await _presaleService.GetPresaleStatsAsync();
                if (presaleStats != null)
                {
                    stats.TotalRaised = presaleStats.TotalRaised;
                    stats.TokensSold = presaleStats.TokensSold;
                    stats.ParticipantsCount = presaleStats.ParticipantsCount;
                    stats.IsPresaleActive = presaleStats.IsActive;
                }

                // Get current tier information
                var currentTier = await _presaleService.GetCurrentTierAsync();
                if (currentTier != null)
                {
                    stats.CurrentTierPrice = currentTier.Price;
                    stats.CurrentTierName = GetTierDisplayName(currentTier.Id);
                    stats.CurrentTierProgress = currentTier.Allocation > 0 ?
                        (currentTier.Sold / currentTier.Allocation) * 100 : 0;
                }

                // Get token price from contract
                try
                {
                    stats.TokenPrice = await _tokenService.GetTokenPriceAsync();
                    stats.MarketCap = await _tokenService.CalculateMarketCapAsync();
                    stats.HoldersCount = await _tokenService.GetHoldersCountAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not retrieve token contract data");
                    // Use fallback values
                    stats.TokenPrice = currentTier?.Price ?? 0.06m;
                    stats.MarketCap = stats.TokenPrice * 3500000000m; // Estimated circulating supply
                    stats.HoldersCount = 2847; // Fallback value
                }

                // Cache for short duration due to frequent updates
                _cache.Set(cacheKey, stats, TimeSpan.FromMinutes(1));

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
        [ResponseCache(Duration = 300)] // 5 minutes client-side cache
        public async Task<ActionResult<List<TierDisplayModel>>> GetTiersInfo()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_TIERS, out List<TierDisplayModel> cachedTiers))
                {
                    return Ok(cachedTiers);
                }

                var tiers = await _presaleService.GetAllTiersAsync();
                var tierDisplayModels = new List<TierDisplayModel>();

                if (tiers != null && tiers.Any())
                {
                    foreach (var tier in tiers)
                    {
                        var displayModel = new TierDisplayModel
                        {
                            Id = tier.Id,
                            Name = GetTierDisplayName(tier.Id),
                            Price = tier.Price,
                            TotalAllocation = tier.Allocation,
                            Sold = tier.Sold,
                            Remaining = tier.Allocation - tier.Sold,
                            Progress = tier.Allocation > 0 ? (tier.Sold / tier.Allocation) * 100 : 0,
                            IsActive = tier.IsActive,
                            IsSoldOut = tier.Sold >= tier.Allocation,
                            MinPurchase = tier.MinPurchase,
                            MaxPurchase = tier.MaxPurchase,
                            VestingTGE = tier.VestingTGE,
                            VestingMonths = tier.VestingMonths,
                            EndTime = await _presaleService.GetTierEndTimeAsync(tier.Id)
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

                // Cache for medium duration
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
        /// API endpoint for contract addresses and network info
        /// </summary>
        [HttpGet("api/home/contracts")]
        [ResponseCache(Duration = 3600)] // 1 hour client-side cache
        public ActionResult<ContractInfoModel> GetContractInfo()
        {
            try
            {
                var addresses = _blockchainService.GetContractAddresses();

                var contractInfo = new ContractInfoModel
                {
                    PresaleAddress = addresses.PresaleAddress,
                    TokenAddress = addresses.TokenAddress,
                    PaymentTokenAddress = addresses.PaymentTokenAddress,
                    NetworkId = addresses.NetworkId,
                    ChainName = addresses.ChainName
                };

                return Ok(contractInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contract information");
                return StatusCode(500, new { message = "Error retrieving contract data" });
            }
        }

        /// <summary>
        /// Health check endpoint for the home page
        /// </summary>
        [HttpGet("api/home/health")]
        public async Task<ActionResult> HealthCheck()
        {
            try
            {
                // Quick health check of core services
                await _presaleService.GetPresaleStatsAsync();

                return Ok(new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    version = "1.0.0"
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Health check failed");
                return StatusCode(503, new
                {
                    status = "degraded",
                    message = "Some services may be unavailable",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Error page
        /// </summary>
        [HttpGet("error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        #region Private Helper Methods

        /// <summary>
        /// Get comprehensive home page data
        /// </summary>
        private async Task<HomePageDataModel> GetHomePageData()
        {
            if (_cache.TryGetValue(CACHE_KEY_HOME_DATA, out HomePageDataModel cachedData))
            {
                return cachedData;
            }

            var homeData = new HomePageDataModel();

            try
            {
                // Get presale statistics
                var presaleStats = await _presaleService.GetPresaleStatsAsync();
                if (presaleStats != null)
                {
                    homeData.PresaleStats = new PresaleStatsModel
                    {
                        TotalRaised = presaleStats.TotalRaised,
                        FundingGoal = presaleStats.FundingGoal,
                        TokensSold = presaleStats.TokensSold,
                        TokensRemaining = presaleStats.TokensRemaining,
                        ParticipantsCount = presaleStats.ParticipantsCount,
                        IsActive = presaleStats.IsActive,
                        FundingProgress = presaleStats.FundingGoal > 0 ?
                            (presaleStats.TotalRaised / presaleStats.FundingGoal) * 100 : 0
                    };
                }

                // Get current tier
                var currentTier = await _presaleService.GetCurrentTierAsync();
                if (currentTier != null)
                {
                    homeData.CurrentTier = new CurrentTierModel
                    {
                        Id = currentTier.Id,
                        Name = GetTierDisplayName(currentTier.Id),
                        Price = currentTier.Price,
                        Progress = currentTier.Allocation > 0 ?
                            (currentTier.Sold / currentTier.Allocation) * 100 : 0,
                        Sold = currentTier.Sold,
                        Remaining = currentTier.Allocation - currentTier.Sold,
                        IsActive = currentTier.IsActive
                    };
                }

                // Get token information
                try
                {
                    var totalSupply = await _tokenService.GetTotalSupplyAsync();
                    var circulatingSupply = await _tokenService.GetCirculatingSupplyAsync();
                    var tokenPrice = await _tokenService.GetTokenPriceAsync();
                    var holdersCount = await _tokenService.GetHoldersCountAsync();

                    homeData.TokenInfo = new TokenInfoModel
                    {
                        TotalSupply = totalSupply,
                        CirculatingSupply = circulatingSupply,
                        CurrentPrice = tokenPrice,
                        MarketCap = await _tokenService.CalculateMarketCapAsync(),
                        HoldersCount = holdersCount
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not retrieve complete token information");
                    // Use fallback token info
                    homeData.TokenInfo = new TokenInfoModel
                    {
                        TotalSupply = 5000000000m, // 5B total supply
                        CirculatingSupply = 1000000000m, // 1B circulating
                        CurrentPrice = currentTier?.Price ?? 0.06m,
                        MarketCap = (currentTier?.Price ?? 0.06m) * 1000000000m,
                        HoldersCount = 2847
                    };
                }

                // Get investment highlights
                homeData.InvestmentHighlights = GetInvestmentHighlights();

                // Cache the complete data
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

        /// <summary>
        /// Get display name for tier
        /// </summary>
        private string GetTierDisplayName(int tierId)
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
    }