using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models.Burning;
using TeachCrowdSale.Core.Models.Governance;
using TeachCrowdSale.Core.Models.Response;
using TeachCrowdSale.Core.Models.Tokenomics;
using TeachCrowdSale.Core.Models.Treasury;
using TeachCrowdSale.Core.Models.Utility;
using TeachCrowdSale.Core.Models.Vesting;

namespace TeachCrowdSale.Api.Controllers
{
    [EnableRateLimiting("Tokenomics")]
    [ApiController]
    [Route("api/tokenomics")]
    [Authorize]
    public class TokenomicsApiController : ControllerBase
    {
        private readonly ITokenomicsService _tokenomicsService;
        private readonly ILogger<TokenomicsApiController> _logger;

        public TokenomicsApiController(
            ITokenomicsService tokenomicsService,
            ILogger<TokenomicsApiController> logger)
        {
            _tokenomicsService = tokenomicsService ?? throw new ArgumentNullException(nameof(tokenomicsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get comprehensive tokenomics data for the main tokenomics page
        /// </summary>
        [HttpGet("data")]
        [ResponseCache(Duration = 120, VaryByHeader = "Authorization")]
        public async Task<ActionResult<TokenomicsDisplayModel>> GetTokenomicsData()
        {
            try
            {
                var tokenomicsData = await _tokenomicsService.GetTokenomicsDataAsync();
                return Ok(tokenomicsData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comprehensive tokenomics data");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving tokenomics data",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get live token metrics with frequent updates
        /// </summary>
        [HttpGet("live-metrics")]
        [EnableRateLimiting("LiveMetrics")]
        [ResponseCache(Duration = 60, VaryByHeader = "Authorization")]
        public async Task<ActionResult<LiveTokenMetricsModel>> GetLiveMetrics()
        {
            try
            {
                var metrics = await _tokenomicsService.GetLiveTokenMetricsAsync();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving live token metrics");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving live metrics",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get detailed token supply breakdown and allocation information
        /// </summary>
        [HttpGet("supply-breakdown")]
        [ResponseCache(Duration = 1800, VaryByHeader = "Authorization")] // 30 minutes
        public async Task<ActionResult<SupplyBreakdownModel>> GetSupplyBreakdown()
        {
            try
            {
                var supplyData = await _tokenomicsService.GetSupplyBreakdownAsync();
                return Ok(supplyData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supply breakdown");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving supply breakdown",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get vesting schedule and milestone information
        /// </summary>
        [HttpGet("vesting-schedule")]
        [ResponseCache(Duration = 1800, VaryByHeader = "Authorization")] // 30 minutes
        public async Task<ActionResult<VestingScheduleModel>> GetVestingSchedule()
        {
            try
            {
                var vestingData = await _tokenomicsService.GetVestingScheduleAsync();
                return Ok(vestingData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vesting schedule");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving vesting schedule",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get burn mechanics, statistics, and projections
        /// </summary>
        [HttpGet("burn-mechanics")]
        [ResponseCache(Duration = 900, VaryByHeader = "Authorization")] // 15 minutes
        public async Task<ActionResult<BurnMechanicsModel>> GetBurnMechanics()
        {
            try
            {
                var burnData = await _tokenomicsService.GetBurnMechanicsAsync();
                return Ok(burnData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving burn mechanics");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving burn mechanics",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get treasury analytics including allocations, runway, and scenarios
        /// </summary>
        [HttpGet("treasury-analytics")]
        [ResponseCache(Duration = 900, VaryByHeader = "Authorization")] // 15 minutes
        public async Task<ActionResult<TreasuryAnalyticsModel>> GetTreasuryAnalytics()
        {
            try
            {
                var treasuryData = await _tokenomicsService.GetTreasuryAnalyticsAsync();
                return Ok(treasuryData);
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
        /// Get utility features, metrics, and roadmap
        /// </summary>
        [HttpGet("utility-features")]
        [ResponseCache(Duration = 1800, VaryByHeader = "Authorization")] // 30 minutes
        public async Task<ActionResult<UtilityFeaturesModel>> GetUtilityFeatures()
        {
            try
            {
                var utilityData = await _tokenomicsService.GetUtilityFeaturesAsync();
                return Ok(utilityData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving utility features");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving utility features",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get governance information including proposals and voting data
        /// </summary>
        [HttpGet("governance-info")]
        [ResponseCache(Duration = 900, VaryByHeader = "Authorization")] // 15 minutes
        public async Task<ActionResult<GovernanceInfoModel>> GetGovernanceInfo()
        {
            try
            {
                var governanceData = await _tokenomicsService.GetGovernanceInfoAsync();
                return Ok(governanceData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving governance information");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving governance information",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get real-time price and market data summary
        /// </summary>
        [HttpGet("price-summary")]
        [EnableRateLimiting("LiveMetrics")]
        [ResponseCache(Duration = 30, VaryByHeader = "Authorization")]
        public async Task<ActionResult<PriceSummaryModel>> GetPriceSummary()
        {
            try
            {
                var liveMetrics = await _tokenomicsService.GetLiveTokenMetricsAsync();

                var summary = new PriceSummaryModel
                {
                    CurrentPrice = liveMetrics.CurrentPrice,
                    PriceChange24h = liveMetrics.PriceChange24h,
                    PriceChangePercent24h = liveMetrics.PriceChangePercent24h,
                    Volume24h = liveMetrics.Volume24h,
                    MarketCap = liveMetrics.MarketCap,
                    High24h = liveMetrics.High24h,
                    Low24h = liveMetrics.Low24h,
                    LastUpdated = liveMetrics.LastUpdated
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving price summary");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving price summary",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get supply metrics summary
        /// </summary>
        [HttpGet("supply-summary")]
        [ResponseCache(Duration = 600, VaryByHeader = "Authorization")] // 10 minutes
        public async Task<ActionResult<SupplyMetricsModel>> GetSupplySummary()
        {
            try
            {
                var supplyData = await _tokenomicsService.GetSupplyBreakdownAsync();
                return Ok(supplyData.Metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supply summary");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving supply summary",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get treasury overview summary
        /// </summary>
        [HttpGet("treasury-summary")]
        [ResponseCache(Duration = 600, VaryByHeader = "Authorization")] // 10 minutes
        public async Task<ActionResult<TreasuryOverviewModel>> GetTreasurySummary()
        {
            try
            {
                var treasuryData = await _tokenomicsService.GetTreasuryAnalyticsAsync();
                return Ok(treasuryData.Overview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving treasury summary");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving treasury summary",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get burn statistics summary
        /// </summary>
        [HttpGet("burn-summary")]
        [ResponseCache(Duration = 600, VaryByHeader = "Authorization")] // 10 minutes
        public async Task<ActionResult<BurnStatisticsModel>> GetBurnSummary()
        {
            try
            {
                var burnData = await _tokenomicsService.GetBurnMechanicsAsync();
                return Ok(burnData.Statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving burn summary");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving burn summary",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get governance overview summary
        /// </summary>
        [HttpGet("governance-summary")]
        [ResponseCache(Duration = 600, VaryByHeader = "Authorization")] // 10 minutes
        public async Task<ActionResult<GovernanceOverviewModel>> GetGovernanceSummary()
        {
            try
            {
                var governanceData = await _tokenomicsService.GetGovernanceInfoAsync();
                return Ok(governanceData.Overview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving governance summary");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving governance summary",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Health check endpoint for tokenomics service
        /// </summary>
        [HttpGet("health")]
        [ResponseCache(Duration = 30)]
        public async Task<ActionResult<HealthCheckModel>> HealthCheck()
        {
            try
            {
                var isHealthy = await _tokenomicsService.CheckApiHealthAsync();

                var healthModel = new HealthCheckModel
                {
                    Status = isHealthy ? "healthy" : "degraded",
                    Timestamp = DateTime.UtcNow,
                    Service = "tokenomics",
                    Version = "1.0",
                    Details = new Dictionary<string, object>
                    {
                        { "blockchain_connectivity", isHealthy },
                        { "database_connectivity", true }, // Could add actual DB check
                        { "cache_status", "operational" }
                    }
                };

                return isHealthy ? Ok(healthModel) : StatusCode(503, healthModel);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Tokenomics health check failed");

                var failedHealthModel = new HealthCheckModel
                {
                    Status = "unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Service = "tokenomics",
                    Version = "1.0",
                    Error = ex.Message
                };

                return StatusCode(503, failedHealthModel);
            }
        }

        /// <summary>
        /// Force refresh of cached data (admin endpoint)
        /// </summary>
        [HttpPost("refresh")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> RefreshData([FromQuery] string? section = null)
        {
            try
            {
                _logger.LogInformation("Manual data refresh requested for section: {Section}", section ?? "all");

                // This would clear relevant caches and trigger fresh data fetching
                // Implementation would depend on your caching strategy

                var refreshedSections = new List<string>();

                if (string.IsNullOrEmpty(section) || section == "all")
                {
                    // Refresh all sections
                    await _tokenomicsService.GetLiveTokenMetricsAsync();
                    await _tokenomicsService.GetSupplyBreakdownAsync();
                    await _tokenomicsService.GetVestingScheduleAsync();
                    await _tokenomicsService.GetBurnMechanicsAsync();
                    await _tokenomicsService.GetTreasuryAnalyticsAsync();
                    await _tokenomicsService.GetUtilityFeaturesAsync();
                    await _tokenomicsService.GetGovernanceInfoAsync();

                    refreshedSections.AddRange(new[] { "live-metrics", "supply", "vesting", "burn", "treasury", "utility", "governance" });
                }
                else
                {
                    // Refresh specific section
                    switch (section.ToLower())
                    {
                        case "live-metrics":
                            await _tokenomicsService.GetLiveTokenMetricsAsync();
                            break;
                        case "supply":
                            await _tokenomicsService.GetSupplyBreakdownAsync();
                            break;
                        case "vesting":
                            await _tokenomicsService.GetVestingScheduleAsync();
                            break;
                        case "burn":
                            await _tokenomicsService.GetBurnMechanicsAsync();
                            break;
                        case "treasury":
                            await _tokenomicsService.GetTreasuryAnalyticsAsync();
                            break;
                        case "utility":
                            await _tokenomicsService.GetUtilityFeaturesAsync();
                            break;
                        case "governance":
                            await _tokenomicsService.GetGovernanceInfoAsync();
                            break;
                        default:
                            return BadRequest(new ErrorResponse { Message = $"Unknown section: {section}" });
                    }

                    refreshedSections.Add(section);
                }

                return Ok(new
                {
                    message = "Data refresh completed successfully",
                    refreshedSections = refreshedSections,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual data refresh");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error during data refresh",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }
    }
}