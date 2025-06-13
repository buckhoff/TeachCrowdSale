// TeachCrowdSale.Core/Interfaces/IRoadmapService.cs
using TeachCrowdSale.Core.Models;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Core.Interfaces
{
    /// <summary>
    /// Service interface for roadmap and development operations
    /// </summary>
    public interface IRoadmapService
    {
        /// <summary>
        /// Get comprehensive roadmap page data
        /// </summary>
        Task<RoadmapDataModel> GetRoadmapDataAsync();

        /// <summary>
        /// Get all milestones with optional filtering
        /// </summary>
        Task<List<MilestoneDisplayModel>> GetMilestonesAsync(string? status = null, string? category = null);

        /// <summary>
        /// Get milestone by ID with full details
        /// </summary>
        Task<MilestoneDisplayModel?> GetMilestoneAsync(int id);

        /// <summary>
        /// Get development statistics
        /// </summary>
        Task<GitHubDevelopmentStatsModel> GetDevelopmentStatsAsync();

        /// <summary>
        /// Get recent development updates
        /// </summary>
        Task<List<UpdateDisplayModel>> GetRecentUpdatesAsync(int count = 10);

        /// <summary>
        /// Get releases information
        /// </summary>
        Task<List<ReleaseDisplayModel>> GetReleasesAsync();

        /// <summary>
        /// Get GitHub statistics
        /// </summary>
        Task<GitHubStatsModel> GetGitHubStatsAsync();

        /// <summary>
        /// Get roadmap filter options
        /// </summary>
        Task<RoadmapFilterModel> GetFilterOptionsAsync();

        /// <summary>
        /// Get tasks for a specific milestone
        /// </summary>
        Task<List<TaskDisplayModel>> GetMilestoneTasksAsync(int milestoneId);

        /// <summary>
        /// Search milestones by term
        /// </summary>
        Task<List<MilestoneDisplayModel>> SearchMilestonesAsync(string searchTerm);

        /// <summary>
        /// Get milestone dependencies
        /// </summary>
        Task<List<DependencyDisplayModel>> GetMilestoneDependenciesAsync(int milestoneId);

        /// <summary>
        /// Get development timeline data for charts
        /// </summary>
        Task<object> GetTimelineDataAsync();

        /// <summary>
        /// Get milestone progress over time
        /// </summary>
        Task<object> GetProgressHistoryAsync(int milestoneId);
    }
}
