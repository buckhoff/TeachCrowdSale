namespace TeachCrowdSale.Api.Models;

public class PresaleStatusModel
{
    public decimal TotalRaised { get; set; }
    public decimal FundingGoal { get; set; }
    public int ParticipantsCount { get; set; }
    public decimal TokensSold { get; set; }
    public TierModel CurrentTier { get; set; }
}