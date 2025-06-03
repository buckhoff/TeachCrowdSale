using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using TeachCrowdSale.Core.Interfaces.Services;

namespace TeachCrowdSale.Web.Controllers
{
    /// <summary>
    /// Analytics Dashboard Controller
    /// Follows home page patterns with service injection and caching
    /// </summary>
    public class AnalyticsController : Controller
    {
        private readonly IAnalyticsDashboardService _analyticsService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(
            IAnalyticsDashboardService analyticsService,
            IMemoryCache cache,
            ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Main analytics dashboard page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Get comprehensive dashboard data
                var dashboardData = await _analyticsService.GetDashboardDataAsync();

                if (dashboardData == null)
                {
                    _logger.LogWarning("Failed to load analytics dashboard data");
                    ViewBag.ErrorMessage = "Unable to load analytics data at this time. Please try again later.";
                }

                // Pass data to view via ViewBag for JavaScript consumption
                ViewBag.JsonData = dashboardData != null
                    ? JsonSerializer.Serialize(dashboardData, GetJsonOptions())
                    : "{}";

                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading analytics dashboard");
                ViewBag.ErrorMessage = "An error occurred while loading analytics data.";
                ViewBag.JsonData = "{}";
                return View();
            }
        }

        /// <summary>
        /// AJAX endpoint for dashboard data refresh
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                var data = await _analyticsService.GetDashboardDataAsync();
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard data via AJAX");
                return StatusCode(500, new { error = "Failed to load dashboard data" });
            }
        }

        /// <summary>
        /// Get token analytics data
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTokenAnalytics()
        {
            try
            {
                var data = await _analyticsService.GetTokenAnalyticsAsync();
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching token analytics");
                return StatusCode(500, new { error = "Failed to load token analytics" });
            }
        }

        /// <summary>
        /// Get presale analytics data
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPresaleAnalytics()
        {
            try
            {
                var data = await _analyticsService.GetPresaleAnalyticsAsync();
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching presale analytics");
                return StatusCode(500, new { error = "Failed to load presale analytics" });
            }
        }

        /// <summary>
        /// Get treasury analytics data
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTreasuryAnalytics()
        {
            try
            {
                var data = await _analyticsService.GetTreasuryAnalyticsAsync();
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching treasury analytics");
                return StatusCode(500, new { error = "Failed to load treasury analytics" });
            }
        }

        /// <summary>
        /// Get platform analytics data
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPlatformAnalytics()
        {
            try
            {
                var data = await _analyticsService.GetPlatformAnalyticsAsync();
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching platform analytics");
                return StatusCode(500, new { error = "Failed to load platform analytics" });
            }
        }

        /// <summary>
        /// Get price history for charts
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPriceHistory(DateTime? startDate = null, DateTime? endDate = null, string interval = "1d")
        {
            try
            {
                // Validate date range
                if (startDate.HasValue && endDate.HasValue && startDate >= endDate)
                {
                    return BadRequest(new { error = "Start date must be before end date" });
                }

                // Limit to reasonable date ranges
                var maxRange = TimeSpan.FromDays(365);
                if (startDate.HasValue && endDate.HasValue && (endDate.Value - startDate.Value) > maxRange)
                {
                    return BadRequest(new { error = $"Date range cannot exceed {maxRange.TotalDays} days" });
                }

                // Validate interval
                var validIntervals = new[] { "1h", "4h", "1d", "1w" };
                if (!validIntervals.Contains(interval))
                {
                    return BadRequest(new { error = "Invalid interval. Valid options: 1h, 4h, 1d, 1w" });
                }

                var data = await _analyticsService.GetPriceHistoryAsync(startDate, endDate, interval);
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching price history");
                return StatusCode(500, new { error = "Failed to load price history" });
            }
        }

        /// <summary>
        /// Get volume history for charts
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetVolumeHistory(DateTime? startDate = null, DateTime? endDate = null, string interval = "1d")
        {
            try
            {
                // Validate date range
                if (startDate.HasValue && endDate.HasValue && startDate >= endDate)
                {
                    return BadRequest(new { error = "Start date must be before end date" });
                }

                // Limit to reasonable date ranges
                var maxRange = TimeSpan.FromDays(365);
                if (startDate.HasValue && endDate.HasValue && (endDate.Value - startDate.Value) > maxRange)
                {
                    return BadRequest(new { error = $"Date range cannot exceed {maxRange.TotalDays} days" });
                }

                // Validate interval
                var validIntervals = new[] { "1h", "4h", "1d", "1w" };
                if (!validIntervals.Contains(interval))
                {
                    return BadRequest(new { error = "Invalid interval. Valid options: 1h, 4h, 1d, 1w" });
                }

                var data = await _analyticsService.GetVolumeHistoryAsync(startDate, endDate, interval);
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching volume history");
                return StatusCode(500, new { error = "Failed to load volume history" });
            }
        }

        /// <summary>
        /// Get tier performance analytics
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTierPerformance()
        {
            try
            {
                var data = await _analyticsService.GetTierPerformanceAsync();
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tier performance");
                return StatusCode(500, new { error = "Failed to load tier performance data" });
            }
        }

        /// <summary>
        /// Get analytics summary for quick overview
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAnalyticsSummary()
        {
            try
            {
                var tokenAnalytics = await _analyticsService.GetTokenAnalyticsAsync();
                var presaleAnalytics = await _analyticsService.GetPresaleAnalyticsAsync();

                if (tokenAnalytics == null || presaleAnalytics == null)
                {
                    return StatusCode(500, new { error = "Failed to load analytics data" });
                }

                var summary = new
                {
                    tokenPrice = tokenAnalytics.CurrentPrice,
                    priceChange24h = tokenAnalytics.PriceChange24h,
                    marketCap = tokenAnalytics.MarketCap,
                    volume24h = tokenAnalytics.Volume24h,
                    totalRaised = presaleAnalytics.TotalRaised,
                    fundingProgress = presaleAnalytics.FundingProgress,
                    participants = presaleAnalytics.ParticipantsCount,
                    currentTier = new
                    {
                        name = presaleAnalytics.CurrentTier.TierName,
                        price = presaleAnalytics.CurrentTier.Price,
                        progress = presaleAnalytics.CurrentTier.Progress
                    },
                    lastUpdated = DateTime.UtcNow
                };

                return Json(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching analytics summary");
                return StatusCode(500, new { error = "Failed to load analytics summary" });
            }
        }

        /// <summary>
        /// Export analytics data to CSV
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportData(string type = "summary", DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var exportData = new List<object>();
                var fileName = $"teachtoken_analytics_{type}_{DateTime.UtcNow:yyyyMMdd}.csv";

                switch (type.ToLower())
                {
                    case "token":
                        var tokenData = await _analyticsService.GetTokenAnalyticsAsync();
                        if (tokenData != null)
                        {
                            exportData.Add(new
                            {
                                Date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                                Price = tokenData.CurrentPrice,
                                MarketCap = tokenData.MarketCap,
                                Volume24h = tokenData.Volume24h,
                                PriceChange24h = tokenData.PriceChange24h,
                                HoldersCount = tokenData.HoldersCount,
                                CirculatingSupply = tokenData.CirculatingSupply
                            });
                        }
                        break;

                    case "presale":
                        var presaleData = await _analyticsService.GetPresaleAnalyticsAsync();
                        if (presaleData != null)
                        {
                            exportData.Add(new
                            {
                                Date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                                TotalRaised = presaleData.TotalRaised,
                                FundingGoal = presaleData.FundingGoal,
                                FundingProgress = presaleData.FundingProgress,
                                TokensSold = presaleData.TokensSold,
                                ParticipantsCount = presaleData.ParticipantsCount,
                                CurrentTierName = presaleData.CurrentTier.TierName,
                                CurrentTierPrice = presaleData.CurrentTier.Price,
                                CurrentTierProgress = presaleData.CurrentTier.Progress
                            });
                        }
                        break;

                    case "treasury":
                        var treasuryData = await _analyticsService.GetTreasuryAnalyticsAsync();
                        if (treasuryData != null)
                        {
                            exportData.Add(new
                            {
                                Date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                                TotalTreasuryValue = treasuryData.TotalTreasuryValue,
                                StabilityFundBalance = treasuryData.StabilityFundBalance,
                                OperationalRunwayMonths = treasuryData.OperationalRunwayMonths,
                                MonthlyBurnRate = treasuryData.MonthlyBurnRate
                            });
                        }
                        break;

                    case "price-history":
                        var priceHistory = await _analyticsService.GetPriceHistoryAsync(startDate, endDate);
                        if (priceHistory != null)
                        {
                            exportData.AddRange(priceHistory.Select(p => new
                            {
                                Date = p.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                                Price = p.Value,
                                Open = p.Open,
                                High = p.High,
                                Low = p.Low,
                                Close = p.Close,
                                Volume = p.Volume
                            }));
                        }
                        break;

                    default:
                        // Summary export
                        var summary = await _analyticsService.GetDashboardDataAsync();
                        if (summary != null)
                        {
                            exportData.Add(new
                            {
                                Date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                                TokenPrice = summary.TokenAnalytics.CurrentPrice,
                                MarketCap = summary.TokenAnalytics.MarketCap,
                                Volume24h = summary.TokenAnalytics.Volume24h,
                                TotalRaised = summary.PresaleAnalytics.TotalRaised,
                                FundingProgress = summary.PresaleAnalytics.FundingProgress,
                                ParticipantsCount = summary.PresaleAnalytics.ParticipantsCount,
                                TreasuryValue = summary.TreasuryAnalytics.TotalTreasuryValue,
                                OperationalRunway = summary.TreasuryAnalytics.OperationalRunwayMonths
                            });
                        }
                        break;
                }

                if (!exportData.Any())
                {
                    return NotFound(new { error = "No data available for export" });
                }

                // Generate CSV content
                var csvContent = GenerateCsvContent(exportData);
                var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);

                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting analytics data");
                return StatusCode(500, new { error = "Failed to export data" });
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet]
        public IActionResult HealthCheck()
        {
            try
            {
                return Json(new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    version = "1.0.0",
                    service = "Analytics Dashboard"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Analytics dashboard health check failed");
                return StatusCode(500, new
                {
                    status = "unhealthy",
                    timestamp = DateTime.UtcNow,
                    error = "Health check failed"
                });
            }
        }

        #region Helper Methods

        private static JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };
        }

        private static string GenerateCsvContent(List<object> data)
        {
            if (!data.Any()) return string.Empty;

            var csv = new System.Text.StringBuilder();

            // Get headers from first object
            var firstItem = data.First();
            var properties = firstItem.GetType().GetProperties();

            // Write headers
            csv.AppendLine(string.Join(",", properties.Select(p => p.Name)));

            // Write data rows
            foreach (var item in data)
            {
                var values = properties.Select(p =>
                {
                    var value = p.GetValue(item)?.ToString() ?? "";
                    // Escape commas and quotes in CSV
                    if (value.Contains(",") || value.Contains("\""))
                    {
                        value = $"\"{value.Replace("\"", "\"\"")}\"";
                    }
                    return value;
                });

                csv.AppendLine(string.Join(",", values));
            }

            return csv.ToString();
        }

        #endregion
    }
}