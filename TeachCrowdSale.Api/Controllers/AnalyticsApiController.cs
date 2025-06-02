// TeachCrowdSale.Api/Controllers/AnalyticsApiController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models.Response;
using TeachCrowdSale.Core.Models.Treasury;

namespace TeachCrowdSale.Api.Controllers
{
    [EnableRateLimiting("Analytics")]
    [ApiController]
    [Route("api/analytics")]
    public class AnalyticsApiController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<AnalyticsApiController> _logger;

        public AnalyticsApiController(
            IAnalyticsService analyticsService,
            ILogger<AnalyticsApiController> logger)
        {
            _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get comprehensive analytics dashboard data
        /// </summary>
        [HttpGet("dashboard")]
        [ResponseCache(Duration = 120)] // 2 minutes cache
        public async Task<ActionResult<AnalyticsDashboardModel>> GetAnalyticsDashboard()
        {
            try
            {
                var dashboard = await _analyticsService.GetAnalyticsDashboardAsync();
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving analytics dashboard");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving analytics dashboard",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get token-specific analytics
        /// </summary>
        [HttpGet("token")]
        [ResponseCache(Duration = 300)] // 5 minutes cache
        public async Task<ActionResult<TokenAnalyticsModel>> GetTokenAnalytics()
        {
            try
            {
                var tokenAnalytics = await _analyticsService.GetTokenAnalyticsAsync();
                return Ok(tokenAnalytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving token analytics");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving token analytics",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get presale-specific analytics
        /// </summary>
        [HttpGet("presale")]
        [ResponseCache(Duration = 180)] // 3 minutes cache
        public async Task<ActionResult<PresaleAnalyticsModel>> GetPresaleAnalytics()
        {
            try
            {
                var presaleAnalytics = await _analyticsService.GetPresaleAnalyticsAsync();
                return Ok(presaleAnalytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving presale analytics");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving presale analytics",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get platform-specific analytics
        /// </summary>
        [HttpGet("platform")]
        [ResponseCache(Duration = 600)] // 10 minutes cache (less frequent updates)
        public async Task<ActionResult<PlatformAnalyticsModel>> GetPlatformAnalytics()
        {
            try
            {
                var platformAnalytics = await _analyticsService.GetPlatformAnalyticsAsync();
                return Ok(platformAnalytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving platform analytics");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving platform analytics",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get treasury-specific analytics
        /// </summary>
        [HttpGet("treasury")]
        [ResponseCache(Duration = 900)] // 15 minutes cache
        public async Task<ActionResult<TreasuryAnalyticsModel>> GetTreasuryAnalytics()
        {
            try
            {
                var treasuryAnalytics = await _analyticsService.GetTreasuryAnalyticsAsync();
                return Ok(treasuryAnalytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving treasury analytics");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving treasury analytics",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get tier performance analytics
        /// </summary>
        [HttpGet("tiers")]
        [ResponseCache(Duration = 300)] // 5 minutes cache
        public async Task<ActionResult<List<TierPerformanceModel>>> GetTierPerformance()
        {
            try
            {
                var tierPerformance = await _analyticsService.GetTierPerformanceAsync();
                return Ok(tierPerformance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tier performance");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving tier performance",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get price history for charts
        /// </summary>
        [HttpGet("price-history")]
        [ResponseCache(Duration = 600)] // 10 minutes cache
        public async Task<ActionResult<List<TimeSeriesDataPoint>>> GetPriceHistory(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string interval = "1d")
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                if (start >= end)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Start date must be before end date"
                    });
                }

                var maxRange = TimeSpan.FromDays(365); // Max 1 year
                if (end - start > maxRange)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = $"Date range cannot exceed {maxRange.TotalDays} days"
                    });
                }

                var priceHistory = await _analyticsService.GetPriceHistoryAsync(start, end, interval);
                return Ok(priceHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving price history");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving price history",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get volume history for charts
        /// </summary>
        [HttpGet("volume-history")]
        [ResponseCache(Duration = 600)] // 10 minutes cache
        public async Task<ActionResult<List<TimeSeriesDataPoint>>> GetVolumeHistory(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string interval = "1d")
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                if (start >= end)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Start date must be before end date"
                    });
                }

                var volumeHistory = await _analyticsService.GetVolumeHistoryAsync(start, end, interval);
                return Ok(volumeHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving volume history");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving volume history",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get analytics metrics by category
        /// </summary>
        [HttpGet("metrics/{category}")]
        [ResponseCache(Duration = 300)] // 5 minutes cache
        public async Task<ActionResult<List<AnalyticsMetricModel>>> GetMetricsByCategory([FromRoute] string category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category))
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Category parameter is required"
                    });
                }

                var validCategories = new[] { "Token", "Presale", "Platform", "Treasury" };
                if (!validCategories.Contains(category, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = $"Invalid category. Valid categories are: {string.Join(", ", validCategories)}"
                    });
                }

                var metrics = await _analyticsService.GetMetricsByCategoryAsync(category);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving metrics for category {Category}", category);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving metrics",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get real-time analytics snapshot
        /// </summary>
        [HttpGet("snapshot")]
        [ResponseCache(Duration = 60)] // 1 minute cache
        public async Task<ActionResult<Core.Data.Entities.AnalyticsSnapshot>> GetRealTimeSnapshot()
        {
            try
            {
                var snapshot = await _analyticsService.GetRealTimeSnapshotAsync();
                return Ok(snapshot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving real-time snapshot");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving real-time snapshot",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get daily analytics for specified date range
        /// </summary>
        [HttpGet("daily")]
        [ResponseCache(Duration = 1800)] // 30 minutes cache
        public async Task<ActionResult<List<Core.Data.Entities.DailyAnalytics>>> GetDailyAnalytics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                if (start >= end)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Start date must be before end date"
                    });
                }

                var maxRange = TimeSpan.FromDays(365); // Max 1 year
                if (end - start > maxRange)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = $"Date range cannot exceed {maxRange.TotalDays} days"
                    });
                }

                var dailyAnalytics = await _analyticsService.GetDailyAnalyticsAsync(start, end);
                return Ok(dailyAnalytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving daily analytics");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving daily analytics",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get analytics comparison between two periods
        /// </summary>
        [HttpGet("comparison")]
        [ResponseCache(Duration = 900)] // 15 minutes cache
        public async Task<ActionResult<AnalyticsComparisonModel>> GetAnalyticsComparison(
            [FromQuery] DateTime period1Start,
            [FromQuery] DateTime period1End,
            [FromQuery] DateTime period2Start,
            [FromQuery] DateTime period2End)
        {
            try
            {
                // Validate date ranges
                if (period1Start >= period1End || period2Start >= period2End)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Start dates must be before end dates for both periods"
                    });
                }

                var maxRange = TimeSpan.FromDays(90); // Max 90 days per period
                if ((period1End - period1Start) > maxRange || (period2End - period2Start) > maxRange)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = $"Each period cannot exceed {maxRange.TotalDays} days"
                    });
                }

