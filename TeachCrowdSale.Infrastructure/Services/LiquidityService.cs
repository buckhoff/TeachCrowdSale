// TeachCrowdSale.Infrastructure/Services/LiquidityService.cs
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Interfaces.Repositories;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models.Liquidity;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Models.Response;
using Task = System.Threading.Tasks.Task;
using TeachCrowdSale.Core.Models;

namespace TeachCrowdSale.Infrastructure.Services
{
    /// <summary>
    /// Service implementation for liquidity operations
    /// </summary>
    public class LiquidityService : ILiquidityService
    {
        private readonly ILogger<LiquidityService> _logger;
        private readonly ILiquidityRepository _liquidityRepository;
        private readonly IDexIntegrationService _dexIntegrationService;
        private readonly IBlockchainService _blockchainService;
        private readonly IMemoryCache _cache;

        // Cache keys and durations
        private const string CACHE_KEY_POOLS = "liquidity_pools";
        private const string CACHE_KEY_STATS = "liquidity_stats";
        private const string CACHE_KEY_DEX_CONFIG = "dex_configurations";
        private static readonly TimeSpan ShortCacheDuration = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan MediumCacheDuration = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan LongCacheDuration = TimeSpan.FromHours(1);

        public LiquidityService(
            ILogger<LiquidityService> logger,
            ILiquidityRepository liquidityRepository,
            IDexIntegrationService dexIntegrationService,
            IBlockchainService blockchainService,
            IMemoryCache cache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _liquidityRepository = liquidityRepository ?? throw new ArgumentNullException(nameof(liquidityRepository));
            _dexIntegrationService = dexIntegrationService ?? throw new ArgumentNullException(nameof(dexIntegrationService));
            _blockchainService = blockchainService ?? throw new ArgumentNullException(nameof(blockchainService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        #region Pool Management

        public async Task<List<LiquidityPoolResponse>> GetActiveLiquidityPoolsAsync()
        {
            try
            {
                return await _cache.GetOrCreateAsync(CACHE_KEY_POOLS, async entry =>
                {
                    entry.SetAbsoluteExpiration(MediumCacheDuration);

                    var pools = await _liquidityRepository.GetActiveLiquidityPoolsAsync();
                    return pools.Select(MapToLiquidityPoolResponse).ToList();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active liquidity pools");
                throw;
            }
        }

        public async Task<LiquidityPool?> GetLiquidityPoolAsync(int poolId)
        {
            try
            {
                return await _cache.GetOrCreateAsync($"pool_{poolId}", async entry =>
                {
                    entry.SetAbsoluteExpiration(MediumCacheDuration);
                    return await _liquidityRepository.GetLiquidityPoolByIdAsync(poolId);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving liquidity pool {PoolId}", poolId);
                throw;
            }
        }

        public async Task<LiquidityStatsResponse> GetLiquidityStatsAsync()
        {
            try
            {
                return await _cache.GetOrCreateAsync(CACHE_KEY_STATS, async entry =>
                {
                    entry.SetAbsoluteExpiration(ShortCacheDuration);

                    var totalTvl = await _liquidityRepository.GetTotalValueLockedAsync();
                    var totalVolume = await _liquidityRepository.GetTotal24hVolumeAsync();
                    var totalFees = await _liquidityRepository.GetTotalFeesEarnedAsync();  // FIXED: Method exists now
                    var activePools = await _liquidityRepository.GetActivePoolsCountAsync();  // FIXED: Added method
                    var totalProviders = await _liquidityRepository.GetTotalLiquidityProvidersAsync();  // FIXED: Added method
                    var avgApy = await _liquidityRepository.GetAverageAPYAsync();  // FIXED: Added method

                    // Get TEACH price from blockchain service or fallback
                    var teachPrice = 0.065m; // Fallback price
                    try
                    {
                        teachPrice = await GetTeachTokenPriceAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not fetch TEACH price, using fallback");
                    }

                    return new LiquidityStatsResponse
                    {
                        TotalValueLocked = totalTvl,
                        TotalVolume24h = totalVolume,
                        TotalFeesEarned = totalFees,
                        ActivePools = activePools,
                        TotalLiquidityProviders = totalProviders,
                        AverageAPY = avgApy,
                        TeachPrice = teachPrice,
                        PriceChangeDisplay = "0%", // Calculate actual change if historical data available
                        PriceChangeClass = "neutral",
                        LastUpdated = DateTime.UtcNow
                    };
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving liquidity statistics");
                throw;
            }
        }


        public async Task<List<DexConfigurationResponse>> GetDexConfigurationsAsync()
        {
            try
            {
                return await _cache.GetOrCreateAsync(CACHE_KEY_DEX_CONFIG, async entry =>
                {
                    entry.SetAbsoluteExpiration(LongCacheDuration);

                    var dexConfigs = await _liquidityRepository.GetActiveDexConfigurationsAsync();
                    return dexConfigs.Select(MapToDexConfigurationResponse).ToList();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving DEX configurations");
                throw;
            }
        }

        #endregion

        #region User Position Management

        public async Task<List<UserLiquidityPositionResponse>> GetUserLiquidityPositionsAsync(string walletAddress)
        {
            try
            {
                if (!_blockchainService.IsValidAddress(walletAddress))
                {
                    throw new ArgumentException("Invalid wallet address", nameof(walletAddress));
                }

                var positions = await _liquidityRepository.GetUserLiquidityPositionsAsync(walletAddress);
                var positionResponses = new List<UserLiquidityPositionResponse>();

                foreach (var position in positions)
                {
                    var response = await MapToUserLiquidityPositionResponseAsync(position);
                    positionResponses.Add(response);
                }

                return positionResponses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user liquidity positions for {WalletAddress}", walletAddress);
                throw;
            }
        }

        public async Task<UserLiquidityPositionResponse?> GetUserLiquidityPositionAsync(int positionId)
        {
            try
            {
                var position = await _liquidityRepository.GetUserLiquidityPositionByIdAsync(positionId);
                if (position == null) return null;

                return await MapToUserLiquidityPositionResponseAsync(position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user liquidity position {PositionId}", positionId);
                throw;
            }
        }

        public async Task<decimal> GetUserTotalLiquidityValueAsync(string walletAddress)
        {
            try
            {
                if (!_blockchainService.IsValidAddress(walletAddress))
                {
                    throw new ArgumentException("Invalid wallet address", nameof(walletAddress));
                }

                return await _liquidityRepository.GetUserTotalLiquidityValueAsync(walletAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating user total liquidity value for {WalletAddress}", walletAddress);
                throw;
            }
        }

        #endregion

        #region Liquidity Calculations

        public async Task<LiquidityCalculationResponse> CalculateLiquidityPreviewAsync(
     string walletAddress,
     int poolId,
     decimal token0Amount,
     decimal? token1Amount = null,
     decimal slippageTolerance = 0.5m)
        {
            try
            {
                // Input validation
                if (!_blockchainService.IsValidAddress(walletAddress))
                {
                    throw new ArgumentException("Invalid wallet address", nameof(walletAddress));
                }

                if (token0Amount <= 0)
                {
                    throw new ArgumentException("Token0 amount must be positive", nameof(token0Amount));
                }

                if (slippageTolerance < 0.1m || slippageTolerance > 50m)
                {
                    throw new ArgumentException("Slippage tolerance must be between 0.1% and 50%", nameof(slippageTolerance));
                }

                // Get pool data
                var poolResponse = await GetLiquidityPoolAsync(poolId);
                if (poolResponse == null)
                {
                    throw new InvalidOperationException($"Liquidity pool {poolId} not found");
                }

                // Initialize calculation response
                var calculation = new LiquidityCalculationResponse
                {
                    PoolId = poolId,
                    TokenPair = poolResponse.TokenPair,
                    Token0Symbol = poolResponse.Token0Symbol,
                    Token1Symbol = poolResponse.Token1Symbol,
                    WalletAddress = walletAddress,
                    Token0Amount = token0Amount,
                    SlippageTolerance = slippageTolerance,
                    CalculatedAt = DateTime.UtcNow
                };

                // FIXED: Get the actual entity for DEX operations (need pool address)
                var poolEntity = await _liquidityRepository.GetLiquidityPoolByIdAsync(poolId);
                if (poolEntity == null)
                {
                    throw new InvalidOperationException($"Pool entity {poolId} not found");
                }

                // Calculate optimal token1 amount if not provided
                if (!token1Amount.HasValue || token1Amount.Value <= 0)
                {
                    try
                    {
                        // FIXED: Pass proper parameters - removed decimal.MaxValue error
                        var (optimalToken0, optimalToken1) = await _dexIntegrationService
                            .CalculateOptimalLiquidityAmountsAsync(poolEntity.PoolAddress, token0Amount, 0);

                        calculation.Token1Amount = optimalToken1;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not calculate optimal token1 amount, using pool ratio");

                        // Fallback: Use current pool ratio
                        if (poolEntity.Token0Reserve > 0 && poolEntity.Token1Reserve > 0)
                        {
                            var ratio = poolEntity.Token1Reserve / poolEntity.Token0Reserve;
                            calculation.Token1Amount = token0Amount * ratio;
                        }
                        else
                        {
                            calculation.Token1Amount = token0Amount; // 1:1 ratio as final fallback
                        }
                    }
                }
                else
                {
                    calculation.Token1Amount = token1Amount.Value;
                }

                // FIXED: Set current price properly
                calculation.CurrentPrice = poolEntity.CurrentPrice;

                // Calculate minimum amounts with slippage protection
                try
                {
                    var (token0Min, token1Min) = await _dexIntegrationService
                        .CalculateMinimumAmountsAsync(poolEntity.PoolAddress,
                            calculation.Token0Amount, calculation.Token1Amount, slippageTolerance);

                    calculation.Token0AmountMin = token0Min;
                    calculation.Token1AmountMin = token1Min;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not calculate minimum amounts, using slippage calculation");

                    // Fallback calculation
                    var slippageMultiplier = (100 - slippageTolerance) / 100;
                    calculation.Token0AmountMin = calculation.Token0Amount * slippageMultiplier;
                    calculation.Token1AmountMin = calculation.Token1Amount * slippageMultiplier;
                }

                // Estimate LP tokens
                try
                {
                    calculation.EstimatedLpTokens = await _dexIntegrationService
                        .EstimateLpTokensForAmountsAsync(poolEntity.PoolAddress,
                            calculation.Token0Amount, calculation.Token1Amount);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not estimate LP tokens, using fallback calculation");

                    // Fallback: Simple proportion based on pool reserves
                    if (poolEntity.Token0Reserve > 0 && poolEntity.Token1Reserve > 0)
                    {
                        var token0Share = calculation.Token0Amount / poolEntity.Token0Reserve;
                        var token1Share = calculation.Token1Amount / poolEntity.Token1Reserve;
                        var avgShare = (token0Share + token1Share) / 2;

                        // Estimate based on total supply (would need from contract)
                        calculation.EstimatedLpTokens = avgShare * 1000000; // Placeholder total supply
                    }
                    else
                    {
                        calculation.EstimatedLpTokens = Math.Sqrt(calculation.Token0Amount * calculation.Token1Amount);
                    }
                }

                // Calculate total value in USD
                try
                {
                    var token0Price = await _dexIntegrationService.GetTokenPriceAsync(poolEntity.Token0Address);
                    var token1Price = await _dexIntegrationService.GetTokenPriceAsync(poolEntity.Token1Address);

                    calculation.EstimatedValueUsd = (calculation.Token0Amount * token0Price) +
                                                  (calculation.Token1Amount * token1Price);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not get token prices, using pool data");

                    // Fallback: Use current pool price and assume token1 is stablecoin
                    calculation.EstimatedValueUsd = (calculation.Token0Amount * poolEntity.CurrentPrice) +
                                                  calculation.Token1Amount;
                }

                // Calculate price impact
                try
                {
                    calculation.PriceImpact = await _dexIntegrationService
                        .CalculatePriceImpactAsync(poolEntity.PoolAddress,
                            calculation.Token0Amount, calculation.Token1Amount);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not calculate price impact, using estimation");

                    // Simple price impact estimation based on pool size
                    var poolTotalValue = (poolEntity.Token0Reserve * poolEntity.CurrentPrice) + poolEntity.Token1Reserve;
                    calculation.PriceImpact = poolTotalValue > 0 ? (calculation.EstimatedValueUsd / poolTotalValue) * 100 : 0;
                }

                // FIXED: Use correct property names and calculations
                calculation.EstimatedAPY = poolEntity.APY;

                // Calculate estimated earnings (FIXED: correct property names)
                if (calculation.EstimatedValueUsd > 0 && calculation.EstimatedAPY > 0)
                {
                    calculation.EstimatedDailyFees = calculation.EstimatedValueUsd * (calculation.EstimatedAPY / 100) / 365;
                    calculation.EstimatedMonthlyFees = calculation.EstimatedDailyFees * 30;
                    calculation.EstimatedYearlyFees = calculation.EstimatedDailyFees * 365;
                }

                // Calculate pool share
                try
                {
                    var poolTvl = await _dexIntegrationService.GetPoolTotalValueLockedAsync(poolEntity.PoolAddress);
                    calculation.PoolShare = poolTvl > 0 ? (calculation.EstimatedValueUsd / poolTvl) * 100 : 0;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not get pool TVL, using pool data");
                    calculation.PoolShare = poolEntity.TotalValueLocked > 0 ?
                        (calculation.EstimatedValueUsd / poolEntity.TotalValueLocked) * 100 : 0;
                }

                // FIXED: Gas estimation (placeholder - would integrate with gas oracle)
                calculation.GasEstimate = 0.005m; // ~$5 estimated gas cost

                // Validation checks
                try
                {
                    calculation.HasSufficientBalance = await _dexIntegrationService
                        .SimulateAddLiquidityAsync(walletAddress, poolEntity.PoolAddress,
                            calculation.Token0Amount, calculation.Token1Amount);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not simulate transaction, assuming insufficient balance");
                    calculation.HasSufficientBalance = false;
                }

                // FIXED: Add missing validation properties
                calculation.HasSufficientAllowance = true; // Would check actual allowances
                calculation.IsWithinSlippage = calculation.PriceImpact <= 5; // 5% max recommended
                calculation.IsValid = calculation.HasSufficientBalance &&
                                     calculation.IsWithinSlippage &&
                                     calculation.EstimatedValueUsd > 0;

                // Risk assessment
                calculation.RiskLevel = CalculateRiskLevel(calculation.PriceImpact, calculation.EstimatedAPY);
                calculation.ImpermanentLossEstimate = EstimateImpermanentLoss(calculation.PriceImpact);

                // FIXED: Formatted display properties
                calculation.TotalValueDisplay = FormatCurrency(calculation.EstimatedValueUsd);
                calculation.ApyDisplay = $"{calculation.EstimatedAPY:F2}%";
                calculation.DailyFeesDisplay = FormatCurrency(calculation.EstimatedDailyFees);
                calculation.MonthlyFeesDisplay = FormatCurrency(calculation.EstimatedMonthlyFees);
                calculation.PriceImpactDisplay = $"{calculation.PriceImpact:F2}%";

                // Validation and warning messages
                if (!calculation.HasSufficientBalance)
                {
                    calculation.ValidationMessages.Add("Insufficient token balance for this transaction");
                }

                if (!calculation.HasSufficientAllowance)
                {
                    calculation.ValidationMessages.Add("Insufficient token allowance - approval required");
                }

                if (calculation.PriceImpact > 1)
                {
                    calculation.WarningMessages.Add($"Price impact is {calculation.PriceImpact:F2}% - consider smaller amounts");
                }

                if (calculation.PriceImpact > 5)
                {
                    calculation.WarningMessages.Add("High price impact detected - transaction may fail");
                }

                if (calculation.EstimatedAPY > 100)
                {
                    calculation.WarningMessages.Add("Extremely high APY detected - verify pool legitimacy");
                }

                if (calculation.EstimatedValueUsd < 10)
                {
                    calculation.WarningMessages.Add("Small liquidity amount - gas costs may exceed profits");
                }

                return calculation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating liquidity preview for pool {PoolId}, wallet {WalletAddress}",
                    poolId, walletAddress);
                throw;
            }
        }

        public async Task<LiquidityCalculationResponse> CalculateRemoveLiquidityPreviewAsync(string walletAddress, int positionId, decimal percentageToRemove)
        {
            try
            {
                var position = await _liquidityRepository.GetUserLiquidityPositionByIdAsync(positionId);
                if (position == null || position.WalletAddress != walletAddress.ToLowerInvariant())
                {
                    throw new InvalidOperationException("Position not found or unauthorized");
                }

                var lpTokensToRemove = position.LpTokenAmount * (percentageToRemove / 100);
                var (token0Amount, token1Amount) = await _dexIntegrationService.EstimateAmountsForLpTokensAsync(
                    position.LiquidityPool.PoolAddress, lpTokensToRemove);

                var calculation = new LiquidityCalculationResponse
                {
                    PoolId = position.LiquidityPoolId,
                    TokenPair = position.LiquidityPool.TokenPair,
                    Token0Symbol = position.LiquidityPool.Token0Symbol,
                    Token1Symbol = position.LiquidityPool.Token1Symbol,
                    Token0Amount = token0Amount,
                    Token1Amount = token1Amount,
                    EstimatedLpTokens = lpTokensToRemove
                };

                // Calculate value
                var token0Price = await _dexIntegrationService.GetTokenPriceAsync(position.LiquidityPool.Token0Address);
                var token1Price = await _dexIntegrationService.GetTokenPriceAsync(position.LiquidityPool.Token1Address);
                calculation.EstimatedValueUsd = (token0Amount * token0Price) + (token1Amount * token1Price);

                calculation.IsValid = true;
                calculation.TotalValueDisplay = FormatCurrency(calculation.EstimatedValueUsd);

                return calculation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating remove liquidity preview for position {PositionId}", positionId);
                throw;
            }
        }

        #endregion

        #region Liquidity Operations

        public async Task<bool> AddLiquidityAsync(string walletAddress, int poolId, decimal token0Amount, decimal token1Amount, decimal token0AmountMin, decimal token1AmountMin)
        {
            try
            {
                var pool = await GetLiquidityPoolAsync(poolId);
                if (pool == null)
                {
                    throw new InvalidOperationException($"Pool {poolId} not found");
                }

                // This would integrate with the actual DEX router contract
                // For now, we'll simulate the transaction and create a database record

                var lpTokens = await _dexIntegrationService.EstimateLpTokensForAmountsAsync(
                    pool.PoolAddress, token0Amount, token1Amount);

                var token0Price = await _dexIntegrationService.GetTokenPriceAsync(pool.Token0Address);
                var token1Price = await _dexIntegrationService.GetTokenPriceAsync(pool.Token1Address);
                var totalValueUsd = (token0Amount * token0Price) + (token1Amount * token1Price);

                // Create position record
                var position = new UserLiquidityPosition
                {
                    WalletAddress = walletAddress.ToLowerInvariant(),
                    LiquidityPoolId = poolId,
                    LpTokenAmount = lpTokens,
                    Token0Amount = token0Amount,
                    Token1Amount = token1Amount,
                    InitialValueUsd = totalValueUsd,
                    CurrentValueUsd = totalValueUsd,
                    AddTransactionHash = "0x..." // Would be actual transaction hash
                };

                await _liquidityRepository.CreateUserLiquidityPositionAsync(position);

                // Create transaction record
                var transaction = new LiquidityTransaction
                {
                    UserLiquidityPositionId = position.Id,
                    WalletAddress = walletAddress.ToLowerInvariant(),
                    TransactionType = "ADD",
                    Token0Amount = token0Amount,
                    Token1Amount = token1Amount,
                    LpTokenAmount = lpTokens,
                    ValueUsd = totalValueUsd,
                    TransactionHash = "0x..." // Would be actual transaction hash
                };

                await _liquidityRepository.CreateLiquidityTransactionAsync(transaction);

                // Clear relevant caches
                _cache.Remove($"{CACHE_KEY_POOLS}_active");
                _cache.Remove(CACHE_KEY_STATS);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding liquidity for {WalletAddress}", walletAddress);
                throw;
            }
        }

        public async Task<LiquidityPageDataResponse> GetLiquidityPageDataAsync()
        {
            try
            {
                var data = new LiquidityPageDataResponse
                {
                    LiquidityPools = await GetActiveLiquidityPoolsAsync(),
                    DexOptions = await GetDexConfigurationsAsync(),
                    Stats = await GetLiquidityStatsAsync(),
                    Analytics = await GetLiquidityAnalyticsAsync(),
                    GuideSteps = await GetLiquidityGuideStepsAsync(),
                    LoadedAt = DateTime.UtcNow
                };

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving liquidity page data");
                throw;
            }
        }
        public async Task<bool> RemoveLiquidityAsync(string walletAddress, int positionId, decimal percentageToRemove)
        {
            try
            {
                var position = await _liquidityRepository.GetUserLiquidityPositionByIdAsync(positionId);
                if (position == null || position.WalletAddress != walletAddress.ToLowerInvariant())
                {
                    throw new InvalidOperationException("Position not found or unauthorized");
                }

                var lpTokensToRemove = position.LpTokenAmount * (percentageToRemove / 100);
                var (token0Amount, token1Amount) = await _dexIntegrationService.EstimateAmountsForLpTokensAsync(
                    position.LiquidityPool.PoolAddress, lpTokensToRemove);

                // Update position
                position.LpTokenAmount -= lpTokensToRemove;
                position.Token0Amount -= token0Amount;
                position.Token1Amount -= token1Amount;

                if (percentageToRemove >= 100)
                {
                    position.IsActive = false;
                    position.RemovedAt = DateTime.UtcNow;
                }

                await _liquidityRepository.UpdateUserLiquidityPositionAsync(position);

                // Create transaction record
                var valueUsd = await CalculateValueUsd(token0Amount, token1Amount, position.LiquidityPool);
                var transaction = new LiquidityTransaction
                {
                    UserLiquidityPositionId = positionId,
                    WalletAddress = walletAddress.ToLowerInvariant(),
                    TransactionType = "REMOVE",
                    Token0Amount = token0Amount,
                    Token1Amount = token1Amount,
                    LpTokenAmount = lpTokensToRemove,
                    ValueUsd = valueUsd,
                    TransactionHash = "0x..." // Would be actual transaction hash
                };

                await _liquidityRepository.CreateLiquidityTransactionAsync(transaction);

                // Clear caches
                _cache.Remove($"{CACHE_KEY_POOLS}_active");
                _cache.Remove(CACHE_KEY_STATS);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing liquidity for position {PositionId}", positionId);
                throw;
            }
        }

        public async Task<bool> ClaimFeesAsync(string walletAddress, int positionId)
        {
            try
            {
                var position = await _liquidityRepository.GetUserLiquidityPositionByIdAsync(positionId);
                if (position == null || position.WalletAddress != walletAddress.ToLowerInvariant())
                {
                    throw new InvalidOperationException("Position not found or unauthorized");
                }

                // This would integrate with DEX contract to claim fees
                // For now, simulate the operation
                var feesAmount = position.FeesEarnedUsd * 0.1m; // Claim 10% of accrued fees

                var transaction = new LiquidityTransaction
                {
                    UserLiquidityPositionId = positionId,
                    WalletAddress = walletAddress.ToLowerInvariant(),
                    TransactionType = "CLAIM_FEES",
                    ValueUsd = feesAmount,
                    TransactionHash = "0x..." // Would be actual transaction hash
                };

                await _liquidityRepository.CreateLiquidityTransactionAsync(transaction);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error claiming fees for position {PositionId}", positionId);
                throw;
            }
        }

        #endregion

        #region Pool Data Synchronization

        public async Task SyncPoolDataAsync(int poolId)
        {
            try
            {
                var pool = await _liquidityRepository.GetLiquidityPoolByIdAsync(poolId);
                if (pool == null) return;

                // Update pool data from DEX
                var tvl = await _dexIntegrationService.GetPoolTotalValueLockedAsync(pool.PoolAddress);
                var volume24h = await _dexIntegrationService.GetPool24hVolumeAsync(pool.PoolAddress);
                var apy = await _dexIntegrationService.CalculatePoolAPYAsync(pool.PoolAddress);
                var (token0Reserve, token1Reserve, _) = await _dexIntegrationService.GetPoolReservesAsync(pool.PoolAddress);

                pool.TotalValueLocked = tvl;
                pool.Volume24h = volume24h;
                pool.APY = apy;
                pool.Token0Reserve = token0Reserve;
                pool.Token1Reserve = token1Reserve;
                pool.CurrentPrice = token1Reserve > 0 ? token0Reserve / token1Reserve : 0;

                await _liquidityRepository.UpdateLiquidityPoolAsync(pool);

                // Create snapshot
                var snapshot = new LiquidityPoolSnapshot
                {
                    LiquidityPoolId = poolId,
                    TotalValueLocked = tvl,
                    Volume24h = volume24h,
                    Token0Reserve = token0Reserve,
                    Token1Reserve = token1Reserve,
                    Price = pool.CurrentPrice,
                    APY = apy,
                    IsLatest = true
                };

                await _liquidityRepository.CreatePoolSnapshotAsync(snapshot);
                await _liquidityRepository.UpdateLatestSnapshotFlagsAsync(poolId);

                // Clear caches
                _cache.Remove($"pool_{poolId}");
                _cache.Remove($"{CACHE_KEY_POOLS}_active");
                _cache.Remove(CACHE_KEY_STATS);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing pool data for pool {PoolId}", poolId);
            }
        }

        public async Task SyncAllPoolsDataAsync()
        {
            try
            {
                var pools = await _liquidityRepository.GetActiveLiquidityPoolsAsync();
                var tasks = pools.Select(p => SyncPoolDataAsync(p.Id));
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing all pools data");
            }
        }

        public async Task<bool> RefreshPoolPricesAsync()
        {
            try
            {
                var pools = await _liquidityRepository.GetActiveLiquidityPoolsAsync();
                foreach (var pool in pools)
                {
                    var price = await _dexIntegrationService.GetTokenPriceAsync(pool.Token0Address);
                    pool.CurrentPrice = price;
                    await _liquidityRepository.UpdateLiquidityPoolAsync(pool);
                }

                // Clear caches
                _cache.Remove($"{CACHE_KEY_POOLS}_active");
                _cache.Remove(CACHE_KEY_STATS);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing pool prices");
                return false;
            }
        }

        #endregion

        #region Analytics

        public async Task<LiquidityAnalyticsResponse> GetLiquidityAnalyticsAsync()
        {
            try
            {
                return await _cache.GetOrCreateAsync("liquidity_analytics", async entry =>
                {
                    entry.SetAbsoluteExpiration(MediumCacheDuration);

                    var analytics = new LiquidityAnalyticsResponse
                    {
                        TvlTrends = await _liquidityRepository.GetTvlTrendsAsync(30),
                        VolumeTrends = await _liquidityRepository.GetVolumeTrendsAsync(30),
                        PoolPerformance = await _liquidityRepository.GetPoolPerformanceDataAsync(),
                        TopProviders = await _liquidityRepository.GetTopLiquidityProvidersAsync(10),
                        DexComparison = await GetDexComparisonDataAsync(),
                        Overview = await GetLiquidityStatsAsync()
                    };

                    return analytics;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving liquidity analytics");
                throw;
            }
        }

        public async Task<List<UserLiquidityStatsResponse>> GetTopLiquidityProvidersAsync(int limit = 10)
        {
            try
            {
                var allPositions = await _liquidityRepository.GetUserLiquidityPositionsAsync(string.Empty, true);

                var userStats = allPositions
                    .GroupBy(p => p.WalletAddress)
                    .Select(g => new UserLiquidityStatsResponse
                    {
                        WalletAddress = g.Key,
                        DisplayAddress = $"{g.Key[..6]}...{g.Key[^4..]}",  // FIXED: Added DisplayAddress
                        TotalLiquidityValue = g.Sum(p => p.CurrentValueUsd),
                        TotalValueProvided = g.Sum(p => p.InitialValueUsd),  // FIXED: Added TotalValueProvided
                        TotalFeesEarned = g.Sum(p => p.FeesEarnedUsd),
                        TotalPnL = g.Sum(p => p.NetPnL),
                        PnLPercentage = g.Sum(p => p.InitialValueUsd) > 0 ?
                            (g.Sum(p => p.NetPnL) / g.Sum(p => p.InitialValueUsd)) * 100 : 0,
                        ActivePositions = g.Count(p => p.IsActive),
                        FirstPositionDate = g.Min(p => p.AddedAt),
                        TimeActive = DateTime.UtcNow - g.Min(p => p.AddedAt)
                    })
                    .OrderByDescending(s => s.TotalLiquidityValue)
                    .Take(limit)
                    .ToList();

                // Add ranking
                for (int i = 0; i < userStats.Count; i++)
                {
                    userStats[i].Rank = i + 1;
                }

                return userStats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top liquidity providers");
                return new List<UserLiquidityStatsResponse>();
            }
        }

        public async Task<List<PoolPerformanceDataResponse>> GetPoolPerformanceAsync()
        {
            try
            {
                return await _liquidityRepository.GetPoolPerformanceDataAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pool performance data");
                throw;
            }
        }

        public async Task<List<LiquidityTrendDataResponse>> GetTvlTrendsAsync(int days = 30)
        {
            try
            {
                return await _liquidityRepository.GetTvlTrendsAsync(days);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving TVL trends");
                throw;
            }
        }

        public async Task<List<VolumeTrendDataResponse>> GetVolumeTrendsAsync(int days = 30)
        {
            try
            {
                return await _liquidityRepository.GetVolumeTrendsAsync(days);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving volume trends");
                throw;
            }
        }

        #endregion

        #region DEX Integration

        public async Task<decimal> GetTokenPriceFromDexAsync(string tokenAddress, string dexName)
        {
            try
            {
                return await _dexIntegrationService.GetTokenPriceAsync(tokenAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token price from DEX");
                throw;
            }
        }

        public async Task<(decimal token0Reserve, decimal token1Reserve)> GetPoolReservesAsync(string poolAddress, string dexName)
        {
            try
            {
                var (token0Reserve, token1Reserve, _) = await _dexIntegrationService.GetPoolReservesAsync(poolAddress);
                return (token0Reserve, token1Reserve);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pool reserves");
                throw;
            }
        }

        public async Task<decimal> GetPoolAPYAsync(int poolId)
        {
            try
            {
                var pool = await GetLiquidityPoolAsync(poolId);
                if (pool == null) return 0;

                return await _dexIntegrationService.CalculatePoolAPYAsync(pool.PoolAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pool APY for pool {PoolId}", poolId);
                throw;
            }
        }

        #endregion

        #region Guidance and Education

        public async Task<List<LiquidityGuideStepResponse>> GetLiquidityGuideStepsAsync(string? walletAddress = null)
        {
            try
            {
                var steps = new List<LiquidityGuideStepResponse>
                {
                    new()
                    {
                        StepNumber = 1,
                        Title = "Connect Your Wallet",
                        Description = "Connect your crypto wallet to interact with liquidity pools",
                        Icon = "🔗",
                        ActionText = "Connect Wallet",
                        ActionUrl = "/connect-wallet"
                    },
                    new()
                    {
                        StepNumber = 2,
                        Title = "Choose a Liquidity Pool",
                        Description = "Select a TEACH token pair that matches your investment strategy",
                        Icon = "💧",
                        ActionText = "Browse Pools",
                        ActionUrl = "/liquidity#pools"
                    },
                    new()
                    {
                        StepNumber = 3,
                        Title = "Calculate Your Position",
                        Description = "Use our calculator to estimate returns and required token amounts",
                        Icon = "🧮",
                        ActionText = "Open Calculator",
                        ActionUrl = "/liquidity#calculator"
                    },
                    new()
                    {
                        StepNumber = 4,
                        Title = "Add Liquidity",
                        Description = "Deposit your tokens into the pool and receive LP tokens",
                        Icon = "➕",
                        ActionText = "Add Liquidity",
                        ActionUrl = "/liquidity#add"
                    },
                    new()
                    {
                        StepNumber = 5,
                        Title = "Monitor Your Position",
                        Description = "Track your earnings and manage your liquidity position",
                        Icon = "📊",
                        ActionText = "View Dashboard",
                        ActionUrl = "/liquidity#dashboard"
                    }
                };

                // Mark steps as completed based on user's activity
                if (!string.IsNullOrEmpty(walletAddress))
                {
                    var userPositions = await GetUserLiquidityPositionsAsync(walletAddress);
                    if (userPositions.Any())
                    {
                        steps[0].IsCompleted = true; // Wallet connected
                        steps[1].IsCompleted = true; // Pool chosen
                        steps[2].IsCompleted = true; // Position calculated
                        steps[3].IsCompleted = true; // Liquidity added
                        steps[4].IsCompleted = userPositions.Any(p => p.IsActive); // Active position
                    }
                }

                return steps;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving liquidity guide steps");
                throw;
            }
        }

        public async Task<bool> MarkGuideStepCompletedAsync(string walletAddress, int stepNumber)
        {
            try
            {
                // This could be implemented to track user progress
                // For now, we'll just return true as steps are auto-detected
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking guide step completed");
                return false;
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Map LiquidityPool entity to Response model
        /// ARCHITECTURE FIX: Was mapping to web DisplayModel, now maps to Response
        /// </summary>
        private LiquidityPoolResponse MapToLiquidityPoolResponse(LiquidityPool pool)
        {
            return new LiquidityPoolResponse
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
        }

        /// <summary>
        /// Map UserLiquidityPosition entity to Response model
        /// ARCHITECTURE FIX: Was mapping to web Model, now maps to Response
        /// </summary>
        private async Task<UserLiquidityPositionResponse> MapToUserLiquidityPositionResponseAsync(UserLiquidityPosition position)
        {
            // Calculate current values (existing logic)
            var currentValue = await CalculateCurrentPositionValueAsync(position);
            var pnl = currentValue - position.InitialValueUsd;
            var pnlPercentage = position.InitialValueUsd > 0 ? (pnl / position.InitialValueUsd) * 100 : 0;

            return new UserLiquidityPositionResponse
            {
                Id = position.Id,
                PoolId = position.LiquidityPoolId,
                PoolName = position.LiquidityPool.Name,
                TokenPair = position.LiquidityPool.TokenPair,
                Token0Symbol = position.LiquidityPool.Token0Symbol,
                Token1Symbol = position.LiquidityPool.Token1Symbol,
                DexName = position.LiquidityPool.DexName,
                WalletAddress = position.WalletAddress,
                LpTokenAmount = position.LpTokenAmount,
                Token0Amount = position.Token0Amount,
                Token1Amount = position.Token1Amount,
                InitialValueUsd = position.InitialValueUsd,
                CurrentValueUsd = currentValue,
                FeesEarnedUsd = position.FeesEarnedUsd,
                ImpermanentLoss = position.ImpermanentLoss,
                NetPnL = pnl,
                PnLPercentage = pnlPercentage,
                IsActive = position.IsActive,
                AddedAt = position.AddedAt,
                LastUpdatedAt = position.LastUpdatedAt
            };
        }


        /// <summary>
        /// Map DexConfiguration entity to Response model
        /// ARCHITECTURE FIX: Was mapping to web Model, now maps to Response
        /// </summary>
        private DexConfigurationResponse MapToDexConfigurationResponse(DexConfiguration dex)
        {
            return new DexConfigurationResponse
            {
                Id = dex.Id,
                Name = dex.Name,
                DisplayName = dex.DisplayName,
                Description = dex.Description ?? string.Empty,
                LogoUrl = dex.LogoUrl,
                BaseUrl = dex.BaseUrl,
                ApiUrl = dex.ApiUrl,
                IsRecommended = dex.IsRecommended,
                IsActive = dex.IsActive,
                DefaultFeePercentage = dex.DefaultFeePercentage,
                Network = dex.Network,
                RouterAddress = dex.RouterAddress ?? string.Empty,
                FactoryAddress = dex.FactoryAddress ?? string.Empty,
                SortOrder = dex.SortOrder
            };
        }

        private async Task<decimal> GetTeachTokenPriceAsync()
        {
            try
            {
                return await _cache.GetOrCreateAsync("teach_price", async entry =>
                {
                    entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

                    // Get TEACH token address from configuration
                    var teachAddress = "0x..."; // Would be from configuration
                    return await _dexIntegrationService.GetTokenPriceAsync(teachAddress);
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting TEACH token price, using fallback");
                return 0.065m; // Fallback price
            }
        }

        public async Task<List<DexPerformanceResponse>> GetDexComparisonDataAsync()
        {
            try
            {
                var dexes = await _liquidityRepository.GetActiveDexConfigurationsAsync();
                var comparisons = new List<DexPerformanceResponse>();
                var totalMarketTvl = 0m;

                // First pass: calculate individual DEX metrics
                var dexMetrics = new List<(DexConfiguration dex, decimal tvl, decimal volume, decimal avgApy, int poolsCount)>();

                foreach (var dex in dexes)
                {
                    var pools = await _liquidityRepository.GetPoolsByDexAsync(dex.Name);
                    var tvl = pools.Sum(p => p.TotalValueLocked);
                    var volume = pools.Sum(p => p.Volume24h);
                    var avgApy = pools.Any() ? pools.Average(p => p.APY) : 0;
                    var poolsCount = pools.Count;

                    dexMetrics.Add((dex, tvl, volume, avgApy, poolsCount));
                    totalMarketTvl += tvl;
                }

                // Second pass: create response models with market share
                foreach (var (dex, tvl, volume, avgApy, poolsCount) in dexMetrics)
                {
                    var marketShare = totalMarketTvl > 0 ? (tvl / totalMarketTvl) * 100 : 0;

                    comparisons.Add(new DexPerformanceResponse
                    {
                        DexName = dex.DisplayName,
                        TotalValueLocked = tvl,
                        Volume24h = volume,
                        AverageAPY = avgApy,
                        PoolsCount = poolsCount,
                        LogoUrl = dex.LogoUrl ?? GetDexLogoUrl(dex.Name),
                        MarketShare = marketShare,
                        CalculatedAt = DateTime.UtcNow
                    });
                }

                return comparisons.OrderByDescending(d => d.TotalValueLocked).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting DEX comparison data");
                return new List<DexPerformanceResponse>();
            }
        }

        private async Task<decimal> CalculateValueUsd(decimal token0Amount, decimal token1Amount, LiquidityPool pool)
        {
            var token0Price = await _dexIntegrationService.GetTokenPriceAsync(pool.Token0Address);
            var token1Price = await _dexIntegrationService.GetTokenPriceAsync(pool.Token1Address);
            return (token0Amount * token0Price) + (token1Amount * token1Price);
        }

        private string CalculateRiskLevel(decimal priceImpact, decimal apy)
        {
            if (priceImpact > 10 || apy > 200) return "Very High";
            if (priceImpact > 5 || apy > 100) return "High";
            if (priceImpact > 2 || apy > 50) return "Medium";
            if (priceImpact > 1 || apy > 20) return "Low";
            return "Very Low";
        }

        private string EstimateImpermanentLoss(decimal priceImpact)
        {
            if (priceImpact > 10) return "Very High (>5%)";
            if (priceImpact > 5) return "High (2-5%)";
            if (priceImpact > 2) return "Medium (0.5-2%)";
            if (priceImpact > 0.5m) return "Low (0.1-0.5%)";
            return "Minimal (<0.1%)";
        }

        private string GetDexLogoUrl(string dexName)
        {
            return dexName.ToLower() switch
            {
                "quickswap" => "/images/dex/quickswap.png",
                "uniswap" => "/images/dex/uniswap.png",
                "sushiswap" => "/images/dex/sushiswap.png",
                _ => "/images/dex/default.png"
            };
        }
        public async Task<UserLiquidityInfoResponse> GetUserLiquidityInfoAsync(string walletAddress)
        {
            try
            {
                if (!_blockchainService.IsValidAddress(walletAddress))
                {
                    throw new ArgumentException("Invalid wallet address", nameof(walletAddress));
                }

                var positions = await GetUserLiquidityPositionsAsync(walletAddress);
                var transactions = await GetUserTransactionHistoryAsync(walletAddress, 1, 10);

                var totalValue = positions.Where(p => p.IsActive).Sum(p => p.CurrentValueUsd);
                var totalFees = positions.Sum(p => p.FeesEarnedUsd);
                var totalPnL = positions.Sum(p => p.NetPnL);
                var totalInitialValue = positions.Sum(p => p.InitialValueUsd);
                var pnlPercentage = totalInitialValue > 0 ? (totalPnL / totalInitialValue) * 100 : 0;

                var firstPosition = positions.OrderBy(p => p.AddedAt).FirstOrDefault();

                return new UserLiquidityInfoResponse
                {
                    WalletAddress = walletAddress,
                    TotalLiquidityValue = totalValue,
                    TotalFeesEarned = totalFees,
                    TotalPnL = totalPnL,
                    TotalPnLPercentage = pnlPercentage,
                    ActivePositions = positions.Count(p => p.IsActive),
                    TotalPositions = positions.Count,
                    FirstPositionDate = firstPosition?.AddedAt ?? DateTime.UtcNow,
                    Positions = positions,
                    RecentTransactions = transactions,
                    Stats = new UserLiquidityStatsResponse
                    {
                        WalletAddress = walletAddress,
                        DisplayAddress = $"{walletAddress[..6]}...{walletAddress[^4..]}",
                        TotalValueProvided = totalValue,
                        TotalFeesEarned = totalFees,
                        ActivePositions = positions.Count(p => p.IsActive),
                        FirstPositionDate = firstPosition?.AddedAt ?? DateTime.UtcNow
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user liquidity info for {WalletAddress}", walletAddress);
                throw;
            }
        }

        public async Task<LiquidityValidationResponse> ValidateTransactionAsync(string walletAddress, string transactionType, int? poolId = null, int? positionId = null, decimal? token0Amount = null, decimal? token1Amount = null, decimal? percentageToRemove = null)
        {
            try
            {
                var response = new LiquidityValidationResponse();

                switch (transactionType.ToUpper())
                {
                    case "ADD":
                        if (!poolId.HasValue || !token0Amount.HasValue || !token1Amount.HasValue)
                        {
                            response.ValidationMessages.Add("Pool ID and token amounts are required for add liquidity");
                            return response;
                        }

                        var pool = await GetLiquidityPoolAsync(poolId.Value);
                        if (pool == null)
                        {
                            response.ValidationMessages.Add($"Pool {poolId} not found");
                            return response;
                        }

                        // Check balances
                        var token0Balance = await _blockchainService.GetBalanceAsync(walletAddress, pool.Token0Address);
                        var token1Balance = await _blockchainService.GetBalanceAsync(walletAddress, pool.Token1Address);

                        response.HasSufficientBalance = token0Balance >= token0Amount.Value && token1Balance >= token1Amount.Value;
                        if (!response.HasSufficientBalance)
                        {
                            response.ValidationMessages.Add("Insufficient token balance");
                        }

                        // Calculate price impact
                        response.PriceImpact = await _dexIntegrationService.CalculatePriceImpactAsync(
                            pool.PoolAddress, token0Amount.Value, token1Amount.Value);

                        if (response.PriceImpact > 5)
                        {
                            response.WarningMessages.Add($"High price impact: {response.PriceImpact:F2}%");
                        }

                        break;

                    case "REMOVE":
                        if (!positionId.HasValue || !percentageToRemove.HasValue)
                        {
                            response.ValidationMessages.Add("Position ID and percentage are required for remove liquidity");
                            return response;
                        }

                        var position = await _liquidityRepository.GetUserLiquidityPositionByIdAsync(positionId.Value);
                        if (position == null || position.WalletAddress != walletAddress.ToLowerInvariant())
                        {
                            response.ValidationMessages.Add("Position not found or unauthorized");
                            return response;
                        }

                        if (!position.IsActive)
                        {
                            response.ValidationMessages.Add("Position is not active");
                            return response;
                        }

                        break;

                    case "CLAIM_FEES":
                        if (!positionId.HasValue)
                        {
                            response.ValidationMessages.Add("Position ID is required for claim fees");
                            return response;
                        }

                        var claimPosition = await _liquidityRepository.GetUserLiquidityPositionByIdAsync(positionId.Value);
                        if (claimPosition == null || claimPosition.WalletAddress != walletAddress.ToLowerInvariant())
                        {
                            response.ValidationMessages.Add("Position not found or unauthorized");
                            return response;
                        }

                        if (claimPosition.FeesEarnedUsd <= 0)
                        {
                            response.ValidationMessages.Add("No fees available to claim");
                            return response;
                        }

                        break;

                    default:
                        response.ValidationMessages.Add($"Unknown transaction type: {transactionType}");
                        return response;
                }

                // Estimate gas fee
                response.EstimatedGasFee = await EstimateTransactionGasAsync(walletAddress, transactionType, poolId, positionId);

                response.IsValid = !response.ValidationMessages.Any();
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating transaction for {WalletAddress}", walletAddress);
                throw;
            }
        }

        public async Task<List<RewardProjectionModel>> GetLiquidityRewardProjectionsAsync(string walletAddress, int poolId, decimal token0Amount, decimal token1Amount, int days = 365)
        {
            try
            {
                var pool = await GetLiquidityPoolAsync(poolId);
                if (pool == null) return new List<RewardProjectionModel>();

                var projections = new List<RewardProjectionModel>();
                var dailyApy = pool.APY / 365 / 100;
                var totalValue = await CalculateValueUsd(token0Amount, token1Amount, pool);

                var currentDate = DateTime.UtcNow;
                decimal cumulativeRewards = 0;

                for (int day = 1; day <= Math.Min(days, 365); day++)
                {
                    var dailyRewards = totalValue * dailyApy;
                    cumulativeRewards += dailyRewards;

                    if (day % 7 == 0) // Weekly snapshots
                    {
                        projections.Add(new RewardProjectionModel
                        {
                            Date = currentDate.AddDays(day),
                            CumulativeRewards = cumulativeRewards,
                            PeriodRewards = dailyRewards * 7,
                            CompoundedAmount = totalValue + cumulativeRewards,
                            UserShare = cumulativeRewards * 0.5m, // Assuming 50/50 split
                            SchoolShare = cumulativeRewards * 0.5m,
                            FormattedRewards = FormatCurrency(cumulativeRewards),
                            FormattedCompounded = FormatCurrency(totalValue + cumulativeRewards)
                        });
                    }
                }

                return projections;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating reward projections");
                throw;
            }
        }

        public async Task<List<LiquidityTransactionHistoryResponse>> GetUserTransactionHistoryAsync(string walletAddress, int page = 1, int pageSize = 10)
        {
            try
            {
                if (!_blockchainService.IsValidAddress(walletAddress))
                {
                    throw new ArgumentException("Invalid wallet address", nameof(walletAddress));
                }

                var transactions = await _liquidityRepository.GetUserLiquidityTransactionsAsync(walletAddress);

                return transactions
                    .OrderByDescending(t => t.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new LiquidityTransactionHistoryResponse
                    {
                        Id = t.Id,
                        TransactionType = t.TransactionType,
                        TransactionHash = t.TransactionHash,
                        PoolName = t.UserLiquidityPosition?.LiquidityPool?.Name ?? "Unknown Pool",
                        TokenPair = t.UserLiquidityPosition?.LiquidityPool?.TokenPair ?? "Unknown",
                        Token0Amount = t.Token0Amount,
                        Token1Amount = t.Token1Amount,
                        ValueUsd = t.ValueUsd,
                        GasFeesUsd = t.GasFeesUsd,
                        Status = t.Status.ToString(),
                        Timestamp = t.Timestamp,
                        DexName = t.UserLiquidityPosition?.LiquidityPool?.DexName ?? "Unknown"
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user transaction history for {WalletAddress}", walletAddress);
                return new List<LiquidityTransactionHistoryResponse>();
            }
        }

        public async Task<bool> UpdateUserPositionValuesAsync(string walletAddress)
        {
            try
            {
                var positions = await _liquidityRepository.GetUserLiquidityPositionsAsync(walletAddress, true);

                foreach (var position in positions)
                {
                    var token0Price = await _dexIntegrationService.GetTokenPriceAsync(position.LiquidityPool.Token0Address);
                    var token1Price = await _dexIntegrationService.GetTokenPriceAsync(position.LiquidityPool.Token1Address);

                    position.CurrentValueUsd = (position.Token0Amount * token0Price) + (position.Token1Amount * token1Price);

                    // Update fees earned (this would typically come from contract calls)
                    var timeElapsed = DateTime.UtcNow - position.LastUpdatedAt;
                    var dailyFees = position.CurrentValueUsd * (position.LiquidityPool.APY / 100) / 365;
                    position.FeesEarnedUsd += dailyFees * (decimal)timeElapsed.TotalDays;

                    await _liquidityRepository.UpdateUserLiquidityPositionAsync(position);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user position values for {WalletAddress}", walletAddress);
                return false;
            }
        }

        public async Task<decimal> EstimateTransactionGasAsync(string walletAddress, string transactionType, int? poolId = null, int? positionId = null)
        {
            try
            {
                // This would typically call the blockchain service to estimate gas
                // For now, return estimated values based on transaction type
                return transactionType.ToUpper() switch
                {
                    "ADD" => 0.002m, // ~$2 gas fee
                    "REMOVE" => 0.003m, // ~$3 gas fee
                    "CLAIM_FEES" => 0.001m, // ~$1 gas fee
                    _ => 0.002m
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error estimating gas for transaction");
                return 0.002m; // Default estimate
            }
        }

        /// <summary>
        /// Format currency values for display
        /// </summary>
        private string FormatCurrency(decimal value)
        {
            if (value >= 1000000)
                return $"${value / 1000000:F2}M";
            if (value >= 1000)
                return $"${value / 1000:F2}K";
            return $"${value:F2}";
        }

        private string FormatTokenAmount(decimal amount, int decimals = 4)
        {
            return amount.ToString($"N{decimals}");
        }

        private async Task<LiquidityPool?> GetLiquidityPoolEntityAsync(int poolId)
        {
            return await _liquidityRepository.GetLiquidityPoolByIdAsync(poolId);
        }

        #endregion
    }
}