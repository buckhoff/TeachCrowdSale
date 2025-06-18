using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;

using TeachCrowdSale.Core.Interfaces.Services;
using System.Numerics;

namespace TeachCrowdSale.Api.Service
{

    /// <summary>
    /// Service for integrating with various DEX APIs and contracts
    /// </summary>
    public class DexIntegrationService : IDexIntegrationService
    {
        private readonly ILogger<DexIntegrationService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IBlockchainService _blockchainService;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;

        // DEX API endpoints
        private readonly Dictionary<string, string> _dexApiEndpoints;

        // Cache keys and durations
        private const string CACHE_KEY_TOKEN_PRICE = "token_price";
        private const string CACHE_KEY_POOL_RESERVES = "pool_reserves";
        private const string CACHE_KEY_POOL_TVL = "pool_tvl";
        private const string CACHE_KEY_POOL_VOLUME = "pool_volume";
        private static readonly TimeSpan PriceCacheDuration = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan PoolDataCacheDuration = TimeSpan.FromMinutes(5);

        public DexIntegrationService(
            ILogger<DexIntegrationService> logger,
            IHttpClientFactory httpClientFactory,
            IBlockchainService blockchainService,
            IConfiguration configuration,
            IMemoryCache cache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _blockchainService = blockchainService ?? throw new ArgumentNullException(nameof(blockchainService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));

            _dexApiEndpoints = new Dictionary<string, string>
            {
                ["quickswap"] = _configuration["DexIntegrationSettings:QuickSwapApiUrl"] ?? "https://api.thegraph.com/subgraphs/name/sameepsi/quickswap06",
                ["uniswap"] = _configuration["DexIntegrationSettings:UniswapApiUrl"] ?? "https://api.thegraph.com/subgraphs/name/uniswap/uniswap-v3-polygon",
                ["sushiswap"] = _configuration["DexIntegrationSettings:SushiSwapApiUrl"] ?? "https://api.thegraph.com/subgraphs/name/sushiswap/exchange-polygon",
                ["1inch"] = _configuration["DexIntegrationSettings:OneInchApiUrl"] ?? "https://api.1inch.io/v5.0/137"
            };
        }

        #region Price and Reserve Data

        public async Task<decimal> GetTokenPriceAsync(string tokenAddress, string quoteCurrency = "USDC")
        {
            try
            {
                var cacheKey = $"{CACHE_KEY_TOKEN_PRICE}_{tokenAddress}_{quoteCurrency}";
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.SetAbsoluteExpiration(PriceCacheDuration);

                    // Try multiple sources for price data
                    var price = await TryGet1inchPriceAsync(tokenAddress, quoteCurrency);
                    if (price > 0) return price;

                    price = await TryGetUniswapPriceAsync(tokenAddress, quoteCurrency);
                    if (price > 0) return price;

                    price = await TryGetQuickSwapPriceAsync(tokenAddress, quoteCurrency);
                    if (price > 0) return price;

                    // Fallback to blockchain contract call
                    return await GetPriceFromContractAsync(tokenAddress, quoteCurrency);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token price for {TokenAddress}", tokenAddress);
                return 0;
            }
        }

