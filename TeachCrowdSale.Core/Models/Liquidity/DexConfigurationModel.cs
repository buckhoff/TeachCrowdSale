using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Liquidity
{
    /// <summary>
    /// Web display model for DEX configuration information
    /// Maps from DexConfigurationResponse for web layer consumption
    /// </summary>
    public class DexConfigurationModel
    {
        public int Id { get; set; }

        [Display(Name = "DEX Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Display Name")]
        public string DisplayName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Base URL")]
        public string BaseUrl { get; set; } = string.Empty;

        [Display(Name = "API URL")]
        public string ApiUrl { get; set; } = string.Empty;

        [Display(Name = "Logo URL")]
        public string LogoUrl { get; set; } = string.Empty;

        [Display(Name = "Default Fee Percentage")]
        [Range(0, 100)]
        public decimal DefaultFeePercentage { get; set; }

        [Display(Name = "Network")]
        public string Network { get; set; } = string.Empty;

        [EthereumAddress]
        [Display(Name = "Router Address")]
        public string? RouterAddress { get; set; }

        [EthereumAddress]
        [Display(Name = "Factory Address")]
        public string? FactoryAddress { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsRecommended { get; set; }
        public bool IsTestnet { get; set; }

        [Display(Name = "Sort Order")]
        [Range(0, int.MaxValue)]
        public int SortOrder { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Web UI Display Properties
        [Display(Name = "Status")]
        public string StatusText => IsActive ? "Active" : "Inactive";
        public string StatusClass => IsActive ? "status-active" : "status-inactive";

        [Display(Name = "Fee Display")]
        public string FeeDisplay => $"{DefaultFeePercentage:F3}%";

        [Display(Name = "Network Display")]
        public string NetworkDisplay => IsTestnet ? $"{Network} (Testnet)" : Network;
        public string NetworkClass => IsTestnet ? "network-testnet" : "network-mainnet";

        // Visual indicators
        public string RecommendedClass => IsRecommended ? "recommended" : "";
        public string RecommendedBadge => IsRecommended ? "⭐ Recommended" : "";

        // Feature availability
        public bool HasRouter => !string.IsNullOrEmpty(RouterAddress);
        public bool HasFactory => !string.IsNullOrEmpty(FactoryAddress);
        public bool HasLogo => !string.IsNullOrEmpty(LogoUrl);

        // URL validation and display
        public bool IsValidBaseUrl => Uri.TryCreate(BaseUrl, UriKind.Absolute, out _);
        public bool IsValidApiUrl => Uri.TryCreate(ApiUrl, UriKind.Absolute, out _);
        public bool IsValidLogoUrl => Uri.TryCreate(LogoUrl, UriKind.Absolute, out _);

        // Integration capabilities
        public bool SupportsDirectIntegration => HasRouter && HasFactory;
        public bool SupportsApiIntegration => IsValidApiUrl;

        // User experience properties
        public string TutorialUrl { get; set; } = string.Empty;
        public string DocumentationUrl { get; set; } = string.Empty;
        public List<string> SupportedFeatures { get; set; } = new();
        public List<string> NetworkRequirements { get; set; } = new();

        // Statistics (populated from pool data)
        public int PoolCount { get; set; }
        public decimal TotalTVL { get; set; }
        public decimal AverageAPY { get; set; }
        public decimal Volume24h { get; set; }

        // Display formatting for statistics
        public string PoolCountDisplay => $"{PoolCount} pools";
        public string TotalTVLDisplay => FormatCurrency(TotalTVL);
        public string AverageAPYDisplay => $"{AverageAPY:F2}%";
        public string Volume24hDisplay => FormatCurrency(Volume24h);

        // Ranking and recommendation logic
        public decimal PopularityScore { get; set; }
        public decimal SecurityScore { get; set; }
        public decimal LiquidityScore { get; set; }
        public decimal OverallScore { get; set; }

        public string PopularityClass => GetScoreClass(PopularityScore);
        public string SecurityClass => GetScoreClass(SecurityScore);
        public string LiquidityClass => GetScoreClass(LiquidityScore);
        public string OverallClass => GetScoreClass(OverallScore);

        // Risk assessment
        public string RiskLevel { get; set; } = "Medium"; // Low, Medium, High
        public string RiskLevelClass => $"risk-{RiskLevel.ToLower()}";
        public List<string> RiskFactors { get; set; } = new();
        public List<string> SecurityFeatures { get; set; } = new();

        // User guidance
        public bool IsRecommendedForBeginners { get; set; }
        public string BeginnerRecommendationReason { get; set; } = string.Empty;
        public string AdvancedFeatures { get; set; } = string.Empty;

        // Mobile/responsive properties
        public bool HasMobileApp { get; set; }
        public string MobileAppUrl { get; set; } = string.Empty;
        public bool HasResponsiveDesign { get; set; } = true;

        // Integration links for liquidity addition
        public string AddLiquidityUrl { get; set; } = string.Empty;
        public string PoolCreationUrl { get; set; } = string.Empty;
        public string SwapUrl { get; set; } = string.Empty;

        // Helper methods
        private static string FormatCurrency(decimal amount)
        {
            return amount switch
            {
                >= 1_000_000_000 => $"${amount / 1_000_000_000:F2}B",
                >= 1_000_000 => $"${amount / 1_000_000:F2}M",
                >= 1_000 => $"${amount / 1_000:F2}K",
                _ => $"${amount:F2}"
            };
        }

        private static string GetScoreClass(decimal score)
        {
            return score switch
            {
                >= 80 => "score-excellent",
                >= 60 => "score-good",
                >= 40 => "score-fair",
                _ => "score-poor"
            };
        }

        // Create URL for specific liquidity pool
        public string GetPoolUrl(string token0Address, string token1Address)
        {
            if (string.IsNullOrEmpty(AddLiquidityUrl)) return string.Empty;

            return AddLiquidityUrl
                .Replace("{token0}", token0Address)
                .Replace("{token1}", token1Address);
        }

        // Create URL for token swap
        public string GetSwapUrl(string inputToken, string outputToken)
        {
            if (string.IsNullOrEmpty(SwapUrl)) return string.Empty;

            return SwapUrl
                .Replace("{inputToken}", inputToken)
                .Replace("{outputToken}", outputToken);
        }

        // Validation for contract addresses
        public bool ValidateRouterAddress()
        {
            if (string.IsNullOrEmpty(RouterAddress)) return false;
            return RouterAddress.StartsWith("0x") && RouterAddress.Length == 42;
        }

        public bool ValidateFactoryAddress()
        {
            if (string.IsNullOrEmpty(FactoryAddress)) return false;
            return FactoryAddress.StartsWith("0x") && FactoryAddress.Length == 42;
        }
    }
}