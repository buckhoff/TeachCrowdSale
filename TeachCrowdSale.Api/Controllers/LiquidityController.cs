// TeachCrowdSale.Api/Controllers/LiquidityController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Attributes;
using TeachCrowdSale.Core.Interfaces.Services;
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
        public async Task<ActionResult<LiquidityPageDataResponse>> GetLiquidityPageData()
        {
            try
            {
                var data = await _liquidityService.GetLiquidityPageDataAsync();
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
        public async Task<ActionResult<List<LiquidityPoolResponse>>> GetLiquidityPools([FromQuery] string? dexName = null)
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
        /// Get specific liquidity pool
        /// </summary>
        [HttpGet("pools/{poolId}")]
        [ResponseCache(Duration = 180)]
        public async Task<ActionResult<LiquidityPoolResponse>> GetLiquidityPool([FromRoute] int poolId)
        {
            try
            {
                var pool = await _liquidityService.GetLiquidityPoolAsync(poolId);
                if (pool == null)
                {
                    return NotFound(new ErrorResponse { Message = $"Liquidity pool {poolId} not found" });
                }

                // Convert entity to response model
                var response = new LiquidityPoolResponse
                {
                    Id = pool.Id,
                    Name = pool.Name,
                    TokenPair = pool.TokenPair,
                    Token0Symbol = pool.Token0Symbol,
                    Token1Symbol = pool.Token1Symbol,
                    DexName = pool.DexName,
                    PoolAddress = pool.PoolAddress,
                    CurrentAPY = pool.APY,
                    TotalValueLocked = pool.TotalValueLocked,
                    Volume24h = pool.Volume24h,
                    FeePercentage = pool.FeePercentage,
                    IsActive = pool.IsActive,
                    IsRecommended = pool.IsRecommended,
                    CreatedAt = pool.CreatedAt,
                    UpdatedAt = pool.UpdatedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving liquidity pool {PoolId}", poolId);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving liquidity pool",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get liquidity statistics
        /// </summary>
        [HttpGet("stats")]
        [ResponseCache(Duration = 120)]
        public async Task<ActionResult<LiquidityStatsResponse>> GetLiquidityStats()
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
        /// Get DEX configuration options
        /// </summary>
        [HttpGet("dex-options")]
        [ResponseCache(Duration = 600)]
        public async Task<ActionResult<List<DexConfigurationResponse>>> GetDexOptions()
        {
            try
            {
                var dexOptions = await _liquidityService.GetDexConfigurationsAsync();
                return Ok(dexOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving DEX options");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving DEX options",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get liquidity analytics
        /// </summary>
        [HttpGet("analytics")]
        [ResponseCache(Duration = 300)]
        public async Task<ActionResult<LiquidityAnalyticsResponse>> GetLiquidityAnalytics()
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
        /// Get user's liquidity positions
        /// </summary>
        [HttpGet("user/{address}/positions")]
        [Authorize]
        public async Task<ActionResult<List<UserLiquidityPositionResponse>>> GetUserLiquidityPositions([FromRoute] string address)
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
        /// Get user's comprehensive liquidity information
        /// </summary>
        [HttpGet("user/{address}")]
        [Authorize]
        public async Task<ActionResult<UserLiquidityInfoResponse>> GetUserLiquidityInfo([FromRoute] string address)
        {
            try
            {
                if (!_blockchainService.IsValidAddress(address))
                {
                    return BadRequest(new ErrorResponse { Message = "Invalid Ethereum address" });
                }

                var userInfo = await _liquidityService.GetUserLiquidityInfoAsync(address);
                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user liquidity info for {Address}", address);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving user liquidity information",
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
        public async Task<ActionResult<LiquidityCalculationResponse>> CalculateLiquidity([FromBody] LiquidityCalculationRequest request)
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
        [HttpPost("calculate/remove")]
        public async Task<ActionResult<LiquidityCalculationResponse>> CalculateRemoveLiquidity([FromBody] RemoveLiquidityCalculationRequest request)
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
        /// Validate liquidity parameters
        /// </summary>
        [HttpPost("validate")]
        public async Task<ActionResult<LiquidityValidationResponse>> ValidateLiquidityParameters([FromBody] LiquidityValidationRequest request)
        {
            try
            {
                if (!_blockchainService.IsValidAddress(request.WalletAddress))
                {
                    return BadRequest(new ErrorResponse { Message = "Invalid Ethereum address" });
                }

                var validation = await _liquidityService.ValidateLiquidityParametersAsync(
                    request.WalletAddress,
                    request.PoolId,
                    request.Token0Amount,
                    request.Token1Amount);

                return Ok(validation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating liquidity parameters");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error validating liquidity parameters",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get liquidity guide steps
        /// </summary>
        [HttpGet("guide")]
        [ResponseCache(Duration = 1800)]
        public async Task<ActionResult<List<LiquidityGuideStepResponse>>> GetLiquidityGuide([FromQuery] string? walletAddress = null)
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
        /// Get pool performance data
        /// </summary>
        [HttpGet("analytics/pools")]
        [ResponseCache(Duration = 600)]
        public async Task<ActionResult<List<PoolPerformanceDataResponse>>> GetPoolPerformance()
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
        [HttpGet("analytics/tvl-trends")]
        [ResponseCache(Duration = 300)]
        public async Task<ActionResult<List<LiquidityTrendDataResponse>>> GetTvlTrends([FromQuery] int days = 30)
        {
            try
            {
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
        [HttpGet("analytics/volume-trends")]
        [ResponseCache(Duration = 300)]
        public async Task<ActionResult<List<VolumeTrendDataResponse>>> GetVolumeTrends([FromQuery] int days = 30)
        {
            try
            {
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
        /// Get top liquidity providers
        /// </summary>
        [HttpGet("analytics/top-providers")]
        [ResponseCache(Duration = 600)]
        public async Task<ActionResult<List<UserLiquidityStatsResponse>>> GetTopLiquidityProviders([FromQuery] int limit = 10)
        {
            try
            {
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
                    return Ok(new { message = $"Pool {poolId} data synced successfully" });
                }
                else
                {
                    await _liquidityService.SyncAllPoolsDataAsync();
                    return Ok(new { message = "All pools data synced successfully" });
                }
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
                if (success)
                {
                    return Ok(new { message = "Pool prices refreshed successfully" });
                }
                else
                {
                    return StatusCode(500, new ErrorResponse
                    {
                        Message = "Failed to refresh some pool prices",
                        TraceId = HttpContext.TraceIdentifier
                    });
                }
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