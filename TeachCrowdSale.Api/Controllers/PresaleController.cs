using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using TeachCrowdSale.Api.Models;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Models;
using Microsoft.Extensions.Logging;
using TeachCrowdSale.Core.Models.Request;
using TeachCrowdSale.Core.Models.Response;
using Microsoft.AspNetCore.RateLimiting;
using TeachCrowdSale.Api.Validator;

namespace TeachCrowdSale.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/presale")]
    public class PresaleController : ControllerBase
    {
        private readonly IBlockchainService _blockchainService;
        private readonly IPresaleService _presaleService;
        private readonly ILogger<PresaleController> _logger;

        public PresaleController(
            IBlockchainService blockchainService,
            IPresaleService presaleService,
            ILogger<PresaleController> logger)
        {
            _blockchainService = blockchainService ?? throw new ArgumentNullException(nameof(blockchainService));
            _presaleService = presaleService ?? throw new ArgumentNullException(nameof(presaleService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("current-tier")]
        public async Task<ActionResult<TierModel>> GetCurrentTier()
        {
            try
            {
                var currentTier = await _presaleService.GetCurrentTierAsync();

                return Ok(new TierModel
                {
                    Id = currentTier.Id,
                    Name = GetTierName(currentTier.Id),
                    Price = currentTier.Price,
                    MinPurchase = currentTier.MinPurchase,
                    MaxPurchase = currentTier.MaxPurchase,
                    TotalAllocation = currentTier.Allocation,
                    Sold = currentTier.Sold,
                    VestingTGE = currentTier.VestingTGE,
                    VestingMonths = currentTier.VestingMonths,
                    IsActive = currentTier.IsActive,
                    EndTime = await _presaleService.GetTierEndTimeAsync(currentTier.Id)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving current tier: {ex.Message}");
            }
        }

        [HttpGet("status")]
        public async Task<ActionResult<PresaleStatusModel>> GetPresaleStatus()
        {
            try
            {
                var currentTier = await _presaleService.GetCurrentTierAsync();
                var presaleStats = await _presaleService.GetPresaleStatsAsync();

                return Ok(new PresaleStatusModel
                {
                    TotalRaised = presaleStats.TotalRaised,
                    FundingGoal = presaleStats.FundingGoal,
                    ParticipantsCount = presaleStats.ParticipantsCount,
                    TokensSold = presaleStats.TokensSold,
                    CurrentTier = new TierModel
                    {
                        Id = currentTier.Id,
                        Name = GetTierName(currentTier.Id),
                        Price = currentTier.Price,
                        MinPurchase = currentTier.MinPurchase,
                        MaxPurchase = currentTier.MaxPurchase,
                        TotalAllocation = currentTier.Allocation,
                        Sold = currentTier.Sold,
                        VestingTGE = currentTier.VestingTGE,
                        VestingMonths = currentTier.VestingMonths,
                        IsActive = currentTier.IsActive,
                        EndTime = await _presaleService.GetTierEndTimeAsync(currentTier.Id)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving presale status: {ex.Message}");
            }
        }

        [HttpGet("tiers")]
        public async Task<ActionResult<List<TierModel>>> GetAllTiers()
        {
            try
            {
                var tiers = await _presaleService.GetAllTiersAsync();
                var result = new List<TierModel>();

                foreach (var tier in tiers)
                {
                    result.Add(new TierModel
                    {
                        Id = tier.Id,
                        Name = GetTierName(tier.Id),
                        Price = tier.Price,
                        MinPurchase = tier.MinPurchase,
                        MaxPurchase = tier.MaxPurchase,
                        TotalAllocation = tier.Allocation,
                        Sold = tier.Sold,
                        VestingTGE = tier.VestingTGE,
                        VestingMonths = tier.VestingMonths,
                        IsActive = tier.IsActive,
                        EndTime = await _presaleService.GetTierEndTimeAsync(tier.Id)
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving tiers: {ex.Message}");
            }
        }

        [HttpGet("user/{address}")]
        public async Task<ActionResult<UserPurchaseModel>> GetUserPurchases([FromRoute] string address)
        {
            try
            {
                if (!_blockchainService.IsValidAddress(address))
                {
                    return BadRequest("Invalid Ethereum address");
                }

                var userPurchase = await _presaleService.GetUserPurchaseAsync(address);

                if (userPurchase == null)
                {
                    return NotFound($"No purchases found for address {address}");
                }

                // Calculate claimable tokens based on vesting schedule
                var claimableTokens = await _presaleService.GetClaimableTokensAsync(address);

                return Ok(new UserPurchaseModel
                {
                    Address = address,
                    TotalTokens = userPurchase.Tokens,
                    UsdAmount = userPurchase.UsdAmount,
                    ClaimableTokens = claimableTokens,
                    TierPurchases = userPurchase.TierAmounts
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving user purchases: {ex.Message}");
            }
        }

        [HttpGet("next-vesting/{address}")]
        public async Task<ActionResult<VestingMilestoneModel>> GetNextVestingMilestone([FromRoute] string address)
        {
            try
            {
                if (!_blockchainService.IsValidAddress(address))
                {
                    return BadRequest("Invalid Ethereum address");
                }

                var nextVesting = await _presaleService.GetNextVestingMilestoneAsync(address);

                if (nextVesting == null)
                {
                    return NotFound($"No vesting schedule found for address {address}");
                }

                return Ok(new VestingMilestoneModel
                {
                    Timestamp = nextVesting.Timestamp,
                    Amount = nextVesting.Amount,
                    FormattedDate = nextVesting.Timestamp.ToString("MMMM dd, yyyy")
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving next vesting milestone: {ex.Message}");
            }
        }

        [HttpGet("contracts")]
        public ActionResult<ContractAddressesModel> GetContractAddresses()
        {
            try
            {
                var contractAddresses = _blockchainService.GetContractAddresses();

                return Ok(new ContractAddressesModel
                {
                    PresaleAddress = contractAddresses.PresaleAddress,
                    TokenAddress = contractAddresses.TokenAddress,
                    PaymentTokenAddress = contractAddresses.PaymentTokenAddress,
                    NetworkId = contractAddresses.NetworkId,
                    ChainName = contractAddresses.ChainName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving contract addresses: {ex.Message}");
            }
        }

        private string GetTierName(int tierId)
        {
            return tierId switch
            {
                1 => "Seed Round",
                2 => "Community Round",
                3 => "Growth Round",
                4 => "Expansion Round",
                5 => "Adoption Round",
                6 => "Launch Round",
                7 => "Final Round",
                _ => $"Tier {tierId}"
            };
        }

        [EnableRateLimiting("Purchase")]
        [HttpPost("purchase")]
        public async Task<ActionResult> PurchaseTokens([FromBody] PurchaseRequestModel request)
        {
            try
            {
                var validator = new PurchaseRequestValidator(_presaleService);
                var validationResult = await validator.ValidateAsync(request);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Validation failed",
                        ValidationErrors = validationResult.Errors
                            .GroupBy(x => x.PropertyName)
                            .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray())
                    });
                }

                // Get current tier information
                var tiers = await _presaleService.GetAllTiersAsync();
                var tier = tiers.FirstOrDefault(t => t.Id == request.TierId);

                if (tier == null)
                {
                    return NotFound(new ErrorResponse { Message = $"Tier with ID {request.TierId} not found" });
                }

                if (!tier.IsActive)
                {
                    return BadRequest(new ErrorResponse { Message = "Selected tier is not currently active" });
                }

                // Validate purchase amount
                if (request.Amount < tier.MinPurchase)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = $"Purchase amount is below minimum ({tier.MinPurchase} USDC)"
                    });
                }

                if (request.Amount > tier.MaxPurchase)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = $"Purchase amount is above maximum ({tier.MaxPurchase} USDC)"
                    });
                }

                // Execute the purchase
                var success = await _presaleService.PurchaseTokensAsync(request.Address, request.TierId, request.Amount);

                if (!success)
                {
                    return StatusCode(500, new ErrorResponse { Message = "Failed to process token purchase" });
                }

                // Get updated user purchase info
                var userPurchase = await _presaleService.GetUserPurchaseAsync(request.Address);

                // Calculate tokens received
                var tokensReceived = request.Amount / tier.Price;

                return Ok(new PurchaseResponseModel
                {
                    Address = request.Address,
                    TierId = request.TierId,
                    Amount = request.Amount,
                    TokensReceived = tokensReceived,
                    TransactionTime = DateTime.UtcNow,
                    TotalTokens = userPurchase?.Tokens ?? tokensReceived
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing purchase for address {request.Address}");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while processing the purchase",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        [HttpPost("purchase-validation")]
        public async Task<ActionResult<PurchaseValidationModel>> ValidatePurchase(
            [FromBody] PurchaseRequestModel request)
        {
            try
            {
                if (!_blockchainService.IsValidAddress(request.Address))
                {
                    return BadRequest(new ErrorResponse { Message = "Invalid Ethereum address" });
                }

                // Get current tier information
                var tiers = await _presaleService.GetAllTiersAsync();
                var tier = tiers.FirstOrDefault(t => t.Id == request.TierId);

                if (tier == null)
                {
                    return NotFound(new ErrorResponse { Message = $"Tier with ID {request.TierId} not found" });
                }

                if (!tier.IsActive)
                {
                    return BadRequest(new ErrorResponse { Message = "Selected tier is not currently active" });
                }

                // Validate purchase amount
                if (request.Amount < tier.MinPurchase)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = $"Purchase amount is below minimum ({tier.MinPurchase} USDC)"
                    });
                }

                if (request.Amount > tier.MaxPurchase)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = $"Purchase amount is above maximum ({tier.MaxPurchase} USDC)"
                    });
                }

                // Check if tier has enough tokens left
                var tokensToReceive = request.Amount / tier.Price;
                if (tokensToReceive > (tier.Allocation - tier.Sold))
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message =
                            $"Not enough tokens left in this tier (Requested: {tokensToReceive}, Available: {tier.Allocation - tier.Sold})"
                    });
                }

                // Get user's current purchases if any
                var userPurchase = await _presaleService.GetUserPurchaseAsync(request.Address);

                // Create validation response
                return Ok(new PurchaseValidationModel
                {
                    IsValid = true,
                    Address = request.Address,
                    TierId = request.TierId,
                    Amount = request.Amount,
                    TokensToReceive = tokensToReceive,
                    Price = tier.Price,
                    ExistingPurchase = userPurchase?.Tokens ?? 0,
                    TierName = GetTierName(tier.Id),
                    VestingTGE = tier.VestingTGE,
                    VestingMonths = tier.VestingMonths
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validating purchase for address {request.Address}");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while validating the purchase",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        [EnableRateLimiting("Purchase")]
        [HttpPost("claim")]
        public async Task<ActionResult> ClaimTokens([FromBody] ClaimRequestModel request)
        {
            try
            {
                if (!_blockchainService.IsValidAddress(request.Address))
                {
                    return BadRequest(new ErrorResponse { Message = "Invalid Ethereum address" });
                }

                // Check if user has tokens to claim
                var claimableTokens = await _presaleService.GetClaimableTokensAsync(request.Address);

                if (claimableTokens <= 0)
                {
                    return BadRequest(new ErrorResponse { Message = "No tokens available to claim" });
                }

                // Process the claim
                var success = await _presaleService.ClaimTokensAsync(request.Address);

                if (!success)
                {
                    return StatusCode(500, new ErrorResponse { Message = "Failed to process token claim" });
                }

                // Get next vesting milestone for response
                var nextMilestone = await _presaleService.GetNextVestingMilestoneAsync(request.Address);

                return Ok(new ClaimResponseModel
                {
                    Address = request.Address,
                    TokensClaimed = claimableTokens,
                    TransactionTime = DateTime.UtcNow,
                    NextVestingAmount = nextMilestone?.Amount ?? 0,
                    NextVestingDate = nextMilestone?.Timestamp ?? DateTime.MaxValue
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error claiming tokens for address {request.Address}");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while claiming tokens",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        [HttpGet("claimable/{address}")]
        public async Task<ActionResult<decimal>> GetClaimableTokens([FromRoute] string address)
        {
            try
            {
                if (!_blockchainService.IsValidAddress(address))
                {
                    return BadRequest(new ErrorResponse { Message = "Invalid Ethereum address" });
                }

                // Get user purchase information
                var userPurchase = await _presaleService.GetUserPurchaseAsync(address);

                if (userPurchase == null || userPurchase.Tokens <= 0)
                {
                    return NotFound(new ErrorResponse { Message = $"No purchases found for address {address}" });
                }

                // Get claimable tokens
                var claimableTokens = await _presaleService.GetClaimableTokensAsync(address);

                // Get next vesting milestone
                var nextMilestone = await _presaleService.GetNextVestingMilestoneAsync(address);

                return Ok(new ClaimableTokensModel
                {
                    Address = address,
                    TotalTokens = userPurchase.Tokens,
                    ClaimableTokens = claimableTokens,
                    LastClaimTime = userPurchase.LastClaimTime,
                    NextVestingDate = nextMilestone?.Timestamp,
                    NextVestingAmount = nextMilestone?.Amount ?? 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting claimable tokens for address {address}");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while retrieving claimable tokens",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }
    }
}