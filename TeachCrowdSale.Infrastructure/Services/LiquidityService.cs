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

        public async Task<List<LiquidityPoolDisplayModel>> GetActiveLiquidityPoolsAsync()
        {
            try
            {
                return await _cache.GetOrCreateAsync($"{CACHE_KEY_POOLS}_active", async entry =>
                {
                    entry.SetAbsoluteExpiration(MediumCacheDuration);

                    var pools = await _liquidityRepository.GetActiveLiquidityPoolsAsync();
                    return pools.Select(MapToLiquidityPoolDisplayModel).ToList();
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

        public async Task<LiquidityStatsOverviewModel> GetLiquidityStatsAsync()
        {
            try
            {
                return await _cache.GetOrCreateAsync(CACHE_KEY_STATS, async entry =>
                {
                    entry.SetAbsoluteExpiration(ShortCacheDuration);

                    var totalTvl = await _liquidityRepository.GetTotalValueLockedAsync();
                    var totalVolume24h = await _liquidityRepository.GetTotal24hVolumeAsync();
                    var totalVolume7d = totalVolume24h * 7; // Approximation
                    var totalFees = await _liquidityRepository.GetTotalFeesGeneratedAsync();
                    var activePools = (await _liquidityRepository.GetActiveLiquidityPoolsAsync()).Count;
                    var activeProviders = await _liquidityRepository.GetActiveLiquidityProvidersCountAsync();

                    // Calculate average APY
                    var pools = await _liquidityRepository.GetActiveLiquidityPoolsAsync();
                    var avgApy = pools.Where(p => p.TotalValueLocked > 0)
                                     .Average(p => p.APY);

                    // Get TEACH price
                    var teachPrice = await GetTeachTokenPriceAsync();

                    return new LiquidityStatsOverviewModel
                    {
                        TotalValueLocked = totalTvl,
                        TotalVolume24h = totalVolume24h,
                        TotalVolume7d = totalVolume7d,
                        TotalFeesGenerated = totalFees,
                        ActivePools = activePools,
                        ActiveProviders = activeProviders,
                        AverageAPY = avgApy,
                        TeachPriceUsd = teachPrice,
                        PriceChange24h = 0, // Would need historical data
                        VolumeChange24h = 0,
                        TvlChange24h = 0,
                        TvlDisplay = FormatCurrency(totalTvl),
                        Volume24hDisplay = FormatCurrency(totalVolume24h),
                        FeesDisplay = FormatCurrency(totalFees),
                        ApyDisplay = $"{avgApy:F2}%",
                        PriceDisplay = FormatCurrency(teachPrice, 4),
                        PriceChangeDisplay = "0%",
                        PriceChangeClass = "neutral"
                    };
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving liquidity statistics");
                throw;
            }
        }

        public async Task<List<DexConfigurationModel>> GetDexConfigurationsAsync()
        {
            try
            {
                return await _cache.GetOrCreateAsync(CACHE_KEY_DEX_CONFIG, async entry =>
                {
                    entry.SetAbsoluteExpiration(LongCacheDuration);

                    var dexConfigs = await _liquidityRepository.GetActiveDexConfigurationsAsync();
                    return dexConfigs.Select(MapToDexConfigurationModel).ToList();
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

        public async Task<List<UserLiquidityPositionModel>> GetUserLiquidityPositionsAsync(string walletAddress)
        {
            try
            {
                if (!_blockchainService.IsValidAddress(walletAddress))
                {
                    throw new ArgumentException("Invalid wallet address", nameof(walletAddress));
                }

                var positions = await _liquidityRepository.GetUserLiquidityPositionsAsync(walletAddress);
                var positionModels = new List<UserLiquidityPositionModel>();

                foreach (var position in positions)
                {
                    var model = await MapToUserLiquidityPositionModelAsync(position);
                    positionModels.Add(model);
                }

                return positionModels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user liquidity positions for {WalletAddress}", walletAddress);
                throw;
            }
        }

        public async Task<UserLiquidityPositionModel?> GetUserLiquidityPositionAsync(int positionId)
        {
            try
            {
                var position = await _liquidityRepository.GetUserLiquidityPositionByIdAsync(positionId);
                if (position == null) return null;

                return await MapToUserLiquidityPositionModelAsync(position);
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

        public async Task<LiquidityCalculationModel> CalculateLiquidityPreviewAsync(string walletAddress, int poolId, decimal token0Amount, decimal? token1Amount = null, decimal slippageTolerance = 0.5m)
        {
            try
            {
                var pool = await GetLiquidityPoolAsync(poolId);
                if (pool == null)
                {
                    throw new InvalidOperationException($"Liquidity pool {poolId} not found");
                }

                var calculation = new LiquidityCalculationModel
                {
                    PoolId = poolId,
                    TokenPair = pool.TokenPair,
                    Token0Symbol = pool.Token0Symbol,
                    Token1Symbol = pool.Token1Symbol,
                    Token0Amount = token0Amount,
                    SlippageTolerance = slippageTolerance
                };

                // Calculate optimal token1 amount if not provided
                if (!token1Amount.HasValue)
                {
                    var (optimalToken0, optimalToken1) = await _dexIntegrationService.CalculateOptimalLiquidityAmountsAsync(
                        pool.PoolAddress, token0Amount, decimal.MaxValue);
                    calculation.Token1Amount = optimalToken1;
                }
                else
                {
                    calculation.Token1Amount = token1Amount.Value;
                }

                // Get current price and calculate minimum amounts
                calculation.CurrentPrice = pool.CurrentPrice;
                var (token0Min, token1Min) = await _dexIntegrationService.CalculateMinimumAmountsAsync(
                    pool.PoolAddress, calculation.Token0Amount, calculation.Token1Amount, slippageTolerance);

                calculation.Token0AmountMin = token0Min;
                calculation.Token1AmountMin = token1Min;

                // Estimate LP tokens
                calculation.ExpectedLpTokens = await _dexIntegrationService.EstimateLpTokensForAmountsAsync(
                    pool.PoolAddress, calculation.Token0Amount, calculation.Token1Amount);

                // Calculate total value
                var token0Price = await _dexIntegrationService.GetTokenPriceAsync(pool.Token0Address);
                var token1Price = await _dexIntegrationService.GetTokenPriceAsync(pool.Token1Address);
                calculation.TotalValueUsd = (calculation.Token0Amount * token0Price) + (calculation.Token1Amount * token1Price);

                // Calculate price impact
                calculation.PriceImpact = await _dexIntegrationService.CalculatePriceImpactAsync(
                    pool.PoolAddress, calculation.Token0Amount, calculation.Token1Amount);

                // Estimate returns
                calculation.EstimatedAPY = pool.APY;
                calculation.EstimatedDailyFees = calculation.TotalValueUsd * (pool.APY / 100) / 365;
                calculation.EstimatedMonthlyFees = calculation.EstimatedDailyFees * 30;
                calculation.EstimatedYearlyFees = calculation.EstimatedDailyFees * 365;

                // Calculate pool share
                var poolTvl = await _dexIntegrationService.GetPoolTotalValueLockedAsync(pool.PoolAddress);
                calculation.PoolShare = poolTvl > 0 ? (calculation.TotalValueUsd / poolTvl) * 100 : 0;

                // Validation
                var canAddLiquidity = await _dexIntegrationService.SimulateAddLiquidityAsync(
                    walletAddress, pool.PoolAddress, calculation.Token0Amount, calculation.Token1Amount);

                calculation.HasSufficientBalance = canAddLiquidity;
                calculation.IsWithinSlippage = calculation.PriceImpact <= 5; // 5% max
                calculation.IsValid = calculation.HasSufficientBalance && calculation.IsWithinSlippage;

                // Risk assessment
                calculation.RiskLevel = CalculateRiskLevel(calculation.PriceImpact, pool.APY);
                calculation.ImpermanentLossEstimate = EstimateImpermanentLoss(calculation.PriceImpact);

                // Formatted displays
                calculation.TotalValueDisplay = FormatCurrency(calculation.TotalValueUsd);
                calculation.ApyDisplay = $"{calculation.EstimatedAPY:F2}%";
                calculation.DailyFeesDisplay = FormatCurrency(calculation.EstimatedDailyFees);
                calculation.MonthlyFeesDisplay = FormatCurrency(calculation.EstimatedMonthlyFees);
                calculation.PriceImpactDisplay = $"{calculation.PriceImpact:F2}%";

                // Validation messages
                if (!calculation.HasSufficientBalance)
                {
                    calculation.ValidationMessages.Add("Insufficient token balance");
                }
                if (calculation.PriceImpact > 5)
                {
                    calculation.WarningMessages.Add($"High price impact: {calculation.PriceImpact:F2}%");
                }

                return calculation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating liquidity preview for pool {PoolId}", poolId);
                throw;
            }
        }

        public async Task<LiquidityCalculationModel> CalculateRemoveLiquidityPreviewAsync(string walletAddress, int positionId, decimal percentageToRemove)
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

                var calculation = new LiquidityCalculationModel
                {
                    PoolId = position.LiquidityPoolId,
                    TokenPair = position.LiquidityPool.TokenPair,
                    Token0Symbol = position.LiquidityPool.Token0Symbol,
                    Token1Symbol = position.LiquidityPool.Token1Symbol,
                    Token0Amount = token0Amount,
                    Token1Amount = token1Amount,
                    ExpectedLpTokens = lpTokensToRemove
                };

                // Calculate value
                var token0Price = await _dexIntegrationService.GetTokenPriceAsync(position.LiquidityPool.Token0Address);
                var token1Price = await _dexIntegrationService.GetTokenPriceAsync(position.LiquidityPool.Token1Address);
                calculation.TotalValueUsd = (token0Amount * token0Price) + (token1Amount * token1Price);

                calculation.IsValid = true;
                calculation.TotalValueDisplay = FormatCurrency(calculation.TotalValueUsd);

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

        public async Task<LiquidityAnalyticsModel> GetLiquidityAnalyticsAsync()
        {
            try
            {
                return await _cache.GetOrCreateAsync("liquidity_analytics", async entry =>
                {
                    entry.SetAbsoluteExpiration(MediumCacheDuration);

                    var analytics = new LiquidityAnalyticsModel
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

        public async Task<List<UserLiquidityStatsModel>> GetTopLiquidityProvidersAsync(int limit = 10)
        {
            try
            {
                return await _liquidityRepository.GetTopLiquidityProvidersAsync(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top liquidity providers");
                throw;
            }
        }

        public async Task<List<PoolPerformanceDataModel>> GetPoolPerformanceAsync()
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

        public async Task<List<LiquidityTrendDataModel>> GetTvlTrendsAsync(int days = 30)
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

        public async Task<List<VolumeTrendDataModel>> GetVolumeTrendsAsync(int days = 30)
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

        public async Task<List<LiquidityGuideStepModel>> GetLiquidityGuideStepsAsync(string? walletAddress = null)
        {
            try
            {
                var steps = new List<LiquidityGuideStepModel>
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

        private LiquidityPoolDisplayModel MapToLiquidityPoolDisplayModel(LiquidityPool pool)
        {
            var isRecommended = pool.APY >= 10m && pool.TotalValueLocked >= 100000m;

            return new LiquidityPoolDisplayModel
            {
                Id = pool.Id,
                Name = pool.Name,
                DexName = pool.DexName,
                TokenPair = pool.TokenPair,
                Token0Symbol = pool.Token0Symbol,
                Token1Symbol = pool.Token1Symbol,
                TotalValueLocked = pool.TotalValueLocked,
                Volume24h = pool.Volume24h,
                Volume7d = pool.Volume7d,
                FeePercentage = pool.FeePercentage,
                APY = pool.APY,
                APR = pool.APR,
                CurrentPrice = pool.CurrentPrice,
                IsActive = pool.IsActive,
                IsFeatured = pool.IsFeatured,
                IsRecommended = isRecommended,
                DexUrl = pool.DexUrl ?? "",
                AnalyticsUrl = pool.AnalyticsUrl ?? "",
                LogoUrl = GetDexLogoUrl(pool.DexName),
                TvlDisplay = FormatCurrency(pool.TotalValueLocked),
                Volume24hDisplay = FormatCurrency(pool.Volume24h),
                ApyDisplay = $"{pool.APY:F2}%",
                FeeDisplay = $"{pool.FeePercentage:F2}%",
                PriceDisplay = FormatCurrency(pool.CurrentPrice, 6),
                StatusClass = pool.IsActive ? "active" : "inactive",
                RecommendationReason = isRecommended ? "High APY & Deep Liquidity" : ""
            };
        }

        private async Task<UserLiquidityPositionModel> MapToUserLiquidityPositionModelAsync(UserLiquidityPosition position)
        {
            // Calculate current value and P&L
            var token0Price = await _dexIntegrationService.GetTokenPriceAsync(position.LiquidityPool.Token0Address);
            var token1Price = await _dexIntegrationService.GetTokenPriceAsync(position.LiquidityPool.Token1Address);
            var currentValue = (position.Token0Amount * token0Price) + (position.Token1Amount * token1Price);
            var pnl = currentValue - position.InitialValueUsd + position.FeesEarnedUsd;
            var pnlPercentage = position.InitialValueUsd > 0 ? (pnl / position.InitialValueUsd) * 100 : 0;

            var daysActive = (DateTime.UtcNow - position.AddedAt).Days;

            return new UserLiquidityPositionModel
            {
                Id = position.Id,
                PoolId = position.LiquidityPoolId,
                PoolName = position.LiquidityPool.Name,
                TokenPair = position.LiquidityPool.TokenPair,
                DexName = position.LiquidityPool.DexName,
                LpTokenAmount = position.LpTokenAmount,
                Token0Amount = position.Token0Amount,
                Token1Amount = position.Token1Amount,
                InitialValueUsd = position.InitialValueUsd,
                CurrentValueUsd = currentValue,
                FeesEarnedUsd = position.FeesEarnedUsd,
                ImpermanentLoss = position.ImpermanentLoss,
                NetPnL = pnl,
                PnLPercentage = pnlPercentage,
                AddedAt = position.AddedAt,
                LastUpdatedAt = position.LastUpdatedAt,
                IsActive = position.IsActive,
                CanRemove = position.IsActive,
                CanClaimFees = position.IsActive && position.FeesEarnedUsd > 0,
                InitialValueDisplay = FormatCurrency(position.InitialValueUsd),
                CurrentValueDisplay = FormatCurrency(currentValue),
                FeesEarnedDisplay = FormatCurrency(position.FeesEarnedUsd),
                PnLDisplay = FormatCurrency(pnl, 2, true),
                PnLClass = pnl >= 0 ? "positive" : "negative",
                DaysActive = daysActive == 1 ? "1 day" : $"{daysActive} days",
                Token0AmountDisplay = FormatTokenAmount(position.Token0Amount),
                Token1AmountDisplay = FormatTokenAmount(position.Token1Amount),
                Token0Symbol = position.LiquidityPool.Token0Symbol,
                Token1Symbol = position.LiquidityPool.Token1Symbol
            };
        }

        private DexConfigurationModel MapToDexConfigurationModel(DexConfiguration dex)
        {
            return new DexConfigurationModel
            {
                Id = dex.Id,
                Name = dex.Name,
                DisplayName = dex.DisplayName,
                Description = dex.Description ?? "",
                LogoUrl = dex.LogoUrl,
                BaseUrl = dex.BaseUrl,
                IsRecommended = dex.IsRecommended,
                DefaultFeePercentage = dex.DefaultFeePercentage,
                Network = dex.Network
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

        private async Task<List<DexComparisonModel>> GetDexComparisonDataAsync()
        {
            try
            {
                var dexes = await _liquidityRepository.GetActiveDexConfigurationsAsync();
                var comparisons = new List<DexComparisonModel>();

                foreach (var dex in dexes)
                {
                    var pools = await _liquidityRepository.GetPoolsByDexAsync(dex.Name);
                    var tvl = pools.Sum(p => p.TotalValueLocked);
                    var volume = pools.Sum(p => p.Volume24h);
                    var avgApy = pools.Any() ? pools.Average(p => p.APY) : 0;

                    comparisons.Add(new DexComparisonModel
                    {
                        DexName = dex.DisplayName,
                        TotalValueLocked = tvl,
                        Volume24h = volume,
                        AverageAPY = avgApy,
                        PoolsCount = pools.Count
                    });
                }

                return comparisons.OrderByDescending(d => d.TotalValueLocked).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting DEX comparison data");
                return new List<DexComparisonModel>();
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
            if (priceImpact < 1 && apy < 20) return "Low";
            if (priceImpact < 3 && apy < 50) return "Medium";
            return "High";
        }

        private string EstimateImpermanentLoss(decimal priceImpact)
        {
            if (priceImpact < 1) return "Minimal (<0.1%)";
            if (priceImpact < 5) return "Low (0.1-2%)";
            return "Moderate (2-5%)";
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
        public async Task<UserLiquidityInfoModel> GetUserLiquidityInfoAsync(string walletAddress)
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

                return new UserLiquidityInfoModel
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
                    Stats = new UserLiquidityStatsModel
                    {
                        WalletAddress = walletAddress,
                        DisplayAddress = $"{walletAddress[..6]}...{walletAddress[^4..]}",
                        TotalValueProvided = totalValue,
                        TotalFeesEarned = totalFees,
                        ActivePositions = positions.Count(p => p.IsActive),
                        FirstProvisionDate = firstPosition?.AddedAt ?? DateTime.UtcNow
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

        public async Task<List<LiquidityTransactionHistoryModel>> GetUserTransactionHistoryAsync(string walletAddress, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                var transactions = await _liquidityRepository.GetUserLiquidityTransactionsAsync(walletAddress);

                return transactions
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new LiquidityTransactionHistoryModel
                    {
                        Id = t.Id,
                        TransactionType = t.TransactionType,
                        PoolName = t.UserLiquidityPosition?.LiquidityPool?.Name ?? "Unknown Pool",
                        TokenPair = t.UserLiquidityPosition?.LiquidityPool?.TokenPair ?? "",
                        ValueUsd = t.ValueUsd,
                        Timestamp = t.Timestamp,
                        TransactionHash = t.TransactionHash,
                        Status = t.Status.ToString(),
                        FormattedValue = FormatCurrency(t.ValueUsd),
                        FormattedDate = t.Timestamp.ToString("MMM dd, yyyy HH:mm"),
                        StatusClass = t.Status.ToString().ToLower()
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user transaction history for {WalletAddress}", walletAddress);
                throw;
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

        private string FormatCurrency(decimal amount, int decimals = 2, bool showSign = false)
        {
            var formatted = amount.ToString($"C{decimals}");
            if (showSign && amount > 0)
            {
                formatted = "+" + formatted;
            }
            return formatted;
        }

        private string FormatTokenAmount(decimal amount, int decimals = 4)
        {
            return amount.ToString($"N{decimals}");
        }

        #endregion
    }
}