        public async Task<(decimal token0Reserve, decimal token1Reserve, decimal totalSupply)> GetPoolReservesAsync(string poolAddress)
        {
            try
            {
                var cacheKey = $"{CACHE_KEY_POOL_RESERVES}_{poolAddress}";
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.SetAbsoluteExpiration(PoolDataCacheDuration);

                    try
                    {
                        // Try subgraph first for better performance
                        var subgraphData = await GetPoolReservesFromSubgraphAsync(poolAddress);
                        if (subgraphData.token0Reserve > 0 || subgraphData.token1Reserve > 0)
                        {
                            return subgraphData;
                        }

                        // Fallback to direct contract calls
                        return await GetPoolReservesFromContractAsync(poolAddress);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error getting pool reserves from subgraph, trying contract");
                        return await GetPoolReservesFromContractAsync(poolAddress);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pool reserves for {PoolAddress}", poolAddress);
                return (0, 0, 0);
            }
        }

        public async Task<decimal> GetPoolTotalValueLockedAsync(string poolAddress)
        {
            try
            {
                var cacheKey = $"{CACHE_KEY_POOL_TVL}_{poolAddress}";
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.SetAbsoluteExpiration(PoolDataCacheDuration);

                    // Try subgraph first
                    var tvl = await GetTvlFromSubgraphAsync(poolAddress);
                    if (tvl > 0) return tvl;

                    // Calculate from reserves and prices
                    var (token0Reserve, token1Reserve, _) = await GetPoolReservesAsync(poolAddress);
                    var token0Address = await GetToken0AddressAsync(poolAddress);
                    var token1Address = await GetToken1AddressAsync(poolAddress);

                    var token0Price = await GetTokenPriceAsync(token0Address);
                    var token1Price = await GetTokenPriceAsync(token1Address);

                    return token0Reserve * token0Price + token1Reserve * token1Price;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating TVL for pool {PoolAddress}", poolAddress);
                return 0;
            }
        }

        public async Task<decimal> GetPool24hVolumeAsync(string poolAddress)
        {
            try
            {
                var cacheKey = $"{CACHE_KEY_POOL_VOLUME}_{poolAddress}";
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.SetAbsoluteExpiration(PoolDataCacheDuration);

                    // Try multiple DEX subgraphs
                    var volume = await GetVolumeFromQuickSwapAsync(poolAddress);
                    if (volume > 0) return volume;

                    volume = await GetVolumeFromUniswapAsync(poolAddress);
                    if (volume > 0) return volume;

                    volume = await GetVolumeFromSushiSwapAsync(poolAddress);
                    return volume;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting 24h volume for pool {PoolAddress}", poolAddress);
                return 0;
            }
        }

        #endregion

        #region APY and Fee Calculations

        public async Task<decimal> CalculatePoolAPYAsync(string poolAddress, int days = 7)
        {
            try
            {
                var fromTimestamp = DateTimeOffset.UtcNow.AddDays(-days).ToUnixTimeSeconds();

                // Try QuickSwap first
                var query = $@"{{
                    poolDayDatas(
                        where: {{ 
                            pool: ""{poolAddress.ToLower()}"",
                            date_gte: {fromTimestamp}
                        }}
                        orderBy: date
                        orderDirection: desc
                        first: {days}
                    ) {{
                        volumeUSD
                        tvlUSD
                        feesUSD
                        date
                    }}
                }}";

                var result = await QuerySubgraphAsync("quickswap", query);
                if (result != null && result.RootElement.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("poolDayDatas", out var poolDayDatas) &&
                    poolDayDatas.GetArrayLength() > 0)
                {
                    decimal totalFees = 0;
                    decimal avgTvl = 0;
                    int count = 0;

                    foreach (var dayData in poolDayDatas.EnumerateArray())
                    {
                        if (dayData.TryGetProperty("feesUSD", out var feesElement) &&
                            dayData.TryGetProperty("tvlUSD", out var tvlElement))
                        {
                            if (decimal.TryParse(feesElement.GetString(), out var fees) &&
                                decimal.TryParse(tvlElement.GetString(), out var tvl))
                            {
                                totalFees += fees;
                                avgTvl += tvl;
                                count++;
                            }
                        }
                    }

                    if (count > 0 && avgTvl > 0)
                    {
                        avgTvl = avgTvl / count;
                        var dailyYield = totalFees / avgTvl / days;
                        return dailyYield * 365 * 100; // Convert to annual percentage
                    }
                }

                // Fallback calculation using volume and fee percentage
                var volume24h = await GetPool24hVolumeAsync(poolAddress);
                var ptvl = await GetPoolTotalValueLockedAsync(poolAddress);
                var feePercentage = await GetPoolFeePercentageAsync(poolAddress);

                if (ptvl > 0 && volume24h > 0)
                {
                    var dailyFees = volume24h * (feePercentage / 100);
                    var dailyYield = dailyFees / ptvl;
                    return dailyYield * 365 * 100;
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating APY for pool {PoolAddress}", poolAddress);
                return 0;
            }
        }

