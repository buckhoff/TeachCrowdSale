using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models.Request;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Api.Controllers
{
    [EnableRateLimiting("BuyTrade")]
    [ApiController]
    [Route("api/buytrade")]
    public class BuyTradeApiController : ControllerBase
    {
        private readonly IPresaleService _presaleService;
        private readonly IBlockchainService _blockchainService;
        private readonly ITokenContractService _tokenService;
        private readonly ILogger<BuyTradeApiController> _logger;

        public BuyTradeApiController(
            IPresaleService presaleService,
            IBlockchainService blockchainService,
            ITokenContractService tokenService,
            ILogger<BuyTradeApiController> logger)
        {
            _presaleService = presaleService ?? throw new ArgumentNullException(nameof(presaleService));
            _blockchainService = blockchainService ?? throw new ArgumentNullException(nameof(blockchainService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get comprehensive buy/trade page data
        /// </summary>
        [HttpGet("data")]
        [ResponseCache(Duration = 60)]
        public async Task<ActionResult<BuyTradeDataModel>> GetBuyTradeData()
        {
            try
            {
                var data = new BuyTradeDataModel();

                // Get current tier and all tiers - proper C# approach
                var currentTierTask = _presaleService.GetCurrentTierAsync();
                var allTiersTask = _presaleService.GetAllTiersAsync();
                var presaleStatsTask = _presaleService.GetPresaleStatsAsync();
                var tokenInfoTask = GetTokenInfoSafely();
                var contractAddressesTask = Task.FromResult(_blockchainService.GetContractAddresses());

                await Task.WhenAll(currentTierTask, allTiersTask, presaleStatsTask, tokenInfoTask, contractAddressesTask);

                var currentTier = await currentTierTask;
                var allTiers = await allTiersTask;
                var presaleStats = await presaleStatsTask;
                var tokenInfo = await tokenInfoTask;
                var contractAddresses = await contractAddressesTask;

                // Map current tier
                if (currentTier != null)
                {
                    data.CurrentTier = new TierDisplayModel
                    {
                        Id = currentTier.Id,
                        Name = GetTierName(currentTier.Id),
                        Price = currentTier.Price,
                        TotalAllocation = currentTier.Allocation,
                        Sold = currentTier.Sold,
                        Remaining = currentTier.Allocation - currentTier.Sold,
                        Progress = currentTier.Allocation > 0 ? (currentTier.Sold / currentTier.Allocation) * 100 : 0,
                        IsActive = currentTier.IsActive,
                        IsSoldOut = currentTier.Sold >= currentTier.Allocation,
                        MinPurchase = currentTier.MinPurchase,
                        MaxPurchase = currentTier.MaxPurchase,
                        VestingTGE = currentTier.VestingTGE,
                        VestingMonths = currentTier.VestingMonths,
                        EndTime = await _presaleService.GetTierEndTimeAsync(currentTier.Id),
                        Status = currentTier.IsActive ? "ACTIVE" : "UPCOMING",
                        StatusClass = currentTier.IsActive ? "active" : "upcoming"
                    };
                }

                // Map all tiers
                data.AllTiers = allTiers?.Select(tier => new TierDisplayModel
                {
                    Id = tier.Id,
                    Name = GetTierName(tier.Id),
                    Price = tier.Price,
                    TotalAllocation = tier.Allocation,
                    Sold = tier.Sold,
                    Remaining = tier.Allocation - tier.Sold,
                    Progress = tier.Allocation > 0 ? (tier.Sold / tier.Allocation) * 100 : 0,
                    IsActive = tier.IsActive,
                    IsSoldOut = tier.Sold >= tier.Allocation,
                    MinPurchase = tier.MinPurchase,
                    MaxPurchase = tier.MaxPurchase,
                    VestingTGE = tier.VestingTGE,
                    VestingMonths = tier.VestingMonths,
                    Status = tier.Sold >= tier.Allocation ? "SOLD OUT" :
                             tier.IsActive ? "ACTIVE" : "UPCOMING",
                    StatusClass = tier.Sold >= tier.Allocation ? "sold-out" :
                                  tier.IsActive ? "active" : "upcoming"
                }).ToList() ?? new List<TierDisplayModel>();

                // Map presale stats
                if (presaleStats != null)
                {
                    data.PresaleStats = new PresaleStatsModel
                    {
                        TotalRaised = presaleStats.TotalRaised,
                        FundingGoal = presaleStats.FundingGoal,
                        TokensSold = presaleStats.TokensSold,
                        TokensRemaining = presaleStats.TokensRemaining,
                        ParticipantsCount = presaleStats.ParticipantsCount,
                        IsActive = presaleStats.IsActive,
                        FundingProgress = presaleStats.FundingGoal > 0 ?
                            (presaleStats.TotalRaised / presaleStats.FundingGoal) * 100 : 0
                    };
                }

                // Map token info
                if (tokenInfo != null)
                {
                    data.TokenInfo = new TokenInfoModel
                    {
                        TotalSupply = tokenInfo.TotalSupply,
                        CirculatingSupply = tokenInfo.CirculatingSupply,
                        CurrentPrice = tokenInfo.CurrentPrice,
                        MarketCap = tokenInfo.MarketCap,
                        HoldersCount = tokenInfo.HoldersCount
                    };
                }

                // Map contract addresses
                data.ContractInfo = new ContractInfoModel
                {
                    PresaleAddress = contractAddresses.PresaleAddress,
                    TokenAddress = contractAddresses.TokenAddress,
                    PaymentTokenAddress = contractAddresses.PaymentTokenAddress,
                    NetworkId = contractAddresses.NetworkId,
                    ChainName = contractAddresses.ChainName
                };

                // Add purchase options
                data.PurchaseOptions = GetPurchaseOptions();

                // Add DEX integration info
                data.DexIntegrations = GetDexIntegrations();

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving buy/trade page data");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving buy/trade data",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get price calculation for purchase
        /// </summary>
        [HttpPost("calculate-price")]
        [ResponseCache(Duration = 30)]
        public async Task<ActionResult<PriceCalculationModel>> CalculatePrice([FromBody] PriceCalculationRequest request)
        {
            try
            {
                if (!_blockchainService.IsValidAddress(request.Address))
                {
                    return BadRequest(new ErrorResponse { Message = "Invalid Ethereum address" });
                }

                var tiers = await _presaleService.GetAllTiersAsync();
                var tier = tiers?.FirstOrDefault(t => t.Id == request.TierId);

                if (tier == null)
                {
                    return NotFound(new ErrorResponse { Message = $"Tier {request.TierId} not found" });
                }

                if (!tier.IsActive)
                {
                    return BadRequest(new ErrorResponse { Message = "Selected tier is not active" });
                }

                var tokensToReceive = request.UsdAmount / tier.Price;
                var fees = CalculateFees(request.UsdAmount);

                // Check if user has existing purchases
                var userPurchase = await GetUserPurchaseSafely(request.Address);

                var calculation = new PriceCalculationModel
                {
                    TierId = request.TierId,
                    TierName = GetTierName(request.TierId),
                    UsdAmount = request.UsdAmount,
                    TokenPrice = tier.Price,
                    TokensToReceive = tokensToReceive,
                    PlatformFee = fees.PlatformFee,
                    NetworkFee = fees.NetworkFee,
                    TotalFees = fees.TotalFees,
                    TotalCost = request.UsdAmount + fees.TotalFees,
                    MinPurchase = tier.MinPurchase,
                    MaxPurchase = tier.MaxPurchase,
                    UserExistingTokens = userPurchase?.Tokens ?? 0,
                    VestingInfo = new VestingInfoModel
                    {
                        TgePercentage = tier.VestingTGE,
                        VestingMonths = tier.VestingMonths,
                        TgeTokens = tokensToReceive * (tier.VestingTGE / 100m),
                        VestedTokens = tokensToReceive * ((100 - tier.VestingTGE) / 100m)
                    },
                    IsValid = request.UsdAmount >= tier.MinPurchase &&
                              request.UsdAmount <= tier.MaxPurchase &&
                              tokensToReceive <= (tier.Allocation - tier.Sold)
                };

                return Ok(calculation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating price for purchase");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error calculating purchase price",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get user's purchase history and claimable tokens
        /// </summary>
        [HttpGet("user/{address}")]
        public async Task<ActionResult<UserTradeInfoModel>> GetUserTradeInfo([FromRoute] string address)
        {
            try
            {
                if (!_blockchainService.IsValidAddress(address))
                {
                    return BadRequest(new ErrorResponse { Message = "Invalid Ethereum address" });
                }

                var userPurchase = await GetUserPurchaseSafely(address);
                var claimableTokens = await GetClaimableTokensSafely(address);
                var nextMilestone = await GetNextVestingMilestoneSafely(address);

                var userInfo = new UserTradeInfoModel
                {
                    Address = address,
                    TotalTokensPurchased = userPurchase?.Tokens ?? 0,
                    TotalUsdSpent = userPurchase?.UsdAmount ?? 0,
                    ClaimableTokens = claimableTokens,
                    LastClaimTime = userPurchase?.LastClaimTime,
                    NextVestingDate = nextMilestone?.Timestamp,
                    NextVestingAmount = nextMilestone?.Amount ?? 0,
                    TierPurchases = userPurchase?.TierAmounts?.Select((amount, index) => new TierPurchaseInfo
                    {
                        TierId = index,
                        TierName = GetTierName(index),
                        UsdAmount = amount
                    }).Where(tp => tp.UsdAmount > 0).ToList() ?? new List<TierPurchaseInfo>()
                };

                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user trade info for {Address}", address);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving user trade information",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get DEX trading pairs and liquidity info
        /// </summary>
        [HttpGet("dex-info")]
        [ResponseCache(Duration = 300)]
        public async Task<ActionResult<DexInfoModel>> GetDexInfo()
        {
            try
            {
                // This would integrate with actual DEX APIs in production
                var dexInfo = new DexInfoModel
                {
                    IsLive = false, // Will be true after TGE
                    LaunchDate = DateTime.UtcNow.AddDays(90), // Example launch date
                    TradingPairs = new List<TradingPairModel>
                    {
                        new TradingPairModel
                        {
                            PairName = "TEACH/USDC",
                            DexName = "Uniswap V3",
                            DexLogo = "/images/uniswap-logo.png",
                            Liquidity = 0, // Will be populated after launch
                            Volume24h = 0,
                            Price = await GetTokenPriceSafely(),
                            TradingUrl = "https://app.uniswap.org/swap?inputCurrency=USDC&outputCurrency=" // + token address
                        },
                        new TradingPairModel
                        {
                            PairName = "TEACH/ETH",
                            DexName = "SushiSwap",
                            DexLogo = "/images/sushiswap-logo.png",
                            Liquidity = 0,
                            Volume24h = 0,
                            Price = await GetTokenPriceSafely(),
                            TradingUrl = "https://app.sushi.com/swap"
                        }
                    },
                    UpcomingListings = new List<ExchangeListingModel>
                    {
                        new ExchangeListingModel
                        {
                            ExchangeName = "CoinGecko",
                            ExchangeLogo = "/images/coingecko-logo.png",
                            ListingType = "Tracking",
                            EstimatedDate = DateTime.UtcNow.AddDays(95),
                            Status = "Pending"
                        },
                        new ExchangeListingModel
                        {
                            ExchangeName = "CoinMarketCap",
                            ExchangeLogo = "/images/cmc-logo.png",
                            ListingType = "Tracking",
                            EstimatedDate = DateTime.UtcNow.AddDays(100),
                            Status = "Pending"
                        }
                    }
                };

                return Ok(dexInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving DEX information");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving DEX information",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        #region Private Helper Methods

        private async Task<Core.Models.TokenInfo?> GetTokenInfoSafely()
        {
            try
            {
                var totalSupply = await _tokenService.GetTotalSupplyAsync();
                var circulatingSupply = await _tokenService.GetCirculatingSupplyAsync();
                var currentPrice = await _tokenService.GetTokenPriceAsync();
                var marketCap = await _tokenService.CalculateMarketCapAsync();
                var holdersCount = await _tokenService.GetHoldersCountAsync();

                return new Core.Models.TokenInfo
                {
                    Name = "TeachToken",
                    Symbol = "TEACH",
                    Decimals = 18,
                    TotalSupply = totalSupply,
                    CirculatingSupply = circulatingSupply,
                    CurrentPrice = currentPrice,
                    MarketCap = marketCap,
                    HoldersCount = holdersCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get token info, using fallback");
                return null;
            }
        }

        private async Task<Core.Models.UserPurchase?> GetUserPurchaseSafely(string address)
        {
            try
            {
                return await _presaleService.GetUserPurchaseAsync(address);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get user purchase for {Address}", address);
                return null;
            }
        }

        private async Task<decimal> GetClaimableTokensSafely(string address)
        {
            try
            {
                return await _presaleService.GetClaimableTokensAsync(address);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get claimable tokens for {Address}", address);
                return 0;
            }
        }

        private async Task<Core.Models.VestingMilestone?> GetNextVestingMilestoneSafely(string address)
        {
            try
            {
                return await _presaleService.GetNextVestingMilestoneAsync(address);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get next vesting milestone for {Address}", address);
                return null;
            }
        }

        private async Task<decimal> GetTokenPriceSafely()
        {
            try
            {
                return await _tokenService.GetTokenPriceAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get token price, using tier price");
                var currentTier = await _presaleService.GetCurrentTierAsync();
                return currentTier?.Price ?? 0.06m;
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

        private FeeCalculationModel CalculateFees(decimal usdAmount)
        {
            // Simple fee calculation - in production, this would be more sophisticated
            var platformFeePercentage = 0.025m; // 2.5%
            var platformFee = usdAmount * platformFeePercentage;
            var networkFee = 5.0m; // Flat network fee in USD

            return new FeeCalculationModel
            {
                PlatformFee = platformFee,
                NetworkFee = networkFee,
                TotalFees = platformFee + networkFee
            };
        }

        private List<PurchaseOptionModel> GetPurchaseOptions()
        {
            return new List<PurchaseOptionModel>
            {
                new PurchaseOptionModel
                {
                    Method = "USDC",
                    Name = "USD Coin",
                    Logo = "/images/usdc-logo.png",
                    Description = "Purchase directly with USDC",
                    IsRecommended = true,
                    MinAmount = 100,
                    MaxAmount = 100000,
                    ProcessingTime = "Instant",
                    Fees = "2.5% + Network fees"
                },
                new PurchaseOptionModel
                {
                    Method = "ETH",
                    Name = "Ethereum",
                    Logo = "/images/eth-logo.png",
                    Description = "Purchase with ETH (converted to USDC)",
                    IsRecommended = false,
                    MinAmount = 0.05m,
                    MaxAmount = 50,
                    ProcessingTime = "1-2 minutes",
                    Fees = "3% + Network fees + Slippage"
                },
                new PurchaseOptionModel
                {
                    Method = "CARD",
                    Name = "Credit/Debit Card",
                    Logo = "/images/card-logo.png",
                    Description = "Purchase with credit or debit card",
                    IsRecommended = false,
                    MinAmount = 50,
                    MaxAmount = 5000,
                    ProcessingTime = "5-10 minutes",
                    Fees = "5% + Processing fees"
                }
            };
        }

        private List<DexIntegrationModel> GetDexIntegrations()
        {
            return new List<DexIntegrationModel>
            {
                new DexIntegrationModel
                {
                    Name = "1inch",
                    Logo = "/images/1inch-logo.png",
                    Description = "Best price aggregation across multiple DEXs",
                    WidgetUrl = "https://app.1inch.io/#/1/simple/swap/USDC/", // + token address
                    IsActive = false,
                    LaunchDate = DateTime.UtcNow.AddDays(90)
                },
                new DexIntegrationModel
                {
                    Name = "0x Protocol",
                    Logo = "/images/0x-logo.png",
                    Description = "Professional trading interface",
                    WidgetUrl = "https://matcha.xyz/markets/1/", // + token address
                    IsActive = false,
                    LaunchDate = DateTime.UtcNow.AddDays(90)
                }
            };
        }

        #endregion
    }
}