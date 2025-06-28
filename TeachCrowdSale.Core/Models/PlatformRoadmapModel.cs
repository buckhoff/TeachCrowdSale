// TeachCrowdSale.Core/Models/PlatformRoadmapModel.cs
namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Display model for PencilImpact roadmap page
    /// </summary>
    public class PlatformRoadmapModel
    {
        public string CurrentPhase { get; set; } = string.Empty;
        public List<DevelopmentPhase> Phases { get; set; } = new();
        public List<RoadmapMilestone> Milestones { get; set; } = new();
        public List<TokenIntegrationPhase> TokenIntegration { get; set; } = new();
    }

    public class DevelopmentPhase
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Timeline { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<string> Deliverables { get; set; } = new();
    }

    public class RoadmapMilestone
    {
        public string Title { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class TokenIntegrationPhase
    {
        public string Phase { get; set; } = string.Empty;
        public List<string> Features { get; set; } = new();
        public string Timeline { get; set; } = string.Empty;
    }
}