        public async Task<decimal> GetPoolFeePercentageAsync(string poolAddress)
        {
            try
            {
                // Try to get fee from contract
                var fee = await _blockchainService.CallContractFunctionAsync<BigInteger>(
                    poolAddress, "fee()");

                if (fee > 0)
                {
                    return (decimal)fee / 10000; // Convert from basis points
                }

                // Default to 0.3% for most Uniswap V2 forks
                return 0.3m;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not get fee percentage for pool {PoolAddress}, using default 0.3%", poolAddress);
                return 0.3m; // Default 0.3%
            }
        }

        public async Task<decimal> GetPoolFeesGenerated24hAsync(string poolAddress)
        {
            try
            {
                var volume24h = await GetPool24hVolumeAsync(poolAddress);
                var feePercentage = await GetPoolFeePercentageAsync(poolAddress);

                return volume24h * (feePercentage / 100);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating 24h fees for pool {PoolAddress}", poolAddress);
                return 0;
            }
        }

        #endregion

        #region Liquidity Calculations

        public async Task<(decimal token0Amount, decimal token1Amount)> CalculateOptimalLiquidityAmountsAsync(string poolAddress, decimal token0Desired, decimal token1Desired)
        {
            try
            {
                var (reserve0, reserve1, _) = await GetPoolReservesAsync(poolAddress);

                if (reserve0 == 0 || reserve1 == 0)
                {
                    return (token0Desired, token1Desired);
                }

                // Calculate optimal amounts based on current pool ratio
                var ratio = reserve1 / reserve0;
                var token1Optimal = token0Desired * ratio;

                if (token1Optimal <= token1Desired)
                {
                    return (token0Desired, token1Optimal);
                }
                else
                {
                    var token0Optimal = token1Desired / ratio;
                    return (token0Optimal, token1Desired);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating optimal liquidity amounts for pool {PoolAddress}", poolAddress);
                return (token0Desired, token1Desired);
            }
        }

        public async Task<decimal> EstimateLpTokensForAmountsAsync(string poolAddress, decimal token0Amount, decimal token1Amount)
        {
            try
            {
                var (reserve0, reserve1, totalSupply) = await GetPoolReservesAsync(poolAddress);

                if (totalSupply == 0)
                {
                    // First liquidity provision - use geometric mean
                    return (decimal)Math.Sqrt((double)(token0Amount * token1Amount));
                }

                // Calculate LP tokens based on proportion
                var liquidity0 = token0Amount * totalSupply / reserve0;
                var liquidity1 = token1Amount * totalSupply / reserve1;

                return Math.Min(liquidity0, liquidity1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error estimating LP tokens for pool {PoolAddress}", poolAddress);
                return 0;
            }
        }

        public async Task<(decimal token0Amount, decimal token1Amount)> EstimateAmountsForLpTokensAsync(string poolAddress, decimal lpTokenAmount)
        {
            try
            {
                var (reserve0, reserve1, totalSupply) = await GetPoolReservesAsync(poolAddress);

                if (totalSupply == 0)
                {
                    return (0, 0);
                }

                var token0Amount = lpTokenAmount * reserve0 / totalSupply;
                var token1Amount = lpTokenAmount * reserve1 / totalSupply;

                return (token0Amount, token1Amount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error estimating amounts for LP tokens in pool {PoolAddress}", poolAddress);
                return (0, 0);
            }
        }

        #endregion

        #region Price Impact and Slippage

        public async Task<decimal> CalculatePriceImpactAsync(string poolAddress, decimal token0Amount, decimal token1Amount)
        {
            try
            {
                var (reserve0, reserve1, _) = await GetPoolReservesAsync(poolAddress);

                if (reserve0 == 0 || reserve1 == 0)
                {
                    return 0;
                }

                var currentPrice = reserve1 / reserve0;
                var newReserve0 = reserve0 + token0Amount;
                var newReserve1 = reserve1 + token1Amount;
                var newPrice = newReserve1 / newReserve0;

                var priceImpact = Math.Abs((newPrice - currentPrice) / currentPrice) * 100;
                return priceImpact;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating price impact for pool {PoolAddress}", poolAddress);
                return 0;
            }
        }

        public async Task<(decimal token0Min, decimal token1Min)> CalculateMinimumAmountsAsync(string poolAddress, decimal token0Amount, decimal token1Amount, decimal slippageTolerance)
        {
            try
            {
                var slippageMultiplier = (100 - slippageTolerance) / 100;

                return (
                    token0Amount * slippageMultiplier,
                    token1Amount * slippageMultiplier
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating minimum amounts for pool {PoolAddress}", poolAddress);
                return (token0Amount, token1Amount);
            }
        }

        #endregion

        #region Pool Discovery and Validation

        public async Task<bool> ValidatePoolExistsAsync(string poolAddress)
        {
            try
            {
                var (reserve0, reserve1, _) = await GetPoolReservesAsync(poolAddress);
                return reserve0 > 0 || reserve1 > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating pool {PoolAddress}", poolAddress);
                return false;
            }
        }

        public async Task<string?> FindPoolAddressAsync(string token0Address, string token1Address, string dexName)
        {
            try
            {
                var query = $@"{{
                    pairs(
                        where: {{
                            or: [
                                {{ token0: ""{token0Address.ToLower()}"", token1: ""{token1Address.ToLower()}"" }},
                                {{ token0: ""{token1Address.ToLower()}"", token1: ""{token0Address.ToLower()}"" }}
                            ]
                        }}
                        first: 1
                        orderBy: reserveUSD
                        orderDirection: desc
                    ) {{
                        id
                        token0 {{ id symbol }}
                        token1 {{ id symbol }}
                        reserveUSD
                    }}
                }}";

                var result = await QuerySubgraphAsync(dexName.ToLower(), query);
                if (result != null && result.RootElement.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("pairs", out var pairs) &&
                    pairs.GetArrayLength() > 0)
                {
                    return pairs[0].GetProperty("id").GetString();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding pool address for tokens {Token0}-{Token1} on {DexName}", token0Address, token1Address, dexName);
                return null;
            }
        }

        public async Task<List<string>> GetAllPoolsForTokenAsync(string tokenAddress, string dexName)
        {
            try
            {
                var query = $@"{{
                    pairs(
                        where: {{
                            or: [
                                {{ token0: ""{tokenAddress.ToLower()}"" }},
                                {{ token1: ""{tokenAddress.ToLower()}"" }}
                            ]
                        }}
                        orderBy: reserveUSD
                        orderDirection: desc
                        first: 20
                    ) {{
                        id
                        token0 {{ id symbol }}
                        token1 {{ id symbol }}
                        reserveUSD
                    }}
                }}";

                var result = await QuerySubgraphAsync(dexName.ToLower(), query);
                var pools = new List<string>();

                if (result != null && result.RootElement.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("pairs", out var pairs))
                {
                    foreach (var pair in pairs.EnumerateArray())
                    {
                        pools.Add(pair.GetProperty("id").GetString()!);
                    }
                }

                return pools;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pools for token {TokenAddress} on {DexName}", tokenAddress, dexName);
                return new List<string>();
            }
        }

        #endregion

        #region Transaction Simulation

        public async Task<bool> SimulateAddLiquidityAsync(string walletAddress, string poolAddress, decimal token0Amount, decimal token1Amount)
        {
            try
            {
                // Check user balances
                var token0Address = await GetToken0AddressAsync(poolAddress);
                var token1Address = await GetToken1AddressAsync(poolAddress);

                var token0Balance = await _blockchainService.GetBalanceAsync(walletAddress, token0Address);
                var token1Balance = await _blockchainService.GetBalanceAsync(walletAddress, token1Address);

                return token0Balance >= token0Amount && token1Balance >= token1Amount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error simulating add liquidity for {WalletAddress}", walletAddress);
                return false;
            }
        }

        public async Task<bool> SimulateRemoveLiquidityAsync(string walletAddress, string poolAddress, decimal lpTokenAmount)
        {
            try
            {
                var lpBalance = await _blockchainService.GetBalanceAsync(walletAddress, poolAddress);
                return lpBalance >= lpTokenAmount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error simulating remove liquidity for {WalletAddress}", walletAddress);
                return false;
            }
        }

        #endregion

        #region Health and Status

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var healthStatus = await GetDexHealthStatusAsync();
                return healthStatus.Values.Any(status => status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking DEX integration health");
                return false;
            }
        }

