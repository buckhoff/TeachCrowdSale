using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models.Request;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Web.Controllers
{
    [Route("buy")]
    [Route("trade")]
    public class BuyTradeController : Controller
    {
        private readonly IBuyTradeService _buyTradeService;
        private readonly ILogger<BuyTradeController> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public BuyTradeController(
            IBuyTradeService buyTradeService,
            ILogger<BuyTradeController> logger)
        {
            _buyTradeService = buyTradeService ?? throw new ArgumentNullException(nameof(buyTradeService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Main buy/trade page route
        /// </summary>
        [HttpGet("")]
        [HttpGet("buy")]
        [HttpGet("trade")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var buyTradeData = await _buyTradeService.GetBuyTradeDataAsync();

                ViewBag.InitialData = buyTradeData;
                ViewBag.JsonData = JsonSerializer.Serialize(buyTradeData, _jsonOptions);

                // Set page-specific metadata
                ViewData["Title"] = "Buy TEACH Token - Direct Presale & DEX Trading";
                ViewData["Description"] = "Purchase TEACH tokens directly from our presale or trade on decentralized exchanges. Multi-year treasury runway with early investor pricing.";

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading buy/trade page");

                var fallbackData = _buyTradeService.GetFallbackBuyTradeData();
                ViewBag.InitialData = fallbackData;
                ViewBag.JsonData = JsonSerializer.Serialize(fallbackData, _jsonOptions);

                return View();
            }
        }

        /// <summary>
        /// AJAX endpoint for getting aggregated buy/trade page data
        /// </summary>
        [HttpGet("GetBuyTradeData")]
        [ResponseCache(Duration = 30)]
        public async Task<IActionResult> GetBuyTradeData()
        {
            try
            {
                var data = await _buyTradeService.GetBuyTradeDataAsync();
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving buy/trade page data");

                var fallbackData = _buyTradeService.GetFallbackBuyTradeData();
                return Json(fallbackData);
            }
        }

        /// <summary>
        /// AJAX endpoint for price calculation
        /// </summary>
        [HttpPost("CalculatePrice")]
        public async Task<ActionResult<PriceCalculationModel>> CalculatePrice([FromBody] PriceCalculationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid request data",
                        ValidationErrors = ModelState.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                        )
                    });
                }

                var calculation = await _buyTradeService.CalculatePriceAsync(
                    request.Address,
                    request.TierId,
                    request.UsdAmount);

                return Json(calculation);
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
        /// AJAX endpoint for user trade information
        /// </summary>
        [HttpGet("GetUserTradeInfo/{address}")]
        [ResponseCache(Duration = 60)]
        public async Task<ActionResult<UserTradeInfoModel>> GetUserTradeInfo([FromRoute] string address)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(address) || !IsValidEthereumAddress(address))
                {
                    return BadRequest(new ErrorResponse { Message = "Invalid Ethereum address" });
                }

                var userInfo = await _buyTradeService.GetUserTradeInfoAsync(address);
                return Json(userInfo);
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
        /// AJAX endpoint for DEX information
        /// </summary>
        [HttpGet("GetDexInfo")]
        [ResponseCache(Duration = 300)]
        public async Task<ActionResult<DexInfoModel>> GetDexInfo()
        {
            try
            {
                var dexInfo = await _buyTradeService.GetDexInfoAsync();
                return Json(dexInfo);
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

        /// <summary>
        /// AJAX endpoint for purchase validation
        /// </summary>
        [HttpPost("ValidatePurchase")]
        public async Task<ActionResult<PurchaseValidationModel>> ValidatePurchase([FromBody] PriceCalculationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid request data",
                        ValidationErrors = ModelState.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                        )
                    });
                }

                var validation = await _buyTradeService.ValidatePurchaseAsync(
                    request.Address,
                    request.TierId,
                    request.UsdAmount);

                return Json(validation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating purchase");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error validating purchase",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// AJAX endpoint for wallet connection helper
        /// </summary>
        [HttpGet("GetWalletInfo/{address}")]
        [ResponseCache(Duration = 30)]
        public async Task<ActionResult<WalletInfoModel>> GetWalletInfo([FromRoute] string address)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(address) || !IsValidEthereumAddress(address))
                {
                    return BadRequest(new ErrorResponse { Message = "Invalid Ethereum address" });
                }

                var walletInfo = await _buyTradeService.GetWalletInfoAsync(address);
                return Json(walletInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving wallet info for {Address}", address);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving wallet information",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// AJAX endpoint for service health check
        /// </summary>
        [HttpGet("HealthCheck")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                var isHealthy = await _buyTradeService.CheckServiceHealthAsync();
                return Json(new
                {
                    status = isHealthy ? "healthy" : "degraded",
                    timestamp = DateTime.UtcNow,
                    version = "1.0.0"
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Health check failed");
                return Json(new
                {
                    status = "degraded",
                    message = "Health check failed",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Validate Ethereum address format
        /// </summary>
        private bool IsValidEthereumAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return false;

            // Check if it starts with 0x and is 42 characters long
            if (!address.StartsWith("0x") || address.Length != 42)
                return false;

            // Check if the remaining 40 characters are valid hex
            return System.Text.RegularExpressions.Regex.IsMatch(
                address.Substring(2),
                "^[0-9a-fA-F]{40}$");
        }

        #endregion
    }
}