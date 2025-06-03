using TeachCrowdSale.Core.Data.Enum;

// TeachCrowdSale.Core/Data/Entities/Dependency.cs
namespace TeachCrowdSale.Core.Data.Entities
{
    public class Dependency
    {
        public int Id { get; set; }

        public int MilestoneId { get; set; }

        public int DependsOnMilestoneId { get; set; }

        public DependencyType Type { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Milestone Milestone { get; set; } = null!;
        public Milestone DependsOnMilestone { get; set; } = null!;
    }
}
  