namespace TeachCrowdSale.Core.Models;

public class SaleTier
{
    public int Id { get; set; }
    public decimal Price { get; set; }
    public decimal Allocation { get; set; }
    public decimal Sold { get; set; }
    public decimal MinPurchase { get; set; }
    public decimal MaxPurchase { get; set; }
    public int VestingTGE { get; set; }
    public int VestingMonths { get; set; }
    public bool IsActive { get; set; }
}