using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeachCrowdSale.Api.Models;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Models;

namespace TeachCrowdSale.Api.Controllers
{
    [ApiController]
    [Route("api/presale")]
    public class PresaleController : ControllerBase
    {
        private readonly IBlockchainService _blockchainService;
        private readonly IPresaleService _presaleService;
        
        public PresaleController(
            IBlockchainService blockchainService,
            IPresaleService presaleService)
        {
            _blockchainService = blockchainService ?? throw new ArgumentNullException(nameof(blockchainService));
            _presaleService = presaleService ?? throw new ArgumentNullException(nameof(presaleService));
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
        public async Task<ActionResult<UserPurchaseModel>> GetUserPurchases(string address)
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
        public async Task<ActionResult<VestingMilestoneModel>> GetNextVestingMilestone(string address)
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
    }
}