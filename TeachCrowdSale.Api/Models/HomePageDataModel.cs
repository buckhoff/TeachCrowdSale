namespace TeachCrowdSale.Api.Models
{
    /// <summary>
    /// Comprehensive home page data model
    /// </summary>
    public class HomePageDataModel
    {
        public PresaleStatsModel PresaleStats { get; set; } = new();
        public CurrentTierModel CurrentTier { get; set; } = new();
        public TokenInfoModel TokenInfo { get; set; } = new();
        public List<InvestmentHighlightModel> InvestmentHighlights { get; set; } = new();
        public DateTime LoadedAt { get; set; } = DateTime.UtcNow;
    }
}
