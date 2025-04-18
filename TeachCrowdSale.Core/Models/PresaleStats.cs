namespace TeachCrowdSale.Core.Models;

public class PresaleStats
{
    public decimal TotalRaised { get; set; }
    public decimal FundingGoal { get; set; }
    public int ParticipantsCount { get; set; }
    public decimal TokensSold { get; set; }
    public decimal TokensRemaining { get; set; }
    public DateTime PresaleStartTime { get; set; }
    public DateTime PresaleEndTime { get; set; }
    public bool IsActive { get; set; }
}