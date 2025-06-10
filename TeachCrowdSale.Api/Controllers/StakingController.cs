using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models;
using TeachCrowdSale.Core.Models.Request;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Api.Controllers;

[EnableRateLimiting("Staking")]
[ApiController]
[Authorize]
[Route("api/staking")]
public class StakingApiController : ControllerBase
{
    private readonly IStakingService _stakingService;
    private readonly IBlockchainService _blockchainService;
    private readonly ILogger<StakingApiController> _logger;

    public StakingApiController(
        IStakingService stakingService,
        IBlockchainService blockchainService,
        ILogger<StakingApiController> logger)
    {
        _stakingService = stakingService ?? throw new ArgumentNullException(nameof(stakingService));
        _blockchainService = blockchainService ?? throw new ArgumentNullException(nameof(blockchainService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get comprehensive staking page data
    /// </summary>
    [HttpGet("data")]
    [ResponseCache(Duration = 60)]
    public async Task<ActionResult<StakingPageDataModel>> GetStakingPageData()
    {
        try
        {
            var data = new StakingPageDataModel();

            // Get staking pools
            var pools = await _stakingService.GetActiveStakingPoolsAsync();
            data.StakingPools = pools.Select(MapToStakingPoolDisplayModel).ToList();

            // Get staking statistics
            data.StakingStats = await _stakingService.GetStakingStatsAsync();

            // Get school beneficiaries
            var schools = await _stakingService.GetActiveSchoolsAsync();
            data.Schools = schools.Select(MapToSchoolBeneficiaryModel).ToList();

            // Get analytics
            data.Analytics = await _stakingService.GetStakingAnalyticsAsync();

            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staking page data");
            return StatusCode(500, new ErrorResponse
            {
                Message = "Error retrieving staking data",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    /// <summary>
    /// Get staking pools
    /// </summary>
    [HttpGet("pools")]
    [ResponseCache(Duration = 300)]
    public async Task<ActionResult<List<StakingPoolDisplayModel>>> GetStakingPools()
    {
        try
        {
            var pools = await _stakingService.GetActiveStakingPoolsAsync();
            var poolModels = pools.Select(MapToStakingPoolDisplayModel).ToList();

            return Ok(poolModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staking pools");
            return StatusCode(500, new ErrorResponse
            {
                Message = "Error retrieving staking pools",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    /// <summary>
    /// Get staking statistics
    /// </summary>
    [HttpGet("stats")]
    [ResponseCache(Duration = 120)]
    public async Task<ActionResult<StakingStatsModel>> GetStakingStats()
    {
        try
        {
            var stats = await _stakingService.GetStakingStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staking statistics");
            return StatusCode(500, new ErrorResponse
            {
                Message = "Error retrieving staking statistics",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    /// <summary>
    /// Get user's staking information
    /// </summary>
    [HttpGet("user/{address}")]
    public async Task<ActionResult<UserStakingInfoModel>> GetUserStakingInfo([FromRoute] string address)
    {
        try
        {
            if (!_blockchainService.IsValidAddress(address))
            {
                return BadRequest(new ErrorResponse { Message = "Invalid Ethereum address" });
            }

            var userInfo = await _stakingService.GetUserStakingInfoAsync(address);
            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user staking info for {Address}", address);
            return StatusCode(500, new ErrorResponse
            {
                Message = "Error retrieving user staking information",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    /// <summary>
    /// Calculate staking preview
    /// </summary>
    [HttpPost("calculate")]
    public async Task<ActionResult<StakingCalculationModel>> CalculateStaking([FromBody] StakingCalculationRequest request)
    {
        try
        {
            if (!_blockchainService.IsValidAddress(request.WalletAddress))
            {
                return BadRequest(new ErrorResponse { Message = "Invalid Ethereum address" });
            }

            var pool = await _stakingService.GetStakingPoolAsync(request.PoolId);
            if (pool == null)
            {
                return NotFound(new ErrorResponse { Message = $"Staking pool {request.PoolId} not found" });
            }

            var lockPeriod = request.LockPeriodDays ?? pool.LockPeriodDays;
            var calculation = await _stakingService.CalculateStakingPreviewAsync(
                request.WalletAddress, request.PoolId, request.Amount, lockPeriod);

            return Ok(calculation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating staking preview");
            return StatusCode(500, new ErrorResponse
            {
                Message = "Error calculating staking preview",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    /// <summary>
    /// Get reward projections
    /// </summary>
    [HttpPost("projections")]
    public async Task<ActionResult<List<RewardProjectionModel>>> GetRewardProjections([FromBody] RewardProjectionsRequest request)
    {
        try
        {
            if (!_blockchainService.IsValidAddress(request.WalletAddress))
            {
                return BadRequest(new ErrorResponse { Message = "Invalid Ethereum address" });
            }

            var projections = await _stakingService.GetRewardProjectionsAsync(
                request.WalletAddress, request.PoolId, request.Amount, request.LockPeriodDays);

            return Ok(projections);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating reward projections");
            return StatusCode(500, new ErrorResponse
            {
                Message = "Error calculating reward projections",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    /// <summary>
    /// Stake tokens
    /// </summary>
    [EnableRateLimiting("StakingTransaction")]
    [HttpPost("stake")]
    public async Task<ActionResult> StakeTokens([FromBody] StakeTokensRequest request)
    {
        try
        {
            if (!_blockchainService.IsValidAddress(request.WalletAddress))
            {
                return BadRequest(new ErrorResponse { Message = "Invalid Ethereum address" });
            }

            var success = await _stakingService.StakeTokensAsync(
                request.WalletAddress, request.PoolId, request.Amount, request.SchoolBeneficiaryId);

            if (!success)
            {
                return StatusCode(500, new ErrorResponse { Message = "Failed to stake tokens" });
            }

            return Ok(new { message = "Tokens staked successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error staking tokens for {WalletAddress}", request.WalletAddress);
            return StatusCode(500, new ErrorResponse
            {
                Message = "An error occurred while staking tokens",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    /// <summary>
    /// Unstake tokens
    /// </summary>
    [EnableRateLimiting("StakingTransaction")]
    [HttpPost("unstake")]
    public async Task<ActionResult> UnstakeTokens([FromBody] UnstakeTokensRequest request)
    {
        try
        {
            if (!_blockchainService.IsValidAddress(request.WalletAddress))
            {
                return BadRequest(new ErrorResponse { Message = "Invalid Ethereum address" });
            }

            var success = await _stakingService.UnstakeTokensAsync(request.WalletAddress, request.StakeId);

            if (!success)
            {
                return StatusCode(500, new ErrorResponse { Message = "Failed to unstake tokens" });
            }

            return Ok(new { message = "Tokens unstaked successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unstaking tokens for {WalletAddress}", request.WalletAddress);
            return StatusCode(500, new ErrorResponse
            {
                Message = "An error occurred while unstaking tokens",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    /// <summary>
    /// Claim staking rewards
    /// </summary>
    [EnableRateLimiting("StakingTransaction")]
    [HttpPost("claim")]
    public async Task<ActionResult> ClaimRewards([FromBody] ClaimRewardsRequest request)
    {
        try
        {
            if (!_blockchainService.IsValidAddress(request.WalletAddress))
            {
                return BadRequest(new ErrorResponse { Message = "Invalid Ethereum address" });
            }

            var success = await _stakingService.ClaimRewardsAsync(request.WalletAddress, request.StakeId);

            if (!success)
            {
                return StatusCode(500, new ErrorResponse { Message = "Failed to claim rewards" });
            }

            return Ok(new { message = "Rewards claimed successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error claiming rewards for {WalletAddress}", request.WalletAddress);
            return StatusCode(500, new ErrorResponse
            {
                Message = "An error occurred while claiming rewards",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    /// <summary>
    /// Compound staking rewards
    /// </summary>
    [EnableRateLimiting("StakingTransaction")]
    [HttpPost("compound")]
    public async Task<ActionResult> CompoundRewards([FromBody] CompoundRewardsRequest request)
    {
        try
        {
            if (!_blockchainService.IsValidAddress(request.WalletAddress))
            {
                return BadRequest(new ErrorResponse { Message = "Invalid Ethereum address" });
            }

            var success = await _stakingService.CompoundRewardsAsync(request.WalletAddress, request.StakeId);

            if (!success)
            {
                return StatusCode(500, new ErrorResponse { Message = "Failed to compound rewards" });
            }

            return Ok(new { message = "Rewards compounded successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compounding rewards for {WalletAddress}", request.WalletAddress);
            return StatusCode(500, new ErrorResponse
            {
                Message = "An error occurred while compounding rewards",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    /// <summary>
    /// Get school beneficiaries
    /// </summary>
    [HttpGet("schools")]
    [ResponseCache(Duration = 600)]
    public async Task<ActionResult<List<SchoolBeneficiaryModel>>> GetSchools([FromQuery] SchoolSearchRequest request)
    {
        try
        {
            var schools = await _stakingService.GetActiveSchoolsAsync(request.Country, request.State);
            var schoolModels = schools.Select(MapToSchoolBeneficiaryModel).ToList();

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLowerInvariant();
                schoolModels = schoolModels.Where(s =>
                    s.Name.ToLowerInvariant().Contains(searchTerm) ||
                    s.Description.ToLowerInvariant().Contains(searchTerm) ||
                    s.City.ToLowerInvariant().Contains(searchTerm)
                ).ToList();
            }

            // Apply pagination
            var totalCount = schoolModels.Count;
            var pagedSchools = schoolModels
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            Response.Headers.Add("X-Total-Count", totalCount.ToString());
            Response.Headers.Add("X-Page-Size", request.PageSize.ToString());
            Response.Headers.Add("X-Page-Number", request.PageNumber.ToString());

            return Ok(pagedSchools);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving schools");
            return StatusCode(500, new ErrorResponse
            {
                Message = "Error retrieving school beneficiaries",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    /// <summary>
    /// Select school beneficiary
    /// </summary>
    [HttpPost("select-school")]
    public async Task<ActionResult> SelectSchoolBeneficiary([FromBody] SelectSchoolBeneficiaryRequest request)
    {
        try
        {
            if (!_blockchainService.IsValidAddress(request.WalletAddress))
            {
                return BadRequest(new ErrorResponse { Message = "Invalid Ethereum address" });
            }

            var success = await _stakingService.SelectSchoolBeneficiaryAsync(
                request.WalletAddress, request.SchoolBeneficiaryId);

            if (!success)
            {
                return StatusCode(500, new ErrorResponse { Message = "Failed to select school beneficiary" });
            }

            return Ok(new { message = "School beneficiary selected successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting school beneficiary for {WalletAddress}", request.WalletAddress);
            return StatusCode(500, new ErrorResponse
            {
                Message = "An error occurred while selecting school beneficiary",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    /// <summary>
    /// Get staking analytics
    /// </summary>
    [HttpGet("analytics")]
    [ResponseCache(Duration = 300)]
    public async Task<ActionResult<StakingAnalyticsModel>> GetStakingAnalytics()
    {
        try
        {
            var analytics = await _stakingService.GetStakingAnalyticsAsync();
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staking analytics");
            return StatusCode(500, new ErrorResponse
            {
                Message = "Error retrieving staking analytics",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    /// <summary>
    /// Get top stakers leaderboard
    /// </summary>
    [HttpGet("leaderboard")]
    [ResponseCache(Duration = 600)]
    public async Task<ActionResult<List<TopStakerModel>>> GetTopStakers([FromQuery] int limit = 10)
    {
        try
        {
            if (limit <= 0 || limit > 100)
            {
                return BadRequest(new ErrorResponse { Message = "Limit must be between 1 and 100" });
            }

            var topStakers = await _stakingService.GetTopStakersAsync(limit);
            return Ok(topStakers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving top stakers");
            return StatusCode(500, new ErrorResponse
            {
                Message = "Error retrieving top stakers",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    #region Private Helper Methods

    private StakingPoolDisplayModel MapToStakingPoolDisplayModel(Core.Data.Entities.StakingPool pool)
    {
        var totalAPY = pool.BaseAPY + pool.BonusAPY;
        var utilization = pool.MaxPoolSize > 0 ? (pool.TotalStaked / pool.MaxPoolSize) * 100 : 0;

        return new StakingPoolDisplayModel
        {
            Id = pool.Id,
            Name = pool.Name,
            Description = pool.Description,
            MinStakeAmount = pool.MinStakeAmount,
            MaxStakeAmount = pool.MaxStakeAmount,
            LockPeriodDays = pool.LockPeriodDays,
            BaseAPY = pool.BaseAPY,
            BonusAPY = pool.BonusAPY,
            TotalAPY = totalAPY,
            TotalStaked = pool.TotalStaked,
            MaxPoolSize = pool.MaxPoolSize,
            PoolUtilization = utilization,
            IsActive = pool.IsActive,
            IsRecommended = totalAPY >= 15m, // Mark pools with 15%+ APY as recommended
            LockPeriodDisplay = FormatLockPeriod(pool.LockPeriodDays),
            PoolCategory = GetPoolCategory(pool.LockPeriodDays),
            StatusClass = pool.IsActive ? "active" : "inactive"
        };
    }

    private SchoolBeneficiaryModel MapToSchoolBeneficiaryModel(Core.Data.Entities.SchoolBeneficiary school)
    {
        return new SchoolBeneficiaryModel
        {
            Id = school.Id,
            Name = school.Name,
            Description = school.Description,
            Country = school.Country,
            State = school.State,
            City = school.City,
            LogoUrl = school.LogoUrl,
            StudentCount = school.StudentCount,
            TotalFundingReceived = school.TotalReceived,
            SupporterCount = 0, // Would be calculated from user stakes
            IsVerified = school.IsVerified,
            Location = $"{school.City}, {school.State}, {school.Country}".Trim(new[] { ',', ' ' }),
            ImpactSummary = $"Supporting {school.StudentCount} students with educational funding"
        };
    }

    private string FormatLockPeriod(int days)
    {
        if (days == 0) return "No Lock";
        if (days < 30) return $"{days} days";
        if (days < 365) return $"{days / 30} months";
        return $"{days / 365} years";
    }

    private string GetPoolCategory(int lockPeriodDays)
    {
        return lockPeriodDays switch
        {
            0 => "Flexible",
            <= 30 => "Short Term",
            <= 180 => "Medium Term",
            <= 365 => "Long Term",
            _ => "Extended"
        };
    }

    #endregion
}