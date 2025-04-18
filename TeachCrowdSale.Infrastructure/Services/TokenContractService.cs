using System;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Models;

namespace TeachCrowdSale.Infrastructure.Services
{
    public class TokenContractService : ITokenContractService
    {
        private readonly ILogger<TokenContractService> _logger;
        private readonly IBlockchainService _blockchainService;
        
        // Cache variables
        private decimal _cachedTokenPrice;
        private DateTime _lastPriceCacheUpdate = DateTime.MinValue;
        private readonly TimeSpan _priceCacheExpiry = TimeSpan.FromMinutes(5);
        
        public TokenContractService(
            ILogger<TokenContractService> logger,
            IBlockchainService blockchainService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _blockchainService = blockchainService ?? throw new ArgumentNullException(nameof(blockchainService));
        }
        
        public async Task<decimal> GetTotalSupplyAsync()
        {
            try
            {
                // Get contract addresses
                var contractAddresses = _blockchainService.GetContractAddresses();
                
                // Call totalSupply function on the token contract
                var totalSupply = await _blockchainService.CallContractFunctionAsync<BigInteger>(
                    contractAddresses.TokenAddress, 
                    "totalSupply()",
                    Array.Empty<object>());
                
                // Convert from wei (18 decimals) to regular units
                return ConvertFromWei(totalSupply, 18);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total supply");
                throw;
            }
        }
        
        public async Task<decimal> GetCirculatingSupplyAsync()
        {
            try
            {
                // Get contract addresses
                var contractAddresses = _blockchainService.GetContractAddresses();
                
                // Get total supply
                var totalSupply = await GetTotalSupplyAsync();
                
                // Subtract tokens in known non-circulating addresses (treasury, team vesting, etc.)
                // This would normally come from contract state or a configuration
                
                // For demo purposes, we'll assume 30% of tokens are not in circulation
                return totalSupply * 0.7m;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting circulating supply");
                throw;
            }
        }
        
        public async Task<decimal> GetTokenPriceAsync()
        {
            try
            {
                // Check if we have a recent price cache
                if (DateTime.UtcNow - _lastPriceCacheUpdate < _priceCacheExpiry)
                {
                    return _cachedTokenPrice;
                }
                
                // Get contract addresses
                var contractAddresses = _blockchainService.GetContractAddresses();
                
                // In a real implementation, we would call the price from a price oracle or DEX
                // For this example, we'll get it from our stability fund contract
                
                if (!string.IsNullOrEmpty(contractAddresses.StabilityFundAddress))
                {
                    try
                    {
                        var price = await _blockchainService.CallContractFunctionAsync<BigInteger>(
                            contractAddresses.StabilityFundAddress, 
                            "getVerifiedPrice()",
                            Array.Empty<object>());
                        
                        // Price is in USD with 18 decimals
                        var usdPrice = ConvertFromWei(price, 18);
                        
                        // Update cache
                        _cachedTokenPrice = usdPrice;
                        _lastPriceCacheUpdate = DateTime.UtcNow;
                        
                        return usdPrice;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error getting price from stability fund, using fallback price");
                    }
                }
                
                // Fallback to a hardcoded price if we can't get it from the contract
                // In a real app, this would come from an API call to a price aggregator
                _cachedTokenPrice = 0.045m; // $0.045 USD
                _lastPriceCacheUpdate = DateTime.UtcNow;
                
                return _cachedTokenPrice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token price");
                throw;
            }
        }
        
        public async Task<int> GetHoldersCountAsync()
        {
            try
            {
                // In a real implementation, this would come from contract or an indexer API
                // For this example, we'll return a mock value
                return 1842; // Mock value
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting holders count");
                throw;
            }
        }
        
        public async Task<decimal> CalculateMarketCapAsync()
        {
            try
            {
                // Get circulating supply
                var circulatingSupply = await GetCirculatingSupplyAsync();
                
                // Get token price
                var tokenPrice = await GetTokenPriceAsync();
                
                // Calculate market cap
                return circulatingSupply * tokenPrice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating market cap");
                throw;
            }
        }
        
        // Helper method to convert from wei to decimal
        private decimal ConvertFromWei(BigInteger wei, int decimals)
        {
            return (decimal)wei / (decimal)Math.Pow(10, decimals);
        }
    }
}