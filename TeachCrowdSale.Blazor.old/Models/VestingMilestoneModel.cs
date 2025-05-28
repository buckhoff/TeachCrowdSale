namespace TeachCrowdSale.Web.Models;

public class VestingMilestoneModel
{
    public DateTime Timestamp { get; set; }
    public decimal Amount { get; set; }
    public string FormattedDate { get; set; }
}