using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Models;

namespace TeachCrowdSale.Core.Interfaces.Services
{
    /// <summary>
    /// Web service for roadmap dashboard operations, calling API endpoints via HttpClient
    /// </summary>
    public interface IRoadmapDashboardService
    {
        Task<RoadmapPageModel> GetRoadmapPageDataAsync();
        Task<MilestoneModel?> GetMilestoneDetailsAsync(int milestoneId);
        Task<IEnumerable<MilestoneModel>> GetFilteredMilestonesAsync(string? status, string? category);
        Task<ProgressSummaryModel> GetProgressSummaryAsync();
        Task<GitHubStatsModel> GetGitHubStatsAsync();
        Task<IEnumerable<ReleaseModel>> GetRecentReleasesAsync(int count = 5);
        Task<DevelopmentStatsModel> GetDevelopmentStatsAsync();
    }
}
