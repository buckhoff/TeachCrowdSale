using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Data.Enum;
using TeachCrowdSale.Core.Interfaces.Repositories;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models;
using TeachCrowdSale.Core.Models.Vesting;

namespace TeachCrowdSale.Infrastructure.Services
{
    public class PresaleService : IPresaleService
    {
        private readonly ILogger<PresaleService> _logger;
        private readonly IBlockchainService _blockchainService;
        private readonly ITransactionRepository _transactionRepository;

        // Cache variables
        private List<SaleTier> _cachedTiers;
        private DateTime _lastTierCacheUpdate = DateTime.MinValue;
        private readonly TimeSpan _tierCacheExpiry = TimeSpan.FromMinutes(5);
        
        public PresaleService(
            ILogger<PresaleService> logger,
            IBlockchainService blockchainService,
            ITransactionRepository transactionRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _blockchainService = blockchainService ?? throw new ArgumentNullException(nameof(blockchainService));
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        }
        
        public async Task<SaleTier> GetCurrentTierAsync()
        {
            try
            {
                // Get contract addresses
                var contractAddresses = _blockchainService.GetContractAddresses();
                
                // Call the presale contract to get current tier ID
                var currentTierId = await _blockchainService.CallContractFunctionAsync<int>(
                    contractAddresses.PresaleAddress, 
                    "getCurrentTier()",
                    Array.Empty<object>());
                
                // Get all tiers to find the current one
                var tiers = await GetAllTiersAsync();
                
                // Return the current tier (or throw if not found)
                var currentTier = tiers.Find(t => t.Id == currentTierId);
                if (currentTier == null)
                {
                    throw new InvalidOperationException($"Tier with ID {currentTierId} not found");
                }
                
                return currentTier;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current tier");
                throw;
            }
        }
        
        public async Task<List<SaleTier>> GetAllTiersAsync()
        {
            try
            {
                // Check if we have a recent cache
                if (_cachedTiers != null && DateTime.UtcNow - _lastTierCacheUpdate < _tierCacheExpiry)
                {
                    return _cachedTiers;
                }
                
                // Get contract addresses
                var contractAddresses = _blockchainService.GetContractAddresses();
                
                // Get the number of tiers
                var tierCount = await _blockchainService.CallContractFunctionAsync<int>(
                    contractAddresses.PresaleAddress, 
                    "tierCount()",
                    Array.Empty<object>());
                
                // Get current tier to mark as active
                var currentTierId = await _blockchainService.CallContractFunctionAsync<int>(
                    contractAddresses.PresaleAddress, 
                    "getCurrentTier()",
                    Array.Empty<object>());
                
                var tiers = new List<SaleTier>();
                
                // Fetch each tier
                for (int i = 0; i < tierCount; i++)
                {
                    // Call the contract to get tier details
                    var tierResult = await _blockchainService.CallContractFunctionAsync<object[]>(
                        contractAddresses.PresaleAddress, 
                        "tiers(uint256)",
                        i);
                    
                    // Parse tier data from the result
                    var tier = new SaleTier
                    {
                        Id = i,
                        Price = ConvertFromWei((BigInteger)tierResult[0], 6), // Price in USDC (6 decimals)
                        Allocation = ConvertFromWei((BigInteger)tierResult[1], 18), // Allocation in TEACH (18 decimals)
                        Sold = ConvertFromWei((BigInteger)tierResult[2], 18), // Sold in TEACH (18 decimals)
                        MinPurchase = ConvertFromWei((BigInteger)tierResult[3], 6), // Min purchase in USDC
                        MaxPurchase = ConvertFromWei((BigInteger)tierResult[4], 6), // Max purchase in USDC
                        VestingTGE = Convert.ToInt32(tierResult[5]), // Percentage as integer
                        VestingMonths = Convert.ToInt32(tierResult[6]), // Months as integer
                        IsActive = i == currentTierId // Set active flag based on current tier
                    };
                    
                    tiers.Add(tier);
                }
                
                // Update cache
                _cachedTiers = tiers;
                _lastTierCacheUpdate = DateTime.UtcNow;
                
                return tiers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all tiers");
                throw;
            }
        }
        
