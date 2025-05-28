namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Current tier model
    /// </summary>
    public class CurrentTierModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Progress { get; set; }
        public decimal Sold { get; set; }
        public decimal Remaining { get; set; }
        public bool IsActive { get; set; }
    }
}
