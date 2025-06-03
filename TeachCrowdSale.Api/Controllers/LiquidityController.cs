// TeachCrowdSale.Api/Controllers/LiquidityController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Attributes;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models.Liquidity;
using TeachCrowdSale.Core.Models.Request;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Api.Controllers
{
    [EnableRateLimiting("Liquidity")]
    [ApiController]
    [Route("api/liquidity")]
    public class LiquidityController : ControllerBase
    {
        private readonly ILiquidityService _liquidityService;
        private readonly IBlockchainService _blockchainService;
        private readonly ILogger<LiquidityController> _logger;

        public LiquidityController(
            ILiquidityService liquidityService,
            IBlockchainService blockchainService,
            ILogger<LiquidityController> logger)
        {
            _liquidityService = liquidityService ?? throw new ArgumentNullException(nameof(liquidityService));
            _blockchainService = blockchainService ?? throw new ArgumentNullException(nameof(blockchainService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get comprehensive liquidity page data
        /// </summary>
        [HttpGet("data")]
        [ResponseCache(Duration = 60)]
        public async Task<ActionResult<LiquidityPageDataModel>> GetLiquidityPageData()
        {
            try
            {
                var data = new LiquidityPageDataModel
                {
                    LiquidityPools = await _liquidityService.GetActiveLiquidityPoolsAsync(),
                    DexOptions = await _liquidityService.GetDexConfigurationsAsync(),
                    Stats = await _liquidityService.GetLiquidityStatsAsync(),
                    Analytics = await _liquidityService.GetLiquidityAnalyticsAsync(),
                    GuideSteps = await _liquidityService.GetLiquidityGuideStepsAsync(),
                    LoadedAt = DateTime.UtcNow
                };

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving liquidity page data");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving liquidity data",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get active liquidity pools
        /// </summary>
        [HttpGet("pools")]
        [ResponseCache(Duration = 300)]
        public async Task<ActionResult<List<LiquidityPoolDisplayModel>>> GetLiquidityPools([FromQuery] string? dexName = null)
        {
            try
            {
                var pools = await _liquidityService.GetActiveLiquidityPoolsAsync();

                if (!string.IsNullOrEmpty(dexName))
                {
                    pools = pools.Where(p => p.DexName.Equals(dexName, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                return Ok(pools);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving liquidity pools");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving liquidity pools",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get liquidity statistics
        /// </summary>
        [HttpGet("stats")]
        [ResponseCache(Duration = 120)]
        public async Task<ActionResult<LiquidityStatsOverviewModel>> GetLiquidityStats()
        {
            try
            {
                var stats = await _liquidityService.GetLiquidityStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving liquidity statistics");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving liquidity statistics",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get user's liquidity positions
        /// </summary>
        [HttpGet("user/{address}/positions")]
        [Authorize]
        public async Task<ActionResult<List<UserLiquidityPositionModel>>> GetUserLiquidityPositions([FromRoute] string address)
        {
            try
            {
                if (!_blockchainService.IsValidAddress(address))
                {
                    return BadRequest(new ErrorResponse { Message = "Invalid Ethereum address" });
                }

                var positions = await _liquidityService.GetUserLiquidityPositionsAsync(address);
                return Ok(positions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user liquidity positions for {Address}", address);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving user liquidity positions",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get user's total liquidity value
        /// </summary>
        [HttpGet("user/{address}/value")]
        [Authorize]
        public async Task<ActionResult<decimal>> GetUserTotalLiquidityValue([FromRoute] string address)
        {
            try
            {
                if (!_blockchainService.IsValidAddress(address))
                {
                    return BadRequest(new ErrorResponse { Message = "Invalid Ethereum address" });
                }

                var totalValue = await _liquidityService.GetUserTotalLiquidityValueAsync(address);
                return Ok(new { totalValue, formatted = totalValue.ToString("C2") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user total liquidity value for {Address}", address);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving user liquidity value",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Calculate liquidity preview
        /// </summary>
        [HttpPost("calculate")]
        public async Task<ActionResult<LiquidityCalculationModel>> CalculateLiquidity([FromBody] LiquidityCalculationRequest request)
        {
            try
            {
                if (!_blockchainService.IsValidAddress(request.WalletAddress))
                {
                    return BadRequest(new ErrorResponse { Message = "Invalid Ethereum address" });
                }

                var calculation = await _liquidityService.CalculateLiquidityPreviewAsync(
                    request.WalletAddress,
                    request.PoolId,
                    request.Token0Amount,
                    request.Token1Amount,
                    request.SlippageTolerance);

                return Ok(calculation);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating liquidity preview");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error calculating liquidity preview",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Calculate remove liquidity preview
        /// </summary>
        [HttpPost("calculate-remove")]
        [Authorize]
        public async Task<ActionResult<LiquidityCalculationModel>> CalculateRemoveLiquidity([FromBody] RemoveLiquidityCalculationRequest request)
        {
            try
            {
                if (!_blockchainService.IsValidAddress(request.WalletAddress))
                {
                    return BadRequest(new ErrorResponse { Message = "Invalid Ethereum address" });
                }

                var calculation = await _liquidityService.CalculateRemoveLiquidityPreviewAsync(
                    request.WalletAddress,
                    request.PositionId,
                    request.PercentageToRemove);

                return Ok(calculation);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating remove liquidity preview");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error calculating remove liquidity preview",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Add liquidity to a pool
        /// </summary>
        [EnableRateLimiting("LiquidityTransaction")]
        [HttpPost("add")]
        [Authorize]
        public async Task<ActionResult> AddLiquidity([FromBody] AddLiquidityRequest request)
        {
            try
            {
                if (!_blockchainService.IsValidAddress(request.WalletAddress))
                {
                    return BadRequest(new ErrorResponse { Message = "Invalid Ethereum address" });
                }

                var success = await _liquidityService.AddLiquidityAsync(
                    request.WalletAddress,
                    request.PoolId,
                    request.Token0Amount,
                    request.Token1Amount,
                    request.Token0AmountMin,
                    request.Token1AmountMin);

                if (!success)
                {
                    return StatusCode(500, new ErrorResponse { Message = "Failed to add liquidity" });
                }

                return Ok(new { message = "Liquidity added successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding liquidity for {WalletAddress}", request.WalletAddress);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while adding liquidity",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Remove liquidity from a position
        /// </summary>
        [EnableRateLimiting("LiquidityTransaction")]
        [HttpPost("remove")]
        [Authorize]
        public async Task<ActionResult> RemoveLiquidity([FromBody] RemoveLiquidityRequest request)
        {
            try
            {
                if (!_blockchainService.IsValidAddress(request.WalletAddress))
                {
                    return BadRequest(new ErrorResponse { Message = "Invalid Ethereum address" });
                }

                var success = await _liquidityService.RemoveLiquidityAsync(
                    request.WalletAddress,
                    request.PositionId,
                    request.PercentageToRemove);

                if (!success)
                {
                    return StatusCode(500, new ErrorResponse { Message = "Failed to remove liquidity" });
                }

                return Ok(new { message = "Liquidity removed successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing liquidity for {WalletAddress}", request.WalletAddress);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while removing liquidity",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Claim fees from a liquidity position
        /// </summary>
        [EnableRateLimiting("LiquidityTransaction")]
        [HttpPost("claim-fees")]
        [Authorize]
        public async Task<ActionResult> ClaimFees([FromBody] ClaimFeesRequest request)
        {
            try
            {
                if (!_blockchainService.IsValidAddress(request.WalletAddress))
                {
                    return BadRequest(new ErrorResponse { Message = "Invalid Ethereum address" });
                }

                var success = await _liquidityService.ClaimFeesAsync(request.WalletAddress, request.PositionId);

                if (!success)
                {
                    return StatusCode(500, new ErrorResponse { Message = "Failed to claim fees" });
                }

                return Ok(new { message = "Fees claimed successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error claiming fees for {WalletAddress}", request.WalletAddress);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while claiming fees",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get DEX configurations
        /// </summary>
        [HttpGet("dexes")]
        [ResponseCache(Duration = 3600)]
        public async Task<ActionResult<List<DexConfigurationModel>>> GetDexConfigurations()
        {
            try
            {
                var dexes = await _liquidityService.GetDexConfigurationsAsync();
                return Ok(dexes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving DEX configurations");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving DEX configurations",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get liquidity analytics
        /// </summary>
        [HttpGet("analytics")]
        [ResponseCache(Duration = 300)]
        public async Task<ActionResult<LiquidityAnalyticsModel>> GetLiquidityAnalytics()
        {
            try
            {
                var analytics = await _liquidityService.GetLiquidityAnalyticsAsync();
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving liquidity analytics");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving liquidity analytics",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get top liquidity providers leaderboard
        /// </summary>
        [HttpGet("leaderboard")]
        [ResponseCache(Duration = 600)]
        public async Task<ActionResult<List<UserLiquidityStatsModel>>> GetTopLiquidityProviders([FromQuery] int limit = 10)
        {
            try
            {
                if (limit <= 0 || limit > 100)
                {
                    return BadRequest(new ErrorResponse { Message = "Limit must be between 1 and 100" });
                }

                var topProviders = await _liquidityService.GetTopLiquidityProvidersAsync(limit);
                return Ok(topProviders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top liquidity providers");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving top liquidity providers",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get pool performance data
        /// </summary>
        [HttpGet("pool-performance")]
        [ResponseCache(Duration = 300)]
        public async Task<ActionResult<List<PoolPerformanceDataModel>>> GetPoolPerformance()
        {
            try
            {
                var performance = await _liquidityService.GetPoolPerformanceAsync();
                return Ok(performance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pool performance data");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving pool performance data",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get TVL trends
        /// </summary>
        [HttpGet("trends/tvl")]
        [ResponseCache(Duration = 600)]
        public async Task<ActionResult<List<LiquidityTrendDataModel>>> GetTvlTrends([FromQuery] int days = 30)
        {
            try
            {
                if (days <= 0 || days > 365)
                {
                    return BadRequest(new ErrorResponse { Message = "Days must be between 1 and 365" });
                }

                var trends = await _liquidityService.GetTvlTrendsAsync(days);
                return Ok(trends);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving TVL trends");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving TVL trends",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get volume trends
        /// </summary>
        [HttpGet("trends/volume")]
        [ResponseCache(Duration = 600)]
        public async Task<ActionResult<List<VolumeTrendDataModel>>> GetVolumeTrends([FromQuery] int days = 30)
        {
            try
            {
                if (days <= 0 || days > 365)
                {
                    return BadRequest(new ErrorResponse { Message = "Days must be between 1 and 365" });
                }

                var trends = await _liquidityService.GetVolumeTrendsAsync(days);
                return Ok(trends);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving volume trends");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving volume trends",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get liquidity guide steps
        /// </summary>
        [HttpGet("guide")]
        public async Task<ActionResult<List<LiquidityGuideStepModel>>> GetLiquidityGuide([FromQuery] string? walletAddress = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(walletAddress) && !_blockchainService.IsValidAddress(walletAddress))
                {
                    return BadRequest(new ErrorResponse { Message = "Invalid Ethereum address" });
                }

                var steps = await _liquidityService.GetLiquidityGuideStepsAsync(walletAddress);
                return Ok(steps);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving liquidity guide");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving liquidity guide",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Sync pool data from DEX APIs
        /// </summary>
        [HttpPost("sync/pools")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> SyncPoolData([FromQuery] int? poolId = null)
        {
            try
            {
                if (poolId.HasValue)
                {
                    await _liquidityService.SyncPoolDataAsync(poolId.Value);
                }
                else
                {
                    await _liquidityService.SyncAllPoolsDataAsync();
                }

                return Ok(new { message = "Pool data synced successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing pool data");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error syncing pool data",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Refresh pool prices
        /// </summary>
        [HttpPost("sync/prices")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> RefreshPoolPrices()
        {
            try
            {
                var success = await _liquidityService.RefreshPoolPricesAsync();

                if (!success)
                {
                    return StatusCode(500, new ErrorResponse { Message = "Failed to refresh pool prices" });
                }

                return Ok(new { message = "Pool prices refreshed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing pool prices");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error refreshing pool prices",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }
    }
}

// Additional Request Models
namespace TeachCrowdSale.Core.Models.Request
{
    public class RemoveLiquidityCalculationRequest
    {
        [Required]
        [EthereumAddress]
        public string WalletAddress { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int PositionId { get; set; }

        [Required]
        [Range(0.1, 100)]
        public decimal PercentageToRemove { get; set; }
    }

    public class RemoveLiquidityRequest
    {
        [Required]
        [EthereumAddress]
        public string WalletAddress { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int PositionId { get; set; }

        [Required]
        [Range(0.1, 100)]
        public decimal PercentageToRemove { get; set; }
    }

    public class ClaimFeesRequest
    {
        [Required]
        [EthereumAddress]
        public string WalletAddress { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int PositionId { get; set; }
    }
}