        public async Task<DateTime> GetTierEndTimeAsync(int tierId)
        {
            try
            {
                // Get contract addresses
                var contractAddresses = _blockchainService.GetContractAddresses();
                
                // Call the contract to get tier deadline
                var deadlineTimestamp = await _blockchainService.CallContractFunctionAsync<BigInteger>(
                    contractAddresses.PresaleAddress, 
                    "tierDeadlines(uint256)",
                    tierId);
                
                // Convert timestamp to DateTime
                return DateTimeOffset.FromUnixTimeSeconds((long)deadlineTimestamp).DateTime;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting tier end time for tier {tierId}");
                
                // Fallback to default deadline (2 weeks from now)
                return DateTime.UtcNow.AddDays(14);
            }
        }
        
        public async Task<PresaleStats> GetPresaleStatsAsync()
        {
            try
            {
                // Get contract addresses
                var contractAddresses = _blockchainService.GetContractAddresses();
                
                // Get total tokens sold
                var tokensSold = await _blockchainService.CallContractFunctionAsync<BigInteger>(
                    contractAddresses.PresaleAddress, 
                    "totalTokensSold()",
                    Array.Empty<object>());
                
                // Get total raised (in USD)
                // This is a custom function that would need to be implemented in your contract
                var totalRaised = await _blockchainService.CallContractFunctionAsync<BigInteger>(
                    contractAddresses.PresaleAddress, 
                    "totalUsdRaised()",
                    Array.Empty<object>());
                
                // Get participants count
                var participantsCount = await _blockchainService.CallContractFunctionAsync<int>(
                    contractAddresses.PresaleAddress, 
                    "participantsCount()",
                    Array.Empty<object>());
                
                // Get presale start and end times
                var presaleStart = await _blockchainService.CallContractFunctionAsync<BigInteger>(
                    contractAddresses.PresaleAddress, 
                    "presaleStart()",
                    Array.Empty<object>());
                
                var presaleEnd = await _blockchainService.CallContractFunctionAsync<BigInteger>(
                    contractAddresses.PresaleAddress, 
                    "presaleEnd()",
                    Array.Empty<object>());
                
                // Get funding goal
                const decimal fundingGoal = 5_000_000M; // Hardcoded for now, could be fetched from contract
                
                // Calculate tokens remaining
                var totalAllocation = await _blockchainService.CallContractFunctionAsync<BigInteger>(
                    contractAddresses.TokenAddress, 
                    "MAX_SUPPLY()",
                    Array.Empty<object>());
                
                var tokensRemaining = ConvertFromWei(totalAllocation, 18) - ConvertFromWei(tokensSold, 18);
                
                return new PresaleStats
                {
                    TotalRaised = ConvertFromWei(totalRaised, 6), // USDC has 6 decimals
                    FundingGoal = fundingGoal,
                    ParticipantsCount = participantsCount,
                    TokensSold = ConvertFromWei(tokensSold, 18), // TEACH has 18 decimals
                    TokensRemaining = tokensRemaining,
                    PresaleStartTime = DateTimeOffset.FromUnixTimeSeconds((long)presaleStart).DateTime,
                    PresaleEndTime = DateTimeOffset.FromUnixTimeSeconds((long)presaleEnd).DateTime,
                    IsActive = DateTime.UtcNow >= DateTimeOffset.FromUnixTimeSeconds((long)presaleStart).DateTime &&
                               DateTime.UtcNow <= DateTimeOffset.FromUnixTimeSeconds((long)presaleEnd).DateTime
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting presale stats");
                throw;
            }
        }
        
        public async Task<UserPurchase> GetUserPurchaseAsync(string address)
        {
            try
            {
                // Get contract addresses
                var contractAddresses = _blockchainService.GetContractAddresses();
                
                // Call the contract to get user purchase info
                var purchaseResult = await _blockchainService.CallContractFunctionAsync<object[]>(
                    contractAddresses.PresaleAddress, 
                    "purchases(address)",
                    address);
                
                // Parse purchase data
                var tokens = ConvertFromWei((BigInteger)purchaseResult[0], 18); // TEACH has 18 decimals
                var usdAmount = ConvertFromWei((BigInteger)purchaseResult[1], 6); // USDC has 6 decimals
                
                // Get tier amounts
                var tierAmounts = new List<decimal>();
                
                // Get number of tiers
                var tierCount = await _blockchainService.CallContractFunctionAsync<int>(
                    contractAddresses.PresaleAddress, 
                    "tierCount()",
                    Array.Empty<object>());
                
                // Get last claim time (for vesting calculation)
                var lastClaimTime = await _blockchainService.CallContractFunctionAsync<BigInteger>(
                    contractAddresses.PresaleAddress, 
                    "lastClaimTime(address)",
                    address);
                
                // For each tier, get the amount purchased
                for (int i = 0; i < tierCount; i++)
                {
                    var tierAmount = await _blockchainService.CallContractFunctionAsync<BigInteger>(
                        contractAddresses.PresaleAddress, 
                        "getUserTierAmount(address,uint256)",
                        address, i);
                    
                    tierAmounts.Add(ConvertFromWei(tierAmount, 6)); // USDC has 6 decimals
                }
                
                return new UserPurchase
                {
                    Address = address,
                    Tokens = tokens,
                    UsdAmount = usdAmount,
                    TierAmounts = tierAmounts,
                    LastClaimTime = lastClaimTime > 0 ? 
                        (DateTime?)DateTimeOffset.FromUnixTimeSeconds((long)lastClaimTime).DateTime : 
                        null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting purchase info for address {address}");
                throw;
            }
        }
        
        public async Task<decimal> GetClaimableTokensAsync(string address)
        {
            try
            {
                // Get contract addresses
                var contractAddresses = _blockchainService.GetContractAddresses();
                
                // Call the contract to get claimable tokens
                var claimableTokens = await _blockchainService.CallContractFunctionAsync<BigInteger>(
                    contractAddresses.PresaleAddress, 
                    "claimableTokens(address)",
                    address);
                
                return ConvertFromWei(claimableTokens, 18); // TEACH has 18 decimals
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting claimable tokens for address {address}");
                throw;
            }
        }
        
        public async Task<VestingMilestoneModel> GetNextVestingMilestoneAsync(string address)
        {
            try
            {
                // Get contract addresses
                var contractAddresses = _blockchainService.GetContractAddresses();
                
                // Call the contract to get next vesting milestone
                var result = await _blockchainService.CallContractFunctionAsync<object[]>(
                    contractAddresses.PresaleAddress, 
                    "getNextVestingMilestone(address)",
                    address);
                
                var timestamp = (BigInteger)result[0];
                var amount = (BigInteger)result[1];
                
                if (timestamp == 0)
                {
                    return null; // No more vesting milestones
                }
                
                return new VestingMilestoneModel
                {
                    FormattedDate = DateTimeOffset.FromUnixTimeSeconds((long)timestamp).DateTime.ToLongDateString(),
                    Amount = ConvertFromWei(amount, 18) // TEACH has 18 decimals
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting next vesting milestone for address {address}");
                throw;
            }
        }
        
        public async Task<bool> PurchaseTokensAsync(string address, int tierId, decimal amount)
        {
            try
            {
                // Get contract addresses
                var contractAddresses = _blockchainService.GetContractAddresses();
                var tiers = await GetAllTiersAsync();
                var tier = tiers.FirstOrDefault(t => t.Id == tierId);
                if (tier == null)
                {
                    throw new InvalidOperationException($"Tier with ID {tierId} does not exist");
                }

                var purchase = new PurchaseTransaction
                {
                    WalletAddress = address.ToLowerInvariant(),
                    TierId = tierId,
                    UsdAmount = amount,
                    TokenAmount = amount / tier.Price,
                    TokenPrice = tier.Price,
                    CreatedAt = DateTime.UtcNow,
                    Status = TransactionStatus.Pending
                };

                await _transactionRepository.AddPurchaseAsync(purchase);

                // Convert amount to USDC units (6 decimals)
                var amountInUnits = ConvertToWei(amount, 6);
                
                // Execute purchase function
                var transactionHash = await _blockchainService.ExecuteContractFunctionAsync(
                    contractAddresses.PresaleAddress, 
                    "purchase(uint256,uint256)",
                    tierId, amountInUnits);

                purchase.TransactionHash = transactionHash;
                purchase.Status = !string.IsNullOrEmpty(transactionHash) ?
                    TransactionStatus.Confirmed : TransactionStatus.Failed;

                await _transactionRepository.UpdatePurchaseAsync(purchase);

                // Update user balance
                await UpdateUserBalanceAsync(address);

                return !string.IsNullOrEmpty(transactionHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error purchasing tokens for address {address}, tier {tierId}, amount {amount}");
                throw;
            }
        }
        
        public async Task<bool> ClaimTokensAsync(string address)
        {
            try
            {
                var claimableTokens = await GetClaimableTokensAsync(address);
                if (claimableTokens <= 0) return false;

                // Log claim as pending
                var claim = new ClaimTransaction
                {
                    WalletAddress = address.ToLowerInvariant(),
                    TokenAmount = claimableTokens,
                    CreatedAt = DateTime.UtcNow,
                    Status = TransactionStatus.Pending
                };

                await _transactionRepository.AddClaimAsync(claim);

                // Get contract addresses
                var contractAddresses = _blockchainService.GetContractAddresses();
                
                // Execute claim function
                var transactionHash = await _blockchainService.ExecuteContractFunctionAsync(
                    contractAddresses.PresaleAddress, 
                    "withdrawTokens()",
                    Array.Empty<object>());

                // Update claim with hash and status
                claim.TransactionHash = transactionHash;
                claim.Status = !string.IsNullOrEmpty(transactionHash) ?
                    TransactionStatus.Confirmed : TransactionStatus.Failed;

                await _transactionRepository.UpdateClaimAsync(claim);

                // Update user balance
                await UpdateUserBalanceAsync(address);


                return !string.IsNullOrEmpty(transactionHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error claiming tokens for address {address}");
                throw;
            }
        }
        
        // Helper method to convert from wei to decimal
        private decimal ConvertFromWei(BigInteger wei, int decimals)
        {
            return (decimal)wei / (decimal)Math.Pow(10, decimals);
        }
        
        // Helper method to convert from decimal to wei
        private BigInteger ConvertToWei(decimal amount, int decimals)
        {
            return new BigInteger(amount * (decimal)Math.Pow(10, decimals));
        }

        private async Task UpdateUserBalanceAsync(string address)
        {
            var userBalance = await _transactionRepository.GetUserBalanceAsync(address)
                ?? new UserBalance { WalletAddress = address.ToLowerInvariant() };

            var purchases = await _transactionRepository.GetUserPurchasesAsync(address);
            var claims = await _transactionRepository.GetUserClaimsAsync(address);

            userBalance.TotalPurchased = purchases
                .Where(p => p.Status == TransactionStatus.Confirmed)
                .Sum(p => p.TokenAmount);

            userBalance.TotalClaimed = claims
                .Where(c => c.Status == TransactionStatus.Confirmed)
                .Sum(c => c.TokenAmount);

            userBalance.PendingTokens = userBalance.TotalPurchased - userBalance.TotalClaimed;
            userBalance.LastUpdated = DateTime.UtcNow;

            await _transactionRepository.UpdateUserBalanceAsync(userBalance);
        }
    }
}