        public async Task<Dictionary<string, bool>> GetDexHealthStatusAsync()
        {
            var healthStatus = new Dictionary<string, bool>();

            foreach (var dex in _dexApiEndpoints.Keys)
            {
                try
                {
                    var isHealthy = await CheckDexHealthAsync(dex);
                    healthStatus[dex] = isHealthy;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Health check failed for DEX {DexName}", dex);
                    healthStatus[dex] = false;
                }
            }

            return healthStatus;
        }

        #endregion

        #region Private Helper Methods

        private async Task<decimal> TryGet1inchPriceAsync(string tokenAddress, string quoteCurrency)
        {
            try
            {
                using var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);

                var quoteAddress = GetTokenAddressBySymbol(quoteCurrency);
                var url = $"{_dexApiEndpoints["1inch"]}/quote?fromTokenAddress={tokenAddress}&toTokenAddress={quoteAddress}&amount=1000000000000000000";

                var response = await client.GetStringAsync(url);
                var json = JsonDocument.Parse(response);

                if (json.RootElement.TryGetProperty("toTokenAmount", out var amount))
                {
                    return decimal.Parse(amount.GetString()!) / 1000000000000000000m;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get price from 1inch for {TokenAddress}", tokenAddress);
            }

            return 0;
        }

        private async Task<decimal> TryGetUniswapPriceAsync(string tokenAddress, string quoteCurrency)
        {
            try
            {
                var query = $@"{{
                    token(id: ""{tokenAddress.ToLower()}"") {{
                        derivedETH
                        symbol
                    }}
                }}";

