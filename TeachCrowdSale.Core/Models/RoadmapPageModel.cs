// TeachCrowdSale.Core/Models/RoadmapPageModel.cs
namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Main page model for roadmap dashboard
    /// </summary>
    public class RoadmapPageModel
    {
        public ProgressSummaryModel ProgressSummary { get; set; } = new();
        public List<MilestoneModel> ActiveMilestones { get; set; } = new();
        public List<MilestoneModel> UpcomingMilestones { get; set; } = new();
        public List<MilestoneModel> CompletedMilestones { get; set; } = new();
        public List<ReleaseModel> RecentReleases { get; set; } = new();
        public GitHubStatsModel GitHubStats { get; set; } = new();
        public DevelopmentStatsModel DevelopmentStats { get; set; } = new();
        public DateTime LastUpdated { get; set; }

        // Additional computed properties for display
        public string LastUpdatedFormatted => LastUpdated.ToString("MMM dd, yyyy 'at' h:mm tt UTC");
        public int TotalMilestones => ActiveMilestones.Count + UpcomingMilestones.Count + CompletedMilestones.Count;
        public bool HasActiveWork => ActiveMilestones.Any() || UpcomingMilestones.Any();
    }
}