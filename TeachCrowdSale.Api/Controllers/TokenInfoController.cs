using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachCrowdSale.Core.Interfaces;
using Microsoft.Extensions.Logging;
using TeachCrowdSale.Api.Models;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/tokeninfo")]
    public class TokenInfoController : ControllerBase
    {
        private readonly ITokenContractService _tokenService;
        private readonly ILogger<TokenInfoController> _logger;
        
        public TokenInfoController(ITokenContractService tokenService,ILogger<TokenInfoController> logger)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        [HttpGet]
        public async Task<ActionResult<TokenInfoModel>> GetTokenInfo([FromHeader(Name = "X-API-Key")] string apiKey)
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
                _logger.LogError(ex, "Error retrieving token information");
                return StatusCode(500, new ErrorResponse 
                { 
                    Message = "An error occurred while retrieving token information",
                    TraceId = HttpContext.TraceIdentifier
                });
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
                _logger.LogError(ex, "Error retrieving token price");
                return StatusCode(500, new ErrorResponse 
                { 
                    Message = "An error occurred while retrieving token price",
                    TraceId = HttpContext.TraceIdentifier
                });
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
                _logger.LogError(ex, "Error retrieving token supply information");
                return StatusCode(500, new ErrorResponse 
                { 
                    Message = "An error occurred while retrieving token supply information",
                    TraceId = HttpContext.TraceIdentifier
                });
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
                
                var volume24h = await _tokenService.GetVolume24hAsync();
                var priceChange24h = await _tokenService.GetPriceChange24hAsync();
                
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
                _logger.LogError(ex, "Error retrieving market data");
                return StatusCode(500, new ErrorResponse 
                { 
                    Message = "An error occurred while retrieving market data",
                    TraceId = HttpContext.TraceIdentifier
                });
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
                
                var burnedTokens = await _tokenService.GetBurnedTokensAsync();
                var stakedTokens = await _tokenService.GetStakedTokensAsync();
                var liquidityTokens = await _tokenService.GetLiquidityTokensAsync();
                
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
}