namespace TeachCrowdSale.Api.Models;

public class TierModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public decimal MinPurchase { get; set; }
    public decimal MaxPurchase { get; set; }
    public decimal TotalAllocation { get; set; }
    public decimal Sold { get; set; }
    public int VestingTGE { get; set; }
    public int VestingMonths { get; set; }
    public bool IsActive { get; set; }
    public DateTime EndTime { get; set; }
}