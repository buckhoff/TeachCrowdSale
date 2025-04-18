using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Api.Models;

namespace TeachCrowdSale.Api.Controllers
{
    [ApiController]
    [Route("api/tokeninfo")]
    public class TokenInfoController : ControllerBase
    {
        private readonly ITokenContractService _tokenService;
        
        public TokenInfoController(ITokenContractService tokenService)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }
        
        [HttpGet]
        public async Task<ActionResult<TokenInfoModel>> GetTokenInfo()
        {
            try
            {
                var totalSupply = await _tokenService.GetTotalSupplyAsync();
                var circulatingSupply = await _tokenService.GetCirculatingSupplyAsync();
                var tokenPrice = await _tokenService.GetTokenPriceAsync();
                var holdersCount = await _tokenService.GetHoldersCountAsync();
                var marketCap = await _tokenService.CalculateMarketCapAsync();
                
                return Ok(new TokenInfoModel
                {
                    TotalSupply = totalSupply,
                    CirculatingSupply = circulatingSupply,
                    CurrentPrice = tokenPrice,
                    MarketCap = marketCap,
                    HoldersCount = holdersCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving token information: {ex.Message}");
            }
        }
        
        [HttpGet("price")]
        public async Task<ActionResult<decimal>> GetTokenPrice()
        {
            try
            {
                var tokenPrice = await _tokenService.GetTokenPriceAsync();
                return Ok(tokenPrice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving token price: {ex.Message}");
            }
        }
        
        [HttpGet("supply")]
        public async Task<ActionResult<SupplyModel>> GetTokenSupply()
        {
            try
            {
                var totalSupply = await _tokenService.GetTotalSupplyAsync();
                var circulatingSupply = await _tokenService.GetCirculatingSupplyAsync();
                
                return Ok(new SupplyModel
                {
                    TotalSupply = totalSupply,
                    CirculatingSupply = circulatingSupply,
                    PercentCirculating = totalSupply > 0 ? (circulatingSupply / totalSupply) * 100 : 0
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving token supply information: {ex.Message}");
            }
        }
        
        [HttpGet("market")]
        public async Task<ActionResult<MarketDataModel>> GetMarketData()
        {
            try
            {
                var tokenPrice = await _tokenService.GetTokenPriceAsync();
                var marketCap = await _tokenService.CalculateMarketCapAsync();
                var holdersCount = await _tokenService.GetHoldersCountAsync();
                
                // For demonstration purposes, we're generating some mock data
                // In a real application, these would come from exchanges or price APIs
                var priceChange24h = -2.5m; // Mock 24h price change percentage
                var volume24h = 950000m; // Mock 24h trading volume
                
                return Ok(new MarketDataModel
                {
                    CurrentPrice = tokenPrice,
                    MarketCap = marketCap,
                    PriceChange24h = priceChange24h,
                    Volume24h = volume24h,
                    HoldersCount = holdersCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving market data: {ex.Message}");
            }
        }
        
        [HttpGet("stats")]
        public async Task<ActionResult<TokenStatsModel>> GetTokenStats()
        {
            try
            {
                var totalSupply = await _tokenService.GetTotalSupplyAsync();
                var circulatingSupply = await _tokenService.GetCirculatingSupplyAsync();
                var tokenPrice = await _tokenService.GetTokenPriceAsync();
                var holdersCount = await _tokenService.GetHoldersCountAsync();
                var marketCap = await _tokenService.CalculateMarketCapAsync();
                
                // For demonstration purposes, we're using mock values for the additional stats
                var burnedTokens = 75000000m; // 75M tokens burned
                var stakedTokens = 350000000m; // 350M tokens staked
                var liquidityTokens = 225000000m; // 225M tokens in liquidity pools
                
                return Ok(new TokenStatsModel
                {
                    TotalSupply = totalSupply,
                    CirculatingSupply = circulatingSupply,
                    CurrentPrice = tokenPrice,
                    MarketCap = marketCap,
                    HoldersCount = holdersCount,
                    BurnedTokens = burnedTokens,
                    StakedTokens = stakedTokens,
                    LiquidityTokens = liquidityTokens
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving token statistics: {ex.Message}");
            }
        }
    }
    
    // Additional models for specialized endpoints
    public class SupplyModel
    {
        public decimal TotalSupply { get; set; }
        public decimal CirculatingSupply { get; set; }
        public decimal PercentCirculating { get; set; }
    }
    
    public class MarketDataModel
    {
        public decimal CurrentPrice { get; set; }
        public decimal MarketCap { get; set; }
        public decimal PriceChange24h { get; set; }
        public decimal Volume24h { get; set; }
        public int HoldersCount { get; set; }
    }
    
    public class TokenStatsModel
    {
        public decimal TotalSupply { get; set; }
        public decimal CirculatingSupply { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal MarketCap { get; set; }
        public int HoldersCount { get; set; }
        public decimal BurnedTokens { get; set; }
        public decimal StakedTokens { get; set; }
        public decimal LiquidityTokens { get; set; }
    }
    
    public class TokenInfoModel
    {
        public decimal TotalSupply { get; set; }
        public decimal CirculatingSupply { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal MarketCap { get; set; }
        public int HoldersCount { get; set; }
    }
}