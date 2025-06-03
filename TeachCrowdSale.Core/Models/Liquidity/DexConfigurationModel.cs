namespace TeachCrowdSale.Core.Models.Liquidity
{
    public class DexConfigurationModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public bool IsRecommended { get; set; }
        public decimal DefaultFeePercentage { get; set; }
        public string Network { get; set; } = string.Empty;
    }
}
