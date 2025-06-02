using System.ComponentModel.DataAnnotations;

namespace TeachCrowdSale.Core.Models.Response;

public class VestingMilestoneResponse
{
    public DateTime Timestamp { get; set; }
    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }
    public string FormattedDate { get; set; }
}