                var result = await QuerySubgraphAsync("uniswap", query);
                if (result != null && result.RootElement.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("token", out var token) &&
                    token.TryGetProperty("derivedETH", out var derivedETH))
                {
                    var ethPrice = await GetETHPriceInUSDCAsync();
                    return decimal.Parse(derivedETH.GetString()!) * ethPrice;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get price from Uniswap for {TokenAddress}", tokenAddress);
            }

            return 0;
        }

        private async Task<decimal> TryGetQuickSwapPriceAsync(string tokenAddress, string quoteCurrency)
        {
            try
            {
                var query = $@"{{
                    token(id: ""{tokenAddress.ToLower()}"") {{
                        derivedUSD
                        symbol
                    }}
                }}";

                var result = await QuerySubgraphAsync("quickswap", query);
                if (result != null && result.RootElement.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("token", out var token) &&
                    token.TryGetProperty("derivedUSD", out var derivedUSD))
                {
                    return decimal.Parse(derivedUSD.GetString()!);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get price from QuickSwap for {TokenAddress}", tokenAddress);
            }

            return 0;
        }

        private async Task<decimal> GetPriceFromContractAsync(string tokenAddress, string quoteCurrency)
        {
            try
            {
                // This would involve calling a DEX router or price oracle contract
                // For now, return 0 as a fallback
                _logger.LogWarning("Contract-based price fetching not implemented for {TokenAddress}", tokenAddress);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get price from contract for {TokenAddress}", tokenAddress);
                return 0;
            }
        }

        private async Task<(decimal token0Reserve, decimal token1Reserve, decimal totalSupply)> GetPoolReservesFromSubgraphAsync(string poolAddress)
        {
            try
            {
                var query = $@"{{
                    pair(id: ""{poolAddress.ToLower()}"") {{
                        reserve0
                        reserve1
                        totalSupply
                        token0 {{ decimals }}
                        token1 {{ decimals }}
                    }}
                }}";

                var result = await QuerySubgraphAsync("quickswap", query);
                if (result != null && result.RootElement.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("pair", out var pair))
                {
                    var reserve0 = decimal.Parse(pair.GetProperty("reserve0").GetString()!);
                    var reserve1 = decimal.Parse(pair.GetProperty("reserve1").GetString()!);
                    var totalSupply = decimal.Parse(pair.GetProperty("totalSupply").GetString()!);

                    return (reserve0, reserve1, totalSupply);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get reserves from subgraph for {PoolAddress}", poolAddress);
            }

            return (0, 0, 0);
        }

        private async Task<(decimal token0Reserve, decimal token1Reserve, decimal totalSupply)> GetPoolReservesFromContractAsync(string poolAddress)
        {
            try
            {
                // Call pool contract directly for most accurate data
                var reserves = await _blockchainService.CallContractFunctionAsync<(BigInteger, BigInteger, uint)>(
                    poolAddress, "getReserves()");

                var totalSupply = await _blockchainService.CallContractFunctionAsync<BigInteger>(
                    poolAddress, "totalSupply()");

                return (
                    ConvertFromWei(reserves.Item1, 18),
                    ConvertFromWei(reserves.Item2, 18),
                    ConvertFromWei(totalSupply, 18)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pool reserves from contract for {PoolAddress}", poolAddress);
                return (0, 0, 0);
            }
        }

        private async Task<decimal> GetTvlFromSubgraphAsync(string poolAddress)
        {
            try
            {
                var query = $@"{{
                    pair(id: ""{poolAddress.ToLower()}"") {{
                        reserveUSD
                        totalSupply
                    }}
                }}";

                var result = await QuerySubgraphAsync("quickswap", query);
                if (result != null && result.RootElement.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("pair", out var pair) &&
                    pair.TryGetProperty("reserveUSD", out var reserveUSD))
                {
                    return decimal.Parse(reserveUSD.GetString()!);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get TVL from subgraph for {PoolAddress}", poolAddress);
            }

            return 0;
        }

        private async Task<decimal> GetVolumeFromQuickSwapAsync(string poolAddress)
        {
            try
            {
                var query = $@"{{
                    pairDayDatas(
                        where: {{ 
                            pairAddress: ""{poolAddress.ToLower()}"" 
                        }}
                        orderBy: date
                        orderDirection: desc
                        first: 1
                    ) {{
                        dailyVolumeUSD
                        date
                    }}
                }}";

                var result = await QuerySubgraphAsync("quickswap", query);
                if (result != null && result.RootElement.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("pairDayDatas", out var pairDayDatas) &&
                    pairDayDatas.GetArrayLength() > 0)
                {
                    var volumeUSD = pairDayDatas[0].GetProperty("dailyVolumeUSD").GetString();
                    return decimal.TryParse(volumeUSD, out var volume) ? volume : 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get volume from QuickSwap for {PoolAddress}", poolAddress);
            }

            return 0;
        }

        private async Task<decimal> GetVolumeFromUniswapAsync(string poolAddress)
        {
            try
            {
                var query = $@"{{
                    poolDayDatas(
                        where: {{ 
                            pool: ""{poolAddress.ToLower()}"" 
                        }}
                        orderBy: date
                        orderDirection: desc
                        first: 1
                    ) {{
                        volumeUSD
                        date
                    }}
                }}";

                var result = await QuerySubgraphAsync("uniswap", query);
                if (result != null && result.RootElement.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("poolDayDatas", out var poolDayDatas) &&
                    poolDayDatas.GetArrayLength() > 0)
                {
                    var volumeUSD = poolDayDatas[0].GetProperty("volumeUSD").GetString();
                    return decimal.TryParse(volumeUSD, out var volume) ? volume : 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get volume from Uniswap for {PoolAddress}", poolAddress);
            }

            return 0;
        }

        private async Task<decimal> GetVolumeFromSushiSwapAsync(string poolAddress)
        {
            try
            {
                var query = $@"{{
                    pairDayDatas(
                        where: {{ 
                            pair: ""{poolAddress.ToLower()}"" 
                        }}
                        orderBy: date
                        orderDirection: desc
                        first: 1
                    ) {{
                        volumeUSD
                        date
                    }}
                }}";

                var result = await QuerySubgraphAsync("sushiswap", query);
                if (result != null && result.RootElement.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("pairDayDatas", out var pairDayDatas) &&
                    pairDayDatas.GetArrayLength() > 0)
                {
                    var volumeUSD = pairDayDatas[0].GetProperty("volumeUSD").GetString();
                    return decimal.TryParse(volumeUSD, out var volume) ? volume : 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get volume from SushiSwap for {PoolAddress}", poolAddress);
            }

            return 0;
        }

        private async Task<JsonDocument?> QuerySubgraphAsync(string dexName, string query)
        {
            try
            {
                if (!_dexApiEndpoints.TryGetValue(dexName, out var endpoint))
                {
                    _logger.LogWarning("Unknown DEX: {DexName}", dexName);
                    return null;
                }

                using var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(30);

                var content = new StringContent(
                    JsonSerializer.Serialize(new { query }),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(endpoint, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonDocument.Parse(responseContent);
                }
                else
                {
                    _logger.LogWarning("Subgraph query failed for {DexName} with status {StatusCode}", dexName, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying subgraph for {DexName}", dexName);
            }

            return null;
        }

        private async Task<bool> CheckDexHealthAsync(string dexName)
        {
            try
            {
                if (!_dexApiEndpoints.TryGetValue(dexName, out var endpoint))
                {
                    return false;
                }

                using var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);

                // Simple health check query
                var healthQuery = @"{
                    _meta {
                        block {
                            number
                            timestamp
                        }
                    }
                }";

                var content = new StringContent(
                    JsonSerializer.Serialize(new { query = healthQuery }),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(endpoint, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Health check failed for {DexName}", dexName);
                return false;
            }
        }

        private async Task<string> GetToken0AddressAsync(string poolAddress)
        {
            try
            {
                return await _blockchainService.CallContractFunctionAsync<string>(poolAddress, "token0()");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token0 address for pool {PoolAddress}", poolAddress);
                return string.Empty;
            }
        }

        private async Task<string> GetToken1AddressAsync(string poolAddress)
        {
            try
            {
                return await _blockchainService.CallContractFunctionAsync<string>(poolAddress, "token1()");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token1 address for pool {PoolAddress}", poolAddress);
                return string.Empty;
            }
        }

        private async Task<decimal> GetETHPriceInUSDCAsync()
        {
            try
            {
                return await _cache.GetOrCreateAsync("eth_price_usdc", async entry =>
                {
                    entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

                    // Try to get ETH price from multiple sources
                    var ethAddress = GetTokenAddressBySymbol("WETH");
                    var price = await TryGet1inchPriceAsync(ethAddress, "USDC");

                    if (price > 0) return price;

                    // Fallback to a reasonable estimate
                    return 2000m;
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting ETH price, using fallback");
                return 2000m; // Fallback price
            }
        }

        private string GetTokenAddressBySymbol(string symbol)
        {
            // Map common token symbols to addresses on Polygon
            return symbol.ToUpper() switch
            {
                "USDC" => "0x2791Bca1f2de4661ED88A30C99A7a9449Aa84174",
                "USDT" => "0xc2132D05D31c914a87C6611C10748AEb04B58e8F",
                "WETH" => "0x7ceB23fD6bC0adD59E62ac25578270cFf1b9f619",
                "WMATIC" => "0x0d500B1d8E8eF31E21C99d1Db9A6444d3ADf1270",
                "DAI" => "0x8f3Cf7ad23Cd3CaDbD9735AFf958023239c6A063",
                "WBTC" => "0x1bfd67037b42cf73acf2047067bd4f2c47d9bfd6",
                "TEACH" => _configuration["TokenSettings:TeachTokenAddress"] ?? "0x0000000000000000000000000000000000000000",
                _ => throw new ArgumentException($"Unknown token symbol: {symbol}")
            };
        }

        private decimal ConvertFromWei(BigInteger wei, int decimals)
        {
            var divisor = BigInteger.Pow(10, decimals);
            return (decimal)wei / (decimal)divisor;
        }

        private BigInteger ConvertToWei(decimal amount, int decimals)
        {
            var multiplier = (decimal)Math.Pow(10, decimals);
            return new BigInteger(amount * multiplier);
        }

        public Task<decimal> GetTotalLpTokenSupplyAsync(string poolAddress)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}