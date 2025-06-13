using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Comprehensive roadmap page data model
    /// </summary>
    public class RoadmapDataModel
    {
        public List<MilestoneDisplayModel> CurrentMilestones { get; set; } = new();
        public List<MilestoneDisplayModel> UpcomingMilestones { get; set; } = new();
        public List<MilestoneDisplayModel> CompletedMilestones { get; set; } = new();
        public DevelopmentStatsModel DevelopmentStats { get; set; } = new();
        public List<ReleaseDisplayModel> Releases { get; set; } = new();
        public List<UpdateDisplayModel> RecentUpdates { get; set; } = new();
        public GitHubStats GitHubStats { get; set; } = new();
        public DateTime LoadedAt { get; set; } = DateTime.UtcNow;
    }
}