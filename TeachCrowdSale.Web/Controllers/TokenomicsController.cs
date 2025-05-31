using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models.Tokenomics;

namespace TeachCrowdSale.Web.Controllers
{
    [Route("tokenomics")]
    public class TokenomicsController : Controller
    {
        private readonly ITokenomicsService _tokenomicsService;
        private readonly ILogger<TokenomicsController> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public TokenomicsController(
            ITokenomicsService tokenomicsService,
            ILogger<TokenomicsController> logger)
        {
            _tokenomicsService = tokenomicsService ?? throw new ArgumentNullException(nameof(tokenomicsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Main tokenomics page route
        /// </summary>
        [HttpGet("")]
        [HttpGet("index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var tokenomicsData = await _tokenomicsService.GetTokenomicsDataAsync();

                ViewBag.InitialData = tokenomicsData;
                ViewBag.JsonData = JsonSerializer.Serialize(tokenomicsData, _jsonOptions);

                // Set page-specific metadata
                ViewData["Title"] = "TEACH Token Tokenomics - Complete Financial Architecture";
                ViewData["Description"] = "Explore TEACH token's institutional-grade tokenomics with multi-year treasury runway, deflationary mechanics, and sustainable value creation.";

                return View(tokenomicsData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tokenomics page");

                var fallbackData = _tokenomicsService.GetFallbackTokenomicsData();
                ViewBag.InitialData = fallbackData;
                ViewBag.JsonData = JsonSerializer.Serialize(fallbackData, _jsonOptions);

                return View(fallbackData);
            }
        }

        /// <summary>
        /// AJAX endpoint for live token metrics
        /// </summary>
        [HttpGet("live-metrics")]
        public async Task<IActionResult> GetLiveMetrics()
        {
            try
            {
                var metrics = await _tokenomicsService.GetLiveTokenMetricsAsync();
                return Json(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving live token metrics");
                return Json(new { error = "Unable to retrieve live metrics" });
            }
        }

        /// <summary>
        /// AJAX endpoint for supply breakdown data
        /// </summary>
        [HttpGet("supply-breakdown")]
        public async Task<IActionResult> GetSupplyBreakdown()
        {
            try
            {
                var supplyData = await _tokenomicsService.GetSupplyBreakdownAsync();
                return Json(supplyData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supply breakdown");
                return Json(new { error = "Unable to retrieve supply data" });
            }
        }

        /// <summary>
        /// AJAX endpoint for vesting schedule data
        /// </summary>
        [HttpGet("vesting-schedule")]
        public async Task<IActionResult> GetVestingSchedule()
        {
            try
            {
                var vestingData = await _tokenomicsService.GetVestingScheduleAsync();
                return Json(vestingData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vesting schedule");
                return Json(new { error = "Unable to retrieve vesting data" });
            }
        }

        /// <summary>
        /// AJAX endpoint for burn mechanism data
        /// </summary>
        [HttpGet("burn-mechanics")]
        public async Task<IActionResult> GetBurnMechanics()
        {
            try
            {
                var burnData = await _tokenomicsService.GetBurnMechanicsAsync();
                return Json(burnData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving burn mechanics");
                return Json(new { error = "Unable to retrieve burn data" });
            }
        }

        /// <summary>
        /// AJAX endpoint for treasury analytics
        /// </summary>
        [HttpGet("treasury-analytics")]
        public async Task<IActionResult> GetTreasuryAnalytics()
        {
            try
            {
                var treasuryData = await _tokenomicsService.GetTreasuryAnalyticsAsync();
                return Json(treasuryData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving treasury analytics");
                return Json(new { error = "Unable to retrieve treasury data" });
            }
        }

        /// <summary>
        /// AJAX endpoint for utility features
        /// </summary>
        [HttpGet("utility-features")]
        public async Task<IActionResult> GetUtilityFeatures()
        {
            try
            {
                var utilityData = await _tokenomicsService.GetUtilityFeaturesAsync();
                return Json(utilityData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving utility features");
                return Json(new { error = "Unable to retrieve utility data" });
            }
        }

        /// <summary>
        /// AJAX endpoint for governance information
        /// </summary>
        [HttpGet("governance-info")]
        public async Task<IActionResult> GetGovernanceInfo()
        {
            try
            {
                var governanceData = await _tokenomicsService.GetGovernanceInfoAsync();
                return Json(governanceData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving governance information");
                return Json(new { error = "Unable to retrieve governance data" });
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                var isHealthy = await _tokenomicsService.CheckApiHealthAsync();
                return Json(new
                {
                    status = isHealthy ? "healthy" : "degraded",
                    timestamp = DateTime.UtcNow,
                    service = "tokenomics"
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Tokenomics health check failed");
                return Json(new
                {
                    status = "degraded",
                    message = "Health check failed",
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}