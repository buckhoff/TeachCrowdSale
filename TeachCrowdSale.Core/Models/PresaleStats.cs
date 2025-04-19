using System.ComponentModel.DataAnnotations;

namespace TeachCrowdSale.Core.Models;

public class PresaleStats
{
    [Range(0, double.MaxValue)]
    public decimal TotalRaised { get; set; }
    [Range(0, double.MaxValue)]
    public decimal FundingGoal { get; set; }
    [Range(0, int.MaxValue)]
    public int ParticipantsCount { get; set; }
    [Range(0, double.MaxValue)]
    public decimal TokensSold { get; set; }
    [Range(0, double.MaxValue)]
    public decimal TokensRemaining { get; set; }
    public DateTime PresaleStartTime { get; set; }
    public DateTime PresaleEndTime { get; set; }
    public bool IsActive { get; set; }
}