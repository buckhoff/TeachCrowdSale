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
        Task<RoadmapDataModel> GetRoadmapPageDataAsync();
        Task<List<MilestoneDisplayModel>> GetMilestonesAsync(string? status = null, string? category = null);
        Task<MilestoneDisplayModel?> GetMilestoneDetailsAsync(int id);
        Task<DevelopmentStatsModel> GetDevelopmentStatsAsync();
        Task<List<UpdateDisplayModel>> GetRecentUpdatesAsync(int count = 10);
        Task<List<ReleaseDisplayModel>> GetReleasesAsync();
        Task<GitHubStats> GetGitHubStatsAsync();
        Task<RoadmapFilterModel> GetFilterOptionsAsync();
        Task<List<TaskDisplayModel>> GetMilestoneTasksAsync(int milestoneId);
        Task<List<MilestoneDisplayModel>> SearchMilestonesAsync(string searchTerm);
        Task<List<DependencyDisplayModel>> GetMilestoneDependenciesAsync(int milestoneId);
        Task<object> GetTimelineDataAsync();
        Task<object> GetProgressHistoryAsync(int milestoneId);
        Task<bool> CheckServiceHealthAsync();
    }
}