                var comparison = await _analyticsService.GetAnalyticsComparisonAsync(
                    period1Start, period1End, period2Start, period2End);

                return Ok(comparison);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving analytics comparison");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving analytics comparison",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Trigger analytics snapshot (Admin only)
        /// </summary>
        [HttpPost("snapshot")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> TakeAnalyticsSnapshot()
        {
            try
            {
                var success = await _analyticsService.TakeAnalyticsSnapshotAsync();

                if (success)
                {
                    return Ok(new { message = "Analytics snapshot taken successfully" });
                }
                else
                {
                    return StatusCode(500, new ErrorResponse
                    {
                        Message = "Failed to take analytics snapshot"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error taking analytics snapshot");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error taking analytics snapshot",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get analytics summary for quick overview
        /// </summary>
        [HttpGet("summary")]
        [ResponseCache(Duration = 180)] // 3 minutes cache
        public async Task<ActionResult<object>> GetAnalyticsSummary()
        {
            try
            {
                // Get key metrics for quick overview
                var tokenAnalytics = await _analyticsService.GetTokenAnalyticsAsync();
                var presaleAnalytics = await _analyticsService.GetPresaleAnalyticsAsync();

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

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving analytics summary");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving analytics summary",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        [ResponseCache(Duration = 30)]
        public ActionResult GetHealthStatus()
        {
            try
            {
                return Ok(new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    version = "1.0.0",
                    service = "Analytics API"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(500, new
                {
                    status = "unhealthy",
                    timestamp = DateTime.UtcNow,
                    error = "Service health check failed"
                });
            }
        }
    }
}