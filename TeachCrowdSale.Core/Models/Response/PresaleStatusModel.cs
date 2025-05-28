using System.ComponentModel.DataAnnotations;

namespace TeachCrowdSale.Core.Models.Response;

public class PresaleStatusModel
{
    [Range(0, double.MaxValue)]
    public decimal TotalRaised { get; set; }
    [Range(0, double.MaxValue)]
    public decimal FundingGoal { get; set; }
    [Range(0, int.MaxValue)]
    public int ParticipantsCount { get; set; }
    [Range(0, double.MaxValue)]
    public decimal TokensSold { get; set; }
    public TierModel CurrentTier { get; set; }
}