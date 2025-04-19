using System.ComponentModel.DataAnnotations;

namespace TeachCrowdSale.Core.Models;

public class VestingMilestone
{
    public DateTime Timestamp { get; set; }
    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }
}