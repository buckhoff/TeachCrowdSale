using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using TeachCrowdSale.Core.Models.Request;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Web.Controllers
{
    [Route("buy")]
    [Route("trade")]
    public class BuyTradeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<BuyTradeController> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        // Cache keys and durations
        private const string CACHE_KEY_BUYTRADE_DATA = "buytrade_page_data";
        private const string CACHE_KEY_DEX_INFO = "dex_info";
        private const string CACHE_KEY_PURCHASE_OPTIONS = "purchase_options";
        private readonly TimeSpan _shortCacheDuration = TimeSpan.FromMinutes(2);
        private readonly TimeSpan _mediumCacheDuration = TimeSpan.FromMinutes(10);
        private readonly TimeSpan _longCacheDuration = TimeSpan.FromMinutes(30);

        public BuyTradeController(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            ILogger<BuyTradeController> logger)
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
        /// Main buy/trade page route
        /// </summary>
        [HttpGet("")]
        [HttpGet("buy")]
        [HttpGet("trade")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var buyTradeData = await GetBuyTradeDataAsync();

                ViewBag.InitialData = buyTradeData;
                ViewBag.JsonData = JsonSerializer.Serialize(buyTradeData, _jsonOptions);

                // Set page-specific metadata
                ViewData["Title"] = "Buy TEACH Token - Direct Presale & DEX Trading";
                ViewData["Description"] = "Purchase TEACH tokens directly from our presale or trade on decentralized exchanges. Multi-year treasury runway with early investor pricing.";

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading buy/trade page");

                ViewBag.InitialData = GetFallbackBuyTradeData();
                ViewBag.JsonData = JsonSerializer.Serialize(GetFallbackBuyTradeData(), _jsonOptions);

                return View();
            }
        }

        /// <summary>
        /// endpoint for getting aggregated buy/trade page data
        /// </summary>
        [HttpGet("data")]
        [ResponseCache(Duration = 30)]
        public async Task<IActionResult> GetBuyTradeData()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/buytrade/data");
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving buy/trade page data");
                return StatusCode(500, new { message = "Error retrieving data", error = ex.Message });
            }
        }

        /// <summary>
        /// endpoint for price calculation
        /// </summary>
        [HttpPost("calculate-price")]
        public async Task<ActionResult<PriceCalculationModel>> CalculatePrice([FromBody] PriceCalculationRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/buytrade/calculate-price", request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                var content = await response.Content.ReadAsStringAsync();
                var calculation = JsonSerializer.Deserialize<PriceCalculationModel>(content, _jsonOptions);

                return Ok(calculation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating price for purchase");
                return StatusCode(500, new { message = "Error calculating price" });
            }
        }

        /// <summary>
        /// endpoint for user trade information
        /// </summary>
        [HttpGet("user/{address}")]
        [ResponseCache(Duration = 60)]
        public async Task<ActionResult<UserTradeInfoModel>> GetUserTradeInfo([FromRoute] string address)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/buytrade/user/{address}");

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, new { message = "Error retrieving user data" });
                }

                var content = await response.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<UserTradeInfoModel>(content, _jsonOptions);

                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user trade info for {Address}", address);
                return StatusCode(500, new { message = "Error retrieving user data" });
            }
        }

        /// <summary>
        /// API endpoint for DEX information
        /// </summary>
        [HttpGet("dex-info")]
        [ResponseCache(Duration = 300)]
        public async Task<ActionResult<DexInfoModel>> GetDexInfo()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_DEX_INFO, out DexInfoModel cachedDexInfo))
                {
                    return Ok(cachedDexInfo);
                }

                var response = await _httpClient.GetAsync("/api/buytrade/dex-info");

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(500, new { message = "Error retrieving DEX information" });
                }

                var content = await response.Content.ReadAsStringAsync();
                var dexInfo = JsonSerializer.Deserialize<DexInfoModel>(content, _jsonOptions);

                _cache.Set(CACHE_KEY_DEX_INFO, dexInfo, _mediumCacheDuration);
                return Ok(dexInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving DEX information");
                return StatusCode(500, new { message = "Error retrieving DEX data" });
            }
        }

        /// <summary>
        /// Purchase validation endpoint
        /// </summary>
        [HttpPost("validate-purchase")]
        public async Task<ActionResult> ValidatePurchase([FromBody] PriceCalculationRequest request)
        {
            try
            {
                // Forward to the presale API for validation
                var response = await _httpClient.PostAsJsonAsync("/api/presale/purchase-validation", new
                {
                    Address = request.Address,
                    TierId = request.TierId,
                    Amount = request.UsdAmount
                });

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                var content = await response.Content.ReadAsStringAsync();
                return Ok(JsonSerializer.Deserialize<object>(content, _jsonOptions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating purchase");
                return StatusCode(500, new { message = "Error validating purchase" });
            }
        }

        /// <summary>
        /// Wallet connection helper endpoint
        /// </summary>
        [HttpGet("wallet-info/{address}")]
        [ResponseCache(Duration = 30)]
        public async Task<ActionResult> GetWalletInfo([FromRoute] string address)
        {
            try
            {
                // Get wallet balances and info
                var tasks = new[]
                {
                    _httpClient.GetAsync($"/api/buytrade/user/{address}"),
                    _httpClient.GetAsync($"/api/presale/claimable/{address}")
                };

                var responses = await Task.WhenAll(tasks);
                var contents = await Task.WhenAll(responses.Select(r => r.Content.ReadAsStringAsync()));

                var walletInfo = new
                {
                    userInfo = responses[0].IsSuccessStatusCode ?
                        JsonSerializer.Deserialize<UserTradeInfoModel>(contents[0], _jsonOptions) : null,
                    claimableInfo = responses[1].IsSuccessStatusCode ?
                        JsonSerializer.Deserialize<object>(contents[1], _jsonOptions) : null,
                    isConnected = true
                };

                return Ok(walletInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving wallet info for {Address}", address);
                return StatusCode(500, new { message = "Error retrieving wallet information" });
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Get comprehensive buy/trade page data
        /// </summary>
        private async Task<BuyTradeDataModel> GetBuyTradeDataAsync()
        {
            if (_cache.TryGetValue(CACHE_KEY_BUYTRADE_DATA, out BuyTradeDataModel cachedData))
            {
                return cachedData;
            }

            try
            {
                var response = await _httpClient.GetAsync("/api/buytrade/data");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var buyTradeData = JsonSerializer.Deserialize<BuyTradeDataModel>(content, _jsonOptions);

                    if (buyTradeData != null)
                    {
                        _cache.Set(CACHE_KEY_BUYTRADE_DATA, buyTradeData, _shortCacheDuration);
                        return buyTradeData;
                    }
                }

                return GetFallbackBuyTradeData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading comprehensive buy/trade data");
                return GetFallbackBuyTradeData();
            }
        }

        /// <summary>
        /// Get fallback data when API calls fail
        /// </summary>
        private BuyTradeDataModel GetFallbackBuyTradeData()
        {
            return new BuyTradeDataModel
            {
                CurrentTier = new TierDisplayModel
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
                },
                AllTiers = GetFallbackTiers(),
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
                TokenInfo = new TokenInfoModel
                {
                    TotalSupply = 5000000000m,
                    CirculatingSupply = 1000000000m,
                    CurrentPrice = 0.06m,
                    MarketCap = 60000000m,
                    HoldersCount = 2847
                },
                ContractInfo = new ContractInfoModel
                {
                    PresaleAddress = "0x1234567890123456789012345678901234567890",
                    TokenAddress = "0x0987654321098765432109876543210987654321",
                    PaymentTokenAddress = "0xA0b86a33E6441d0B0d7FAB4D4b6bBB8a4d8fC6c8",
                    NetworkId = 1,
                    ChainName = "Ethereum Mainnet"
                },
                PurchaseOptions = GetFallbackPurchaseOptions(),
                DexIntegrations = GetFallbackDexIntegrations()
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
                    StatusClass = "sold-out"
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
                    StatusClass = "active"
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
                    StatusClass = "upcoming"
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
                    StatusClass = "upcoming"
                }
            };
        }

        private List<PurchaseOptionModel> GetFallbackPurchaseOptions()
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
                }
            };
        }

        private List<DexIntegrationModel> GetFallbackDexIntegrations()
        {
            return new List<DexIntegrationModel>
            {
                new DexIntegrationModel
                {
                    Name = "1inch",
                    Logo = "/images/1inch-logo.png",
                    Description = "Best price aggregation across multiple DEXs",
                    WidgetUrl = "https://app.1inch.io/",
                    IsActive = false,
                    LaunchDate = DateTime.UtcNow.AddDays(90)
                },
                new DexIntegrationModel
                {
                    Name = "0x Protocol",
                    Logo = "/images/0x-logo.png",
                    Description = "Professional trading interface",
                    WidgetUrl = "https://matcha.xyz/",
                    IsActive = false,
                    LaunchDate = DateTime.UtcNow.AddDays(90)
                }
            };
        }

        #endregion
    }
}