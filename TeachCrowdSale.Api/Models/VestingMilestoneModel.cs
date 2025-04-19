using System.ComponentModel.DataAnnotations;

namespace TeachCrowdSale.Api.Models;

public class VestingMilestoneModel
{
    public DateTime Timestamp { get; set; }
    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }
    public string FormattedDate { get; set; }
}