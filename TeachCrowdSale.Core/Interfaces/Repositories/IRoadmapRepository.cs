using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Data.Enum;
using TaskStatus = TeachCrowdSale.Core.Data.Enum.TaskStatus;

namespace TeachCrowdSale.Infrastructure.Repositories
{
    /// <summary>
    /// Repository interface for Roadmap-related data access
    /// Works directly with entities and database operations
    /// </summary>
    public interface IRoadmapRepository
    {
        #region Milestone Operations

        // Read operations
        Task<IEnumerable<Milestone>> GetAllMilestonesAsync();
        Task<IEnumerable<Milestone>> GetMilestonesByStatusAsync(MilestoneStatus status);
        Task<IEnumerable<Milestone>> GetFilteredMilestonesAsync(string? status, string? category, string? priority);
        Task<Milestone?> GetMilestoneByIdAsync(int milestoneId);
        Task<Milestone?> GetMilestoneWithDetailsAsync(int milestoneId);
        Task<IEnumerable<Milestone>> GetMilestonesByPriorityAsync(MilestonePriority priority);
        Task<IEnumerable<Milestone>> GetMilestonesByCategoryAsync(string category);
        Task<IEnumerable<Milestone>> GetOverdueMilestonesAsync();
        Task<IEnumerable<Milestone>> GetUpcomingMilestonesAsync(int days = 30);

        // Write operations
        Task<Milestone> CreateMilestoneAsync(Milestone milestone);
        Task<Milestone> UpdateMilestoneAsync(Milestone milestone);
        Task<bool> DeleteMilestoneAsync(int milestoneId);
        Task<bool> UpdateMilestoneProgressAsync(int milestoneId, decimal progressPercentage);
        Task<bool> UpdateMilestoneStatusAsync(int milestoneId, MilestoneStatus status);

        #endregion

        #region Task Operations

        // Read operations
        Task<IEnumerable<Core.Data.Entities.Task>> GetAllTasksAsync();
        Task<IEnumerable<Core.Data.Entities.Task>> GetTasksByMilestoneAsync(int milestoneId);
        Task<IEnumerable<Core.Data.Entities.Task>> GetTasksByStatusAsync(TaskStatus status);
        Task<IEnumerable<Core.Data.Entities.Task>> GetTasksByAssigneeAsync(string assignee);
        Task<Core.Data.Entities.Task?> GetTaskByIdAsync(int taskId);
        Task<IEnumerable<Core.Data.Entities.Task>> GetOverdueTasksAsync();
        Task<IEnumerable<Core.Data.Entities.Task>> GetTasksByPriorityAsync(MilestonePriority priority);

        // Write operations
        Task<Core.Data.Entities.Task> CreateTaskAsync(Core.Data.Entities.Task task);
        Task<Core.Data.Entities.Task> UpdateTaskAsync(Core.Data.Entities.Task task);
        Task<bool> DeleteTaskAsync(int taskId);
        Task<bool> UpdateTaskProgressAsync(int taskId, decimal progressPercentage);
        Task<bool> UpdateTaskStatusAsync(int taskId, TaskStatus status);
        Task<bool> AssignTaskAsync(int taskId, string assignee);

        #endregion

        #region Update Operations

        // Read operations
        Task<IEnumerable<Update>> GetAllUpdatesAsync();
        Task<IEnumerable<Update>> GetUpdatesByMilestoneAsync(int milestoneId);
        Task<IEnumerable<Update>> GetUpdatesByTypeAsync(UpdateType updateType);
        Task<IEnumerable<Update>> GetUpdatesByAuthorAsync(string author);
        Task<Update?> GetUpdateByIdAsync(int updateId);
        Task<IEnumerable<Update>> GetRecentUpdatesAsync(int count = 10);

        // Write operations
        Task<Update> CreateUpdateAsync(Update update);
        Task<Update> UpdateUpdateAsync(Update update);
        Task<bool> DeleteUpdateAsync(int updateId);

        #endregion

        #region Release Operations

        // Read operations
        Task<IEnumerable<Release>> GetAllReleasesAsync();
        Task<IEnumerable<Release>> GetRecentReleasesAsync(int count = 10);
        Task<IEnumerable<Release>> GetReleasesByTypeAsync(string releaseType);
        Task<Release?> GetReleaseByIdAsync(int releaseId);
        Task<Release?> GetReleaseByVersionAsync(string version);
        Task<IEnumerable<Release>> GetPreReleasesAsync();
        Task<IEnumerable<Release>> GetDraftReleasesAsync();

        // Write operations
        Task<Release> CreateReleaseAsync(Release release);
        Task<Release> UpdateReleaseAsync(Release release);
        Task<bool> DeleteReleaseAsync(int releaseId);
        Task<bool> PublishReleaseAsync(int releaseId);

        #endregion

        #region Dependency Operations

        // Read operations
        Task<IEnumerable<Dependency>> GetAllDependenciesAsync();
        Task<IEnumerable<Dependency>> GetDependenciesByMilestoneAsync(int milestoneId);
        Task<IEnumerable<Dependency>> GetDependentMilestonesAsync(int milestoneId);
        Task<IEnumerable<Dependency>> GetBlockingDependenciesAsync(int milestoneId);
        Task<Dependency?> GetDependencyByIdAsync(int dependencyId);
        Task<IEnumerable<Dependency>> GetActiveDependenciesAsync();

        // Write operations
        Task<Dependency> CreateDependencyAsync(Dependency dependency);
        Task<Dependency> UpdateDependencyAsync(Dependency dependency);
        Task<bool> DeleteDependencyAsync(int dependencyId);
        Task<bool> ActivateDependencyAsync(int dependencyId);
        Task<bool> DeactivateDependencyAsync(int dependencyId);

        #endregion

        #region Analytics and Reporting

        // Progress and statistics
        Task<int> GetTotalMilestonesCountAsync();
        Task<int> GetCompletedMilestonesCountAsync();
        Task<int> GetInProgressMilestonesCountAsync();
        Task<int> GetTotalTasksCountAsync();
        Task<int> GetCompletedTasksCountAsync();
        Task<int> GetOverdueTasksCountAsync();
        Task<decimal> GetOverallProgressPercentageAsync();
        Task<TimeSpan> GetAverageCompletionTimeAsync();

        // Performance metrics
        Task<IEnumerable<Milestone>> GetMilestonesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Core.Data.Entities.Task>> GetTasksByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetMilestonesByCategoryStatsAsync();
        Task<Dictionary<MilestoneStatus, int>> GetMilestonesByStatusStatsAsync();
        Task<Dictionary<string, int>> GetTasksByAssigneeStatsAsync();

        // Time-based analytics
        Task<IEnumerable<Milestone>> GetMilestonesCompletedInPeriodAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Core.Data.Entities.Task>> GetTasksCompletedInPeriodAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Update>> GetUpdatesInPeriodAsync(DateTime startDate, DateTime endDate);

        #endregion

        #region Search and Filtering

        // Advanced search
        Task<IEnumerable<Milestone>> SearchMilestonesAsync(string searchTerm);
        Task<IEnumerable<Core.Data.Entities.Task>> SearchTasksAsync(string searchTerm);
        Task<IEnumerable<Update>> SearchUpdatesAsync(string searchTerm);

        // Complex filtering
        Task<IEnumerable<Milestone>> GetMilestonesWithFiltersAsync(
            MilestoneStatus? status = null,
            MilestonePriority? priority = null,
            string? category = null,
            DateTime? startDateFrom = null,
            DateTime? startDateTo = null,
            DateTime? dueDateFrom = null,
            DateTime? dueDateTo = null,
            decimal? minProgress = null,
            decimal? maxProgress = null);

        Task<IEnumerable<Core.Data.Entities.Task>> GetTasksWithFiltersAsync(
            TaskStatus? status = null,
            MilestonePriority? priority = null,
            string? assignee = null,
            int? milestoneId = null,
            DateTime? dueDateFrom = null,
            DateTime? dueDateTo = null,
            decimal? minProgress = null,
            decimal? maxProgress = null);

        #endregion

        #region Bulk Operations

        // Bulk updates
        Task<bool> BulkUpdateMilestoneStatusAsync(IEnumerable<int> milestoneIds, MilestoneStatus status);
        Task<bool> BulkUpdateTaskStatusAsync(IEnumerable<int> taskIds, TaskStatus status);
        Task<bool> BulkUpdateTaskAssigneeAsync(IEnumerable<int> taskIds, string assignee);
        Task<bool> BulkDeleteMilestonesAsync(IEnumerable<int> milestoneIds);
        Task<bool> BulkDeleteTasksAsync(IEnumerable<int> taskIds);

        // Bulk creation
        Task<IEnumerable<Milestone>> BulkCreateMilestonesAsync(IEnumerable<Milestone> milestones);
        Task<IEnumerable<Core.Data.Entities.Task>> BulkCreateTasksAsync(IEnumerable<Core.Data.Entities.Task> tasks);

        #endregion

        #region Data Validation and Integrity

        // Validation methods
        Task<bool> ValidateMilestoneDependenciesAsync(int milestoneId);
        Task<bool> CheckForCircularDependenciesAsync(int milestoneId, int dependentMilestoneId);
        Task<bool> CanDeleteMilestoneAsync(int milestoneId);
        Task<bool> CanDeleteTaskAsync(int taskId);

        // Data integrity
        Task<IEnumerable<Milestone>> GetOrphanedMilestonesAsync();
        Task<IEnumerable<Core.Data.Entities.Task>> GetOrphanedTasksAsync();
        Task<IEnumerable<Dependency>> GetInvalidDependenciesAsync();

        #endregion

        #region Import/Export Support

        // Data export
        Task<IEnumerable<Milestone>> ExportMilestonesAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<Core.Data.Entities.Task>> ExportTasksAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<Update>> ExportUpdatesAsync(DateTime? fromDate = null, DateTime? toDate = null);

        // Archive operations
        Task<bool> ArchiveCompletedMilestonesAsync(DateTime beforeDate);
        Task<bool> ArchiveCompletedTasksAsync(DateTime beforeDate);
        Task<IEnumerable<Milestone>> GetArchivedMilestonesAsync();

        #endregion
    }
}