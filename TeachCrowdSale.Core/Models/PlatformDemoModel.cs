// TeachCrowdSale.Core/Models/PlatformDemoModel.cs
namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Display model for PencilImpact demo page
    /// </summary>
    public class PlatformDemoModel
    {
        public List<DemoProjectModel> FeaturedProjects { get; set; } = new();
        public PencilDriveModel PencilDrive { get; set; } = new();
        public List<DemoFlowStep> DemoFlow { get; set; } = new();
        public List<InteractiveFeature> InteractiveFeatures { get; set; } = new();
    }

    public class DemoFlowStep
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public List<string> Features { get; set; } = new();
    }

    public class InteractiveFeature
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsInteractive { get; set; }
    }
}