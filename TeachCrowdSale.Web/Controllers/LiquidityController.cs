using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models.Liquidity;
using TeachCrowdSale.Core.Models.Request;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Web.Controllers
{
    /// <summary>
    /// Controller for liquidity management functionality
    /// Routes: /liquidity, /liquidity/add, /liquidity/manage
    /// Uses ILiquidityDashboardService for all data operations
    /// </summary>
    public class LiquidityController : Controller
    {
        private readonly ILiquidityDashboardService _liquidityService;
        private readonly ILogger<LiquidityController> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public LiquidityController(
            ILiquidityDashboardService liquidityService,
            ILogger<LiquidityController> logger)
        {
            _liquidityService = liquidityService;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };
        }

        #region Main Views

        /// <summary>
        /// Main liquidity dashboard - shows pools, stats, and analytics
        /// Route: GET /liquidity
        /// View: Index.cshtml
        /// Model: LiquidityPageDataModel
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Get comprehensive liquidity page data
                var pageData = await _liquidityService.GetLiquidityPageDataAsync();

                if (pageData == null)
                {
                    _logger.LogWarning("Liquidity page data returned null");
                    pageData = new LiquidityPageDataModel();
                }

                // Set page metadata
                ViewData["Title"] = "Liquidity Dashboard - TEACH Token Pool Management";
                ViewData["Description"] = "Manage your TEACH token liquidity positions, view pool analytics, and discover new yield farming opportunities.";

                // Serialize data for JavaScript initialization
                ViewBag.JsonData = JsonSerializer.Serialize(pageData, _jsonOptions);
                ViewBag.InitialData = pageData;

                return View(pageData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading liquidity dashboard");
                ViewBag.ErrorMessage = "Unable to load liquidity data. Please try again.";

                // Return empty model for graceful fallback
                var fallbackModel = new LiquidityPageDataModel();
                ViewBag.JsonData = JsonSerializer.Serialize(fallbackModel, _jsonOptions);
                ViewBag.InitialData = fallbackModel;

                return View(fallbackModel);
            }
        }

        /// <summary>
        /// Add liquidity wizard - step-by-step liquidity addition process
        /// Route: GET /liquidity/add?step=1
        /// View: Add.cshtml
        /// Model: AddLiquidityModel
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Add(int step = 1)
        {
            try
            {
                // Validate step parameter
                if (step < 1 || step > 5)
                    step = 1;

                // Get wizard data
                var wizardData = await _liquidityService.GetAddLiquidityWizardDataAsync();

                if (wizardData == null)
                {
                    _logger.LogWarning("Add liquidity wizard data returned null");
                    wizardData = new AddLiquidityModel();
                }

                wizardData.CurrentStep = step;

                // Set page metadata based on step
                ViewData["Title"] = $"Add Liquidity - Step {step} - TEACH Token";
                ViewData["Description"] = step switch
                {
                    1 => "Step 1: Select a liquidity pool to provide liquidity to",
                    2 => "Step 2: Choose the decentralized exchange to use",
                    3 => "Step 3: Specify the amounts of tokens to add",
                    4 => "Step 4: Review your transaction details before submitting",
                    5 => "Step 5: Monitor your transaction progress",
                    _ => "Step-by-step wizard to add liquidity to TEACH token pools"
                };

                // Serialize data for JavaScript
                ViewBag.JsonData = JsonSerializer.Serialize(wizardData, _jsonOptions);
                ViewBag.InitialData = wizardData;
                ViewBag.CurrentStep = step;

                return View(wizardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading add liquidity wizard for step {Step}", step);
                ViewBag.ErrorMessage = "Unable to load wizard data. Please try again.";

                var fallbackModel = new AddLiquidityModel { CurrentStep = step };
                ViewBag.JsonData = JsonSerializer.Serialize(fallbackModel, _jsonOptions);
                ViewBag.InitialData = fallbackModel;

                return View(fallbackModel);
            }
        }

        /// <summary>
        /// Manage liquidity positions - view and manage existing positions
        /// Route: GET /liquidity/manage?walletAddress=0x...
        /// View: Manage.cshtml
        /// Model: UserLiquidityInfoModel
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Manage(string? walletAddress)
        {
            try
            {
                UserLiquidityInfoModel userInfo;

                if (string.IsNullOrEmpty(walletAddress))
                {
                    // No wallet connected - show connection prompt
                    userInfo = new UserLiquidityInfoModel();
                }
                else
                {
                    // Load user's liquidity information
                    userInfo = await _liquidityService.GetUserLiquidityInfoAsync(walletAddress);

                    if (userInfo == null)
                    {
                        _logger.LogWarning("User liquidity info returned null for {WalletAddress}", walletAddress);
                        userInfo = new UserLiquidityInfoModel { WalletAddress = walletAddress };
                    }
                }

                // Set page metadata
                ViewData["Title"] = "Manage Liquidity Positions - TEACH Token";
                ViewData["Description"] = string.IsNullOrEmpty(walletAddress)
                    ? "Connect your wallet to view and manage your TEACH token liquidity positions"
                    : "View and manage your TEACH token liquidity positions, track performance, and claim rewards.";

                // Serialize data for JavaScript
                ViewBag.JsonData = JsonSerializer.Serialize(userInfo, _jsonOptions);
                ViewBag.InitialData = userInfo;
                ViewBag.WalletAddress = walletAddress;

                return View(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user liquidity management for {WalletAddress}", walletAddress);
                ViewBag.ErrorMessage = "Unable to load position data. Please try again.";

                var fallbackModel = new UserLiquidityInfoModel { WalletAddress = walletAddress ?? string.Empty };
                ViewBag.JsonData = JsonSerializer.Serialize(fallbackModel, _jsonOptions);
                ViewBag.InitialData = fallbackModel;
                ViewBag.WalletAddress = walletAddress;

                return View(fallbackModel);
            }
        }

        #endregion

        #region AJAX Endpoints - Dashboard Data

        /// <summary>
        /// Get dashboard data for AJAX updates
        /// Route: GET /liquidity/data
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                var data = await _liquidityService.GetLiquidityPageDataAsync();
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard data");
                return Json(new { error = "Unable to load dashboard data" });
            }
        }

        /// <summary>
        /// Get current liquidity statistics
        /// Route: GET /liquidity/stats
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _liquidityService.GetLiquidityStatsAsync();
                return Json(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting liquidity stats");
                return Json(new { error = "Unable to load statistics" });
            }
        }

        /// <summary>
        /// Get liquidity analytics
        /// Route: GET /liquidity/analytics
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAnalytics()
        {
            try
            {
                var analytics = await _liquidityService.GetLiquidityAnalyticsAsync();
                return Json(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting liquidity analytics");
                return Json(new { error = "Unable to load analytics" });
            }
        }

        /// <summary>
        /// Get specific pool details
        /// Route: GET /liquidity/pool/{poolId}
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPoolDetails(int poolId)
        {
            try
            {
                var pool = await _liquidityService.GetLiquidityPoolDetailsAsync(poolId);

                if (pool == null)
                    return NotFound(new { error = "Pool not found" });

                return Json(pool);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pool details for pool {PoolId}", poolId);
                return Json(new { error = "Unable to load pool details" });
            }
        }

        /// <summary>
        /// Get active pools
        /// Route: GET /liquidity/pools/active
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetActivePools()
        {
            try
            {
                var pools = await _liquidityService.GetActiveLiquidityPoolsAsync();
                return Json(pools);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active pools");
                return Json(new { error = "Unable to load active pools" });
            }
        }

        /// <summary>
        /// Get DEX configurations
        /// Route: GET /liquidity/dex-configs
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDexConfigurations()
        {
            try
            {
                var dexes = await _liquidityService.GetDexConfigurationsAsync();
                return Json(dexes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting DEX configurations");
                return Json(new { error = "Unable to load DEX configurations" });
            }
        }

        #endregion

        #region AJAX Endpoints - Wizard Operations

        /// <summary>
        /// Get wizard initialization data
        /// Route: GET /liquidity/wizard-data
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetWizardData(string? walletAddress = null)
        {
            try
            {
                var data = await _liquidityService.GetAddLiquidityWizardDataAsync(walletAddress);
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wizard data");
                return Json(new { error = "Unable to load wizard data" });
            }
        }

        /// <summary>
        /// Calculate liquidity amounts and estimates
        /// Route: POST /liquidity/calculate
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CalculateLiquidity([FromBody] LiquidityCalculationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

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
                _logger.LogError(ex, "Error calculating liquidity");
                return Json(new { error = "Unable to calculate liquidity" });
            }
        }

        /// <summary>
        /// Calculate remove liquidity preview
        /// Route: POST /liquidity/calculate-remove
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CalculateRemoveLiquidity([FromBody] RemoveLiquidityCalculationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var calculation = await _liquidityService.CalculateRemoveLiquidityPreviewAsync(
                    request.WalletAddress,
                    request.PositionId,
                    request.PercentageToRemove);

                return Json(calculation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating remove liquidity");
                return Json(new { error = "Unable to calculate removal" });
            }
        }

        /// <summary>
        /// Get liquidity guide steps
        /// Route: GET /liquidity/guide
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetGuideSteps(string? dexName = null)
        {
            try
            {
                var steps = await _liquidityService.GetLiquidityGuideStepsAsync(dexName);
                return Json(steps);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guide steps");
                return Json(new { error = "Unable to load guide steps" });
            }
        }

        #endregion

        #region AJAX Endpoints - User Management

        /// <summary>
        /// Get user's liquidity positions
        /// Route: GET /liquidity/user/{address}/positions
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUserPositions(string address)
        {
            try
            {
                if (string.IsNullOrEmpty(address))
                    return BadRequest(new { error = "Wallet address required" });

                var positions = await _liquidityService.GetUserLiquidityPositionsAsync(address);
                return Json(positions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user positions for {Address}", address);
                return Json(new { error = "Unable to load positions" });
            }
        }

        /// <summary>
        /// Get user's comprehensive liquidity info
        /// Route: GET /liquidity/user/{address}/info
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUserInfo(string address)
        {
            try
            {
                if (string.IsNullOrEmpty(address))
                    return BadRequest(new { error = "Wallet address required" });

                var userInfo = await _liquidityService.GetUserLiquidityInfoAsync(address);
                return Json(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user info for {Address}", address);
                return Json(new { error = "Unable to load user information" });
            }
        }

        /// <summary>
        /// Get user's transaction history
        /// Route: GET /liquidity/user/{address}/transactions
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUserTransactions(string address, int page = 1, int pageSize = 20)
        {
            try
            {
                if (string.IsNullOrEmpty(address))
                    return BadRequest(new { error = "Wallet address required" });

                var transactions = await _liquidityService.GetUserTransactionHistoryAsync(address, page, pageSize);
                return Json(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user transactions for {Address}", address);
                return Json(new { error = "Unable to load transaction history" });
            }
        }

        /// <summary>
        /// Clear user cache
        /// Route: POST /liquidity/user/{address}/clear-cache
        /// </summary>
        [HttpPost]
        public IActionResult ClearUserCache(string address)
        {
            try
            {
                if (string.IsNullOrEmpty(address))
                    return BadRequest(new { error = "Wallet address required" });

                _liquidityService.ClearUserCache(address);
                return Json(new { success = true, message = "User cache cleared" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing user cache for {Address}", address);
                return Json(new { error = "Unable to clear cache" });
            }
        }

        #endregion

        #region Error Handling

        /// <summary>
        /// Handle errors and return appropriate response
        /// </summary>
        private IActionResult HandleError(Exception ex, string action)
        {
            _logger.LogError(ex, "Error in {Action}", action);

            if (Request.Headers.Accept.Contains("application/json"))
            {
                return Json(new { error = $"An error occurred in {action}. Please try again." });
            }

            ViewBag.ErrorMessage = "An error occurred. Please try again.";
            return View("Error");
        }

        #endregion
    }
}