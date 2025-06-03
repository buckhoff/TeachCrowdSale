// TeachCrowdSale.Core/Interfaces/IRoadmapService.cs

// TeachCrowdSale.Core/Interfaces/IRoadmapService.cs
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Models;

namespace TeachCrowdSale.Core.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for roadmap data access
    /// </summary>
    public interface IRoadmapRepository
    {
        // Milestone operations
        Task<List<Milestone>> GetMilestonesAsync(string? status = null, string? category = null);
        Task<Milestone?> GetMilestoneAsync(int id);
        Task<Milestone> CreateMilestoneAsync(Milestone milestone);
        Task<Milestone> UpdateMilestoneAsync(Milestone milestone);
        System.Threading.Tasks.Task DeleteMilestoneAsync(int id);

        // Task operations
        Task<List<Data.Entities.Task>> GetTasksAsync(int milestoneId);
        Task<Data.Entities.Task?> GetTaskAsync(int id);
        Task<Data.Entities.Task> CreateTaskAsync(Data.Entities.Task task);
        Task<Data.Entities.Task> UpdateTaskAsync(Data.Entities.Task task);
        System.Threading.Tasks.Task DeleteTaskAsync(int id);

        // Update operations
        Task<List<Update>> GetUpdatesAsync(int? milestoneId = null, int count = 10);
        Task<Update> CreateUpdateAsync(Update update);

        // Release operations
        Task<List<Release>> GetReleasesAsync();
        Task<Release?> GetReleaseAsync(int id);
        Task<Release> CreateReleaseAsync(Release release);
        Task<Release> UpdateReleaseAsync(Release release);

        // Dependency operations
        Task<List<Dependency>> GetDependenciesAsync(int milestoneId);
        Task<Dependency> CreateDependencyAsync(Dependency dependency);
        System.Threading.Tasks.Task DeleteDependencyAsync(int id);

        // Statistics operations
        Task<DevelopmentStatsData> GetDevelopmentStatsAsync();
        Task<List<ProgressHistoryData>> GetProgressHistoryAsync(int milestoneId);

        // Search operations
        Task<List<Milestone>> SearchMilestonesAsync(string searchTerm);
        Task<List<string>> GetCategoriesAsync();
        Task<List<string>> GetDevelopersAsync();
    }
}
