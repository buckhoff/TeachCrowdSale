using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Response model for complete roadmap data
    /// </summary>
    public class RoadmapResponse
    {
        public ProgressSummaryResponse? ProgressSummary { get; set; }
        public List<MilestoneResponse>? ActiveMilestones { get; set; }
        public List<MilestoneResponse>? UpcomingMilestones { get; set; }
        public List<MilestoneResponse>? CompletedMilestones { get; set; }
        public List<ReleaseResponse>? RecentReleases { get; set; }
        public GitHubStatsResponse? GitHubStats { get; set; }
        public DevelopmentStatsResponse? DevelopmentStats { get; set; }
        public DateTime LastUpdated { get; set; }
    }

}
