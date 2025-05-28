namespace TeachCrowdSale.Api.Models
{
    /// <summary>
    /// Presale statistics model
    /// </summary>
    public class PresaleStatsModel
    {
        public decimal TotalRaised { get; set; }
        public decimal FundingGoal { get; set; }
        public decimal TokensSold { get; set; }
        public decimal TokensRemaining { get; set; }
        public int ParticipantsCount { get; set; }
        public bool IsActive { get; set; }
        public decimal FundingProgress { get; set; }
    }
}
