using Microsoft.AspNetCore.Mvc;
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models;

namespace TeachCrowdSale.Web.Controllers
{
    /// <summary>
    /// Web controller for staking dashboard operations
    /// </summary>
    public class StakingController : Controller
    {
        private readonly IStakingDashboardService _stakingService;
        private readonly ILogger<StakingController> _logger;

        public StakingController(
            IStakingDashboardService stakingService,
            ILogger<StakingController> logger)
        {
            _stakingService = stakingService ?? throw new ArgumentNullException(nameof(stakingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Main Views

        /// <summary>
        /// Main staking dashboard page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                ViewData["Title"] = "Staking Dashboard - TeachToken";
                ViewData["Description"] = "Stake TEACH tokens and support education with our unique 50/50 reward sharing model. Choose your school beneficiary and start earning rewards while making a difference.";
                ViewData["Keywords"] = "staking, TEACH token, education funding, rewards, school support, cryptocurrency";

                // Get initial dashboard data
                var stakingStats = await _stakingService.GetStakingStatsAsync();
                var pools = await _stakingService.GetActiveStakingPoolsAsync();
                var schools = await _stakingService.GetAvailableSchoolsAsync();

                // Pass data to view via ViewBag
                ViewBag.StakingStats = stakingStats;
                ViewBag.StakingPools = pools ?? new List<StakingPool>();
                ViewBag.AvailableSchools = schools ?? new List<SchoolBeneficiaryModel>();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading staking dashboard");
                return View("Error");
            }
        }

        #endregion

        #region AJAX Endpoints

        /// <summary>
        /// Get user staking data via AJAX
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUserData(string walletAddress)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(walletAddress))
                {
                    return BadRequest("Wallet address is required");
                }

                var userInfo = await _stakingService.GetUserStakingInfoAsync(walletAddress);
                return Json(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user staking data for {WalletAddress}", walletAddress);
                return StatusCode(500, new { error = "Failed to fetch user data" });
            }
        }

        /// <summary>
        /// Get current pool data via AJAX
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPoolData()
        {
            try
            {
                var pools = await _stakingService.GetActiveStakingPoolsAsync();
                return Json(pools);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pool data");
                return StatusCode(500, new { error = "Failed to fetch pool data" });
            }
        }

        /// <summary>
        /// Calculate staking rewards preview
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CalculateRewards(string walletAddress, int poolId, decimal amount, int lockPeriodDays = 30)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(walletAddress) || amount <= 0)
                {
                    return BadRequest("Valid wallet address and amount are required");
                }

                var calculation = await _stakingService.CalculateStakingPreviewAsync(walletAddress, poolId, amount, lockPeriodDays);
                return Json(calculation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating staking rewards");
                return StatusCode(500, new { error = "Failed to calculate rewards" });
            }
        }

        /// <summary>
        /// Get available schools for beneficiary selection
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSchools()
        {
            try
            {
                var schools = await _stakingService.GetAvailableSchoolsAsync();
                return Json(schools);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching schools");
                return StatusCode(500, new { error = "Failed to fetch schools" });
            }
        }

        /// <summary>
        /// Get reward projections for specific parameters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRewardProjections(string walletAddress, int poolId, decimal amount, int days = 365)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(walletAddress) || amount <= 0)
                {
                    return BadRequest("Valid wallet address and amount are required");
                }

                var projections = await _stakingService.GetRewardProjectionsAsync(walletAddress, poolId, amount, days);
                return Json(projections);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching reward projections");
                return StatusCode(500, new { error = "Failed to fetch projections" });
            }
        }

        /// <summary>
        /// Get user's current claimable rewards
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetClaimableRewards(string walletAddress, int poolId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(walletAddress))
                {
                    return BadRequest("Wallet address is required");
                }

                var rewards = await _stakingService.CalculateStakingRewardsAsync(walletAddress, poolId);
                return Json(new { claimableRewards = rewards });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching claimable rewards");
                return StatusCode(500, new { error = "Failed to fetch claimable rewards" });
            }
        }

        /// <summary>
        /// Get refreshed staking statistics
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStakingStats()
        {
            try
            {
                var stats = await _stakingService.GetStakingStatsAsync();
                return Json(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching staking statistics");
                return StatusCode(500, new { error = "Failed to fetch statistics" });
            }
        }

        #endregion

        #region Validation Helpers

        /// <summary>
        /// Validate staking parameters
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ValidateStakeParameters([FromBody] StakeValidationRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.WalletAddress) || request.Amount <= 0)
                {
                    return Json(new { isValid = false, error = "Invalid parameters" });
                }

                var pool = (await _stakingService.GetActiveStakingPoolsAsync())?.FirstOrDefault(p => p.Id == request.PoolId);
                if (pool == null)
                {
                    return Json(new { isValid = false, error = "Pool not found" });
                }

                var isValid = request.Amount >= pool.MinStakeAmount && request.Amount <= pool.MaxStakeAmount;
                var errors = new List<string>();

                if (request.Amount < pool.MinStakeAmount)
                    errors.Add($"Minimum stake amount is {pool.MinStakeAmount:N0} TEACH");

                if (request.Amount > pool.MaxStakeAmount)
                    errors.Add($"Maximum stake amount is {pool.MaxStakeAmount:N0} TEACH");

                return Json(new
                {
                    isValid,
                    errors,
                    pool = new
                    {
                        pool.Name,
                        pool.LockPeriodDays,
                        TotalAPY = pool.BaseAPY + pool.BonusAPY,
                        pool.MinStakeAmount,
                        pool.MaxStakeAmount
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating stake parameters");
                return StatusCode(500, new { error = "Validation failed" });
            }
        }

        #endregion
    }

    /// <summary>
    /// Request model for stake validation
    /// </summary>
    public class StakeValidationRequest
    {
        public string WalletAddress { get; set; } = string.Empty;
        public int PoolId { get; set; }
        public decimal Amount { get; set; }
        public int LockPeriodDays { get; set; }
    }
}