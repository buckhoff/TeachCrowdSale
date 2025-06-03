// TeachCrowdSale.Core/Models/Response/DependencyDisplayModel.cs
namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Dependency display model for UI
    /// </summary>
    public class DependencyDisplayModel
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DependsOnMilestoneId { get; set; }
        public string DependsOnMilestoneTitle { get; set; } = string.Empty;
        public string DependsOnMilestoneStatus { get; set; } = string.Empty;
        public bool IsBlocking { get; set; }
    }
}
