using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Diagnostics;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHomeService _homeService;
        private readonly ILogger<HomeController> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public HomeController(
            IHomeService homeService,
            ILogger<HomeController> logger)
        {
            _homeService = homeService ?? throw new ArgumentNullException(nameof(homeService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Main home page route
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var homeData = await _homeService.GetHomePageDataAsync();

                ViewBag.InitialData = homeData;
                ViewBag.JsonData = JsonSerializer.Serialize(homeData, _jsonOptions);

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page");

                var fallbackData = _homeService.GetFallbackHomeData();
                ViewBag.InitialData = fallbackData;
                ViewBag.JsonData = JsonSerializer.Serialize(fallbackData, _jsonOptions);

                return View();
            }
        }

        /// <summary>
        /// Get live statistics data (AJAX endpoint)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLiveStats()
        {
            try
            {
                var liveStats = await _homeService.GetLiveStatsAsync();
                return Json(liveStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving live statistics");
                return Json(new { error = "Unable to retrieve live statistics" });
            }
        }

        /// <summary>
        /// Get tier information for display (AJAX endpoint)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTierData()
        {
            try
            {
                var tierData = await _homeService.GetTierDisplayDataAsync();
                return Json(tierData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tier data");
                return Json(new { error = "Unable to retrieve tier data" });
            }
        }

        /// <summary>
        /// Get contract information (AJAX endpoint)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetContractInfo()
        {
            try
            {
                var contractInfo = await _homeService.GetContractInfoAsync();
                return Json(contractInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contract information");
                return Json(new { error = "Unable to retrieve contract information" });
            }
        }

        /// <summary>
        /// Get investment highlights
        /// </summary>
        [HttpGet]
        public IActionResult GetInvestmentHighlights()
        {
            try
            {
                var highlights = _homeService.GetInvestmentHighlights();
                return Json(highlights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving investment highlights");
                return Json(new { error = "Unable to retrieve investment highlights" });
            }
        }

        /// <summary>
        /// Check API health status
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                var isHealthy = await _homeService.CheckApiHealthAsync();
                return Json(new
                {
                    status = isHealthy ? "healthy" : "degraded",
                    timestamp = DateTime.UtcNow,
                    version = "1.0.0"
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Health check failed");
                return Json(new
                {
                    status = "degraded",
                    message = "Health check failed",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Error handling
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}