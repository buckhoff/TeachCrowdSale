namespace TeachCrowdSale.Api.Models
{
    /// <summary>
    /// Live statistics model for frequent updates
    /// </summary>
    public class LiveStatsModel
    {
        public decimal TotalRaised { get; set; }
        public decimal TokensSold { get; set; }
        public int ParticipantsCount { get; set; }
        public decimal TokenPrice { get; set; }
        public decimal MarketCap { get; set; }
        public int HoldersCount { get; set; }
        public decimal CurrentTierPrice { get; set; }
        public string CurrentTierName { get; set; } = string.Empty;
        public decimal CurrentTierProgress { get; set; }
        public bool IsPresaleActive { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
