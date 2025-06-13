using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{ 
 /// <summary>
 /// Main roadmap page data container for frontend display
 /// </summary>
    public class RoadmapPageModel
{
    public RoadmapOverviewModel Overview { get; set; } = new();
    public List<MilestoneDisplayModel> ActiveMilestones { get; set; } = new();
    public List<MilestoneDisplayModel> UpcomingMilestones { get; set; } = new();
    public List<MilestoneDisplayModel> CompletedMilestones { get; set; } = new();
    public DevelopmentStatsModel DevelopmentStats { get; set; } = new();
    public List<UpdateDisplayModel> RecentUpdates { get; set; } = new();
    public List<ReleaseDisplayModel> Releases { get; set; } = new();
    public GitHubStats GitHubStats { get; set; } = new();
    public RoadmapFilterModel FilterOptions { get; set; } = new();
    public DateTime LoadedAt { get; set; } = DateTime.UtcNow;

    // Page-specific properties
    public string PageTitle { get; set; } = "Platform Roadmap & Development - TeachToken";
    public string PageDescription { get; set; } = "Track TeachToken development progress, milestones, and upcoming features. See our commitment to transparent development and education innovation.";
    public string PageKeywords { get; set; } = "roadmap, development, milestones, progress, TeachToken, blockchain, education, platform";
    public bool HasErrors { get; set; }
    public List<string> ErrorMessages { get; set; } = new();
}
}
