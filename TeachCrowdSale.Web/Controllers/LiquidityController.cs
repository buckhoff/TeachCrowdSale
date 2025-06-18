using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models.Liquidity;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Web.Controllers
{
    /// <summary>
    /// Liquidity Dashboard Controller
    /// Handles main liquidity dashboard, add liquidity wizard, and position management
    /// Follows established TeachToken patterns with service injection and error handling
    /// </summary>
    [Route("liquidity")]
    public class LiquidityController : Controller
    {
        private readonly ILiquidityDashboardService _liquidityService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<LiquidityController> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public LiquidityController(
            ILiquidityDashboardService liquidityService,
            IMemoryCache cache,
            ILogger<LiquidityController> logger)
        {
            _liquidityService = liquidityService ?? throw new ArgumentNullException(nameof(liquidityService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        #region Main Dashboard

        /// <summary>
        /// Main liquidity dashboard page
        /// Route: /liquidity
        /// </summary>
        [HttpGet("")]
        [HttpGet("index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Get comprehensive dashboard data
                var dashboardData = await _liquidityService.GetLiquidityPageDataAsync();

                if (dashboardData == null)
                {
                    _logger.LogWarning("Failed to load liquidity dashboard data");
                    ViewBag.ErrorMessage = "Unable to load liquidity data at this time. Please try again later.";
                }

                // Pass data to view via ViewBag for JavaScript consumption
                ViewBag.InitialData = dashboardData;
                ViewBag.JsonData = dashboardData != null
                    ? JsonSerializer.Serialize(dashboardData, _jsonOptions)
                    : "{}";

                // Set page-specific metadata
                ViewData["Title"] = "Liquidity Dashboard - TEACH Token Pool Management";
                ViewData["Description"] = "Manage your TEACH token liquidity positions across multiple DEXes. View pool performance, calculate returns, and optimize your liquidity provision strategy.";

                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading liquidity dashboard");
                ViewBag.ErrorMessage = "An error occurred while loading liquidity data.";

                // Return view with null model to show fallback UI
                return View();
            }
        }

        /// <summary>
        /// AJAX endpoint for getting dashboard data
        /// </summary>
        [HttpGet("data")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                var data = await _liquidityService.GetLiquidityPageDataAsync();
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving liquidity dashboard data");
                return StatusCode(500, new { error = "Unable to load dashboard data" });
            }
        }

        /// <summary>
        /// AJAX endpoint for getting liquidity statistics
        /// </summary>
        [HttpGet("stats")]
        [ResponseCache(Duration = 120)]
        public async Task<IActionResult> GetLiquidityStats()
        {
            try
            {
                var stats = await _liquidityService.GetLiquidityStatsAsync();
                return Json(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving liquidity statistics");
                return StatusCode(500, new { error = "Unable to load statistics" });
            }
        }

        #endregion

        #region Add Liquidity Wizard

        /// <summary>
        /// Add liquidity wizard page
        /// Route: /liquidity/add
        /// </summary>
        [HttpGet("add")]
        [HttpGet("add/{step?}")]
        public async Task<IActionResult> Add(int step = 1)
        {
            try
            {
                // Get wizard data
                var wizardData = await _liquidityService.GetAddLiquidityWizardDataAsync();

                if (wizardData == null)
                {
                    _logger.LogWarning("Failed to load add liquidity wizard data");
                    ViewBag.ErrorMessage = "Unable to load wizard data at this time. Please try again later.";
                }
                else
                {
                    // Set current step
                    wizardData.CurrentStep = Math.Max(1, Math.Min(step, wizardData.TotalSteps));
                }

                // Pass data to view via ViewBag for JavaScript consumption
                ViewBag.InitialData = wizardData;
                ViewBag.JsonData = wizardData != null
                    ? JsonSerializer.Serialize(wizardData, _jsonOptions)
                    : "{}";

                // Set page-specific metadata
                ViewData["Title"] = $"Add Liquidity - Step {step} - TEACH Token";
                ViewData["Description"] = "Step-by-step wizard to add liquidity to TEACH token pools. Get guided assistance, compare DEX options, and calculate expected returns.";

                return View(wizardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading add liquidity wizard");
                ViewBag.ErrorMessage = "An error occurred while loading the wizard.";

                return View();
            }
        }

        /// <summary>
        /// AJAX endpoint for wizard data
        /// </summary>
        [HttpGet("wizard-data")]
        public async Task<IActionResult> GetWizardData([FromQuery] string? walletAddress = null)
        {
            try
            {
                var data = await _liquidityService.GetAddLiquidityWizardDataAsync(walletAddress);
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving wizard data for wallet {WalletAddress}", walletAddress);
                return StatusCode(500, new { error = "Unable to load wizard data" });
            }
        }

        /// <summary>
        /// AJAX endpoint for pool comparison
        /// </summary>
        [HttpPost("compare-pools")]
        public async Task<IActionResult> ComparePools([FromBody] int[] poolIds)
        {
            try
            {
                if (poolIds == null || poolIds.Length == 0)
                {
                    return BadRequest(new { error = "Pool IDs are required" });
                }

                var comparison = await _liquidityService.GetPoolComparisonDataAsync(poolIds.ToList());
                return Json(comparison);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing pools {PoolIds}", string.Join(",", poolIds ?? Array.Empty<int>()));
                return StatusCode(500, new { error = "Unable to compare pools" });
            }
        }

        /// <summary>
        /// AJAX endpoint for liquidity calculation
        /// </summary>
        [HttpPost("calculate")]
        public async Task<IActionResult> CalculateLiquidity([FromBody] LiquidityCalculationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        error = "Invalid request data",
                        errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var calculation = await _liquidityService.CalculateAddLiquidityPreviewAsync(
                    request.WalletAddress,
                    request.PoolId,
                    request.Token0Amount,
                    request.Token1Amount,
                    request.SlippageTolerance);

                return Json(calculation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating liquidity for pool {PoolId}", request?.PoolId);
                return StatusCode(500, new { error = "Unable to calculate liquidity" });
            }
        }

        /// <summary>
        /// AJAX endpoint for impermanent loss calculation
        /// </summary>
        [HttpPost("calculate-il")]
        public async Task<IActionResult> CalculateImpermanentLoss([FromBody] ImpermanentLossRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        error = "Invalid request data",
                        errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var impermanentLoss = await _liquidityService.CalculateImpermanentLossAsync(
                    request.PoolId,
                    request.Token0Amount,
                    request.Token1Amount,
                    request.PriceChangePercentage);

                return Json(new { impermanentLoss });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating impermanent loss for pool {PoolId}", request?.PoolId);
                return StatusCode(500, new { error = "Unable to calculate impermanent loss" });
            }
        }

        /// <summary>
        /// AJAX endpoint for return projections
        /// </summary>
        [HttpPost("projections")]
        public async Task<IActionResult> GetReturnProjections([FromBody] ReturnProjectionRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        error = "Invalid request data",
                        errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var projections = await _liquidityService.GetReturnProjectionsAsync(
                    request.PoolId,
                    request.Token0Amount,
                    request.Token1Amount,
                    request.ProjectionDays);

                return Json(projections);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting return projections for pool {PoolId}", request?.PoolId);
                return StatusCode(500, new { error = "Unable to get projections" });
            }
        }

        /// <summary>
        /// AJAX endpoint for validation
        /// </summary>
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateAddLiquidity([FromBody] LiquidityValidationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        error = "Invalid request data",
                        errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var isValid = await _liquidityService.ValidateAddLiquidityAsync(
                    request.WalletAddress,
                    request.PoolId,
                    request.Token0Amount,
                    request.Token1Amount);

                return Json(new { isValid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating liquidity addition for pool {PoolId}", request?.PoolId);
                return StatusCode(500, new { error = "Unable to validate" });
            }
        }

        /// <summary>
        /// Get external DEX URL for adding liquidity
        /// </summary>
        [HttpGet("dex-url")]
        public async Task<IActionResult> GetDexUrl([FromQuery] int poolId, [FromQuery] int dexId,
            [FromQuery] decimal? token0Amount = null, [FromQuery] decimal? token1Amount = null)
        {
            try
            {
                var url = await _liquidityService.GetDexLiquidityUrlAsync(poolId, dexId, token0Amount, token1Amount);

                if (string.IsNullOrEmpty(url))
                {
                    return NotFound(new { error = "DEX URL not found" });
                }

                return Json(new { url });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting DEX URL for pool {PoolId}, DEX {DexId}", poolId, dexId);
                return StatusCode(500, new { error = "Unable to get DEX URL" });
            }
        }

        #endregion

        #region Manage Positions

        /// <summary>
        /// Manage liquidity positions page
        /// Route: /liquidity/manage
        /// </summary>
        [HttpGet("manage")]
        public async Task<IActionResult> Manage([FromQuery] string? walletAddress = null)
        {
            try
            {
                UserLiquidityInfoModel? userInfo = null;

                if (!string.IsNullOrWhiteSpace(walletAddress))
                {
                    userInfo = await _liquidityService.GetUserLiquidityInfoAsync(walletAddress);

                    if (userInfo == null)
                    {
                        _logger.LogWarning("No liquidity information found for wallet {WalletAddress}", walletAddress);
                    }
                }

                // Pass data to view via ViewBag for JavaScript consumption
                ViewBag.InitialData = userInfo;
                ViewBag.JsonData = userInfo != null
                    ? JsonSerializer.Serialize(userInfo, _jsonOptions)
                    : "{}";
                ViewBag.WalletAddress = walletAddress;

                // Set page-specific metadata
                ViewData["Title"] = "Manage Liquidity Positions - TEACH Token";
                ViewData["Description"] = "View and manage your TEACH token liquidity positions. Track performance, claim rewards, and remove liquidity across multiple DEXes.";

                return View(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading manage positions page for wallet {WalletAddress}", walletAddress);
                ViewBag.ErrorMessage = "An error occurred while loading your positions.";

                return View();
            }
        }

        /// <summary>
        /// AJAX endpoint for user positions
        /// </summary>
        [HttpGet("user/{walletAddress}/positions")]
        [ResponseCache(Duration = 30)]
        public async Task<IActionResult> GetUserPositions([FromRoute] string walletAddress)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(walletAddress))
                {
                    return BadRequest(new { error = "Wallet address is required" });
                }

                var positions = await _liquidityService.GetUserLiquidityPositionsAsync(walletAddress);
                return Json(positions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving positions for wallet {WalletAddress}", walletAddress);
                return StatusCode(500, new { error = "Unable to load positions" });
            }
        }

        /// <summary>
        /// AJAX endpoint for user transaction history
        /// </summary>
        [HttpGet("user/{walletAddress}/transactions")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetUserTransactions([FromRoute] string walletAddress,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(walletAddress))
                {
                    return BadRequest(new { error = "Wallet address is required" });
                }

                var transactions = await _liquidityService.GetUserTransactionHistoryAsync(walletAddress, page, pageSize);
                return Json(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions for wallet {WalletAddress}", walletAddress);
                return StatusCode(500, new { error = "Unable to load transactions" });
            }
        }

        /// <summary>
        /// AJAX endpoint for user performance data
        /// </summary>
        [HttpGet("user/{walletAddress}/performance")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetUserPerformance([FromRoute] string walletAddress)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(walletAddress))
                {
                    return BadRequest(new { error = "Wallet address is required" });
                }

                var performance = await _liquidityService.GetUserPerformanceAsync(walletAddress);
                return Json(performance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving performance for wallet {WalletAddress}", walletAddress);
                return StatusCode(500, new { error = "Unable to load performance data" });
            }
        }

        /// <summary>
        /// AJAX endpoint for remove liquidity calculation
        /// </summary>
        [HttpPost("calculate-remove")]
        public async Task<IActionResult> CalculateRemoveLiquidity([FromBody] RemoveLiquidityRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        error = "Invalid request data",
                        errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var calculation = await _liquidityService.CalculateRemoveLiquidityPreviewAsync(
                    request.WalletAddress,
                    request.PositionId,
                    request.PercentageToRemove);

                return Json(calculation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating remove liquidity for position {PositionId}", request?.PositionId);
                return StatusCode(500, new { error = "Unable to calculate removal" });
            }
        }

        #endregion

        #region Educational Content

        /// <summary>
        /// AJAX endpoint for educational content
        /// </summary>
        [HttpGet("education")]
        [ResponseCache(Duration = 3600)] // Cache for 1 hour
        public async Task<IActionResult> GetEducationalContent()
        {
            try
            {
                var content = await _liquidityService.GetEducationalContentAsync();
                return Json(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving educational content");
                return StatusCode(500, new { error = "Unable to load educational content" });
            }
        }

        /// <summary>
        /// AJAX endpoint for risk warnings
        /// </summary>
        [HttpGet("risks")]
        [ResponseCache(Duration = 3600)] // Cache for 1 hour
        public async Task<IActionResult> GetRiskWarnings()
        {
            try
            {
                var warnings = await _liquidityService.GetRiskWarningsAsync();
                return Json(warnings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving risk warnings");
                return StatusCode(500, new { error = "Unable to load risk warnings" });
            }
        }

        /// <summary>
        /// AJAX endpoint for guide steps
        /// </summary>
        [HttpGet("guide")]
        [ResponseCache(Duration = 1800)] // Cache for 30 minutes
        public async Task<IActionResult> GetGuideSteps([FromQuery] string? dexName = null)
        {
            try
            {
                var steps = await _liquidityService.GetLiquidityGuideStepsAsync(dexName);
                return Json(steps);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving guide steps for DEX {DexName}", dexName);
                return StatusCode(500, new { error = "Unable to load guide steps" });
            }
        }

        #endregion

        #region Utility Endpoints

        /// <summary>
        /// AJAX endpoint for pool details
        /// </summary>
        [HttpGet("pool/{poolId}")]
        [ResponseCache(Duration = 120)]
        public async Task<IActionResult> GetPoolDetails([FromRoute] int poolId)
        {
            try
            {
                var pool = await _liquidityService.GetLiquidityPoolDetailsAsync(poolId);

                if (pool == null)
                {
                    return NotFound(new { error = "Pool not found" });
                }

                return Json(pool);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pool details for pool {PoolId}", poolId);
                return StatusCode(500, new { error = "Unable to load pool details" });
            }
        }

        /// <summary>
        /// AJAX endpoint for user token balances
        /// </summary>
        [HttpGet("user/{walletAddress}/balances/{poolId}")]
        [ResponseCache(Duration = 30)]
        public async Task<IActionResult> GetUserBalances([FromRoute] string walletAddress, [FromRoute] int poolId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(walletAddress))
                {
                    return BadRequest(new { error = "Wallet address is required" });
                }

                var balances = await _liquidityService.GetUserTokenBalancesAsync(walletAddress, poolId);
                return Json(balances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving balances for wallet {WalletAddress}, pool {PoolId}", walletAddress, poolId);
                return StatusCode(500, new { error = "Unable to load balances" });
            }
        }

        /// <summary>
        /// AJAX endpoint for service health
        /// </summary>
        [HttpGet("health")]
        public async Task<IActionResult> GetHealthStatus()
        {
            try
            {
                var isHealthy = await _liquidityService.GetServiceHealthAsync();
                return Json(new { isHealthy, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking service health");
                return Json(new { isHealthy = false, timestamp = DateTime.UtcNow });
            }
        }

        /// <summary>
        /// Clear cache endpoint (for development/admin use)
        /// </summary>
        [HttpPost("clear-cache")]
        public IActionResult ClearCache([FromQuery] string? walletAddress = null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(walletAddress))
                {
                    _liquidityService.ClearUserCache(walletAddress);
                    _logger.LogInformation("Cleared cache for wallet {WalletAddress}", walletAddress);
                }
                else
                {
                    _liquidityService.ClearAllCache();
                    _logger.LogInformation("Cleared all liquidity cache");
                }

                return Json(new { success = true, message = "Cache cleared successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache");
                return StatusCode(500, new { error = "Unable to clear cache" });
            }
        }

        #endregion
    }

}