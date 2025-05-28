namespace TeachCrowdSale.Api.Models
{
    /// <summary>
    /// Tier display model for API
    /// </summary>
    public class TierDisplayModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal TotalAllocation { get; set; }
        public decimal Sold { get; set; }
        public decimal Remaining { get; set; }
        public decimal Progress { get; set; }
        public bool IsActive { get; set; }
        public bool IsSoldOut { get; set; }
        public decimal MinPurchase { get; set; }
        public decimal MaxPurchase { get; set; }
        public int VestingTGE { get; set; }
        public int VestingMonths { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
    }
}
