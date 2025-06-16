// TeachCrowdSale.Core/Models/Response/DependencyDisplayModel.cs
using TeachCrowdSale.Core.Data.Enum;
using TeachCrowdSale.Core.Helper;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Dependency display model for UI
    /// </summary>
    public class DependencyModel
    {
        public int Id { get; set; }
        public int MilestoneId { get; set; }
        public int DependentMilestoneId { get; set; }
        public string DependencyType { get; set; } = string.Empty;
        public string DependencyTypeClass { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string MilestoneTitle { get; set; } = string.Empty;
        public string DependentMilestoneTitle { get; set; } = string.Empty;

        // Display helper properties
        public string DependencyText => $"{MilestoneTitle} → {DependentMilestoneTitle}";
        public string DependencyTypeIcon => DisplayHelpers.GetDependencyTypeIcon(DependencyType);
        public string StatusClass => IsActive ? "active" : "inactive";
    }
}
