using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Data.Entities;

namespace TeachCrowdSale.Core.Models.Configuration
{
    /// <summary>
    /// Configuration model for DEX integration settings
    /// </summary>
    public class DexIntegrationSettings
    {
        public List<DexConfiguration> SupportedDexes { get; set; } = new();
        public decimal MinLiquidityUSD { get; set; } = 1000;
        public decimal MaxLiquidityUSD { get; set; } = 1000000;
        public decimal DefaultSlippagePercentage { get; set; } = 1.0m;
        public LiquidityIncentivesConfig LiquidityIncentives { get; set; } = new();
    }
    public class DexConfiguration
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public decimal DefaultFeePercentage { get; set; } = 0.3m;
        public bool IsActive { get; set; } = true;
        public bool IsRecommended { get; set; }
        public int SortOrder { get; set; }
        public string Network { get; set; } = "Polygon";
        public int ChainId { get; set; } = 137;
        public string RouterAddress { get; set; } = string.Empty;
        public string FactoryAddress { get; set; } = string.Empty;
    }

    public class LiquidityIncentivesConfig
    {
        public bool Enabled { get; set; } = true;
        public decimal RewardAPYPercentage { get; set; } = 15.0m;
        public string MinimumLPTokens { get; set; } = "100";
        public int RewardDistributionIntervalHours { get; set; } = 24;
    }
}
