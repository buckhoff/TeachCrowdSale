namespace TeachCrowdSale.Web.Models;

public class PresaleTierModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public decimal TotalAllocation { get; set; }
    public decimal Sold { get; set; }
    public decimal MinPurchase { get; set; }
    public decimal MaxPurchase { get; set; }
    public int VestingTGE { get; set; }
    public int VestingMonths { get; set; }
    public bool IsActive { get; set; }
    public DateTime EndTime { get; set; }
    
    // Properties used in purchase.razor
    public decimal SoldPercentage => TotalAllocation > 0 ? (Sold / TotalAllocation) * 100 : 0;
    public decimal TokensRemaining => TotalAllocation - Sold;
    public string PresaleContractAddress { get; set; }
    public string PaymentTokenAddress { get; set; }
}