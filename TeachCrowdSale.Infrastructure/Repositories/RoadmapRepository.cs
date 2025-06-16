using Microsoft.EntityFrameworkCore;
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Data.Entities.Enums;
using TeachCrowdSale.Core.Data.Enum;
using TeachCrowdSale.Infrastructure.Data.Context;

namespace TeachCrowdSale.Infrastructure.Repositories
{
    /// <summary>
    /// Updated Repository implementation for Roadmap data access
    /// Implements all methods required by the new RoadmapService
    /// </summary>
    public class RoadmapRepository : IRoadmapRepository
    {
        private readonly TeachCrowdSaleDbContext _context;

        public RoadmapRepository(TeachCrowdSaleDbContext context)
        {
            _context = context;
        }

        #region Milestone Operations

        public async Task<IEnumerable<Milestone>> GetAllMilestonesAsync()
        {
            return await _context.Milestones
                .Include(m => m.Tasks)
                .Include(m => m.Dependencies)
                .Include(m => m.Updates)
                .Where(m => m.IsPublic)
                .OrderBy(m => m.SortOrder)
                .ThenBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Milestone>> GetMilestonesByStatusAsync(MilestoneStatus status)
        {
            return await _context.Milestones
                .Include(m => m.Tasks)
                .Include(m => m.Dependencies)
                .Include(m => m.Updates.OrderByDescending(u => u.CreatedAt).Take(3))
                .Where(m => m.Status == status && m.IsPublic)
                .OrderBy(m => m.SortOrder)
                .ThenBy(m => m.EstimatedCompletionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Milestone>> GetFilteredMilestonesAsync(string? status, string? category, string? priority)
        {
            var query = _context.Milestones
                .Include(m => m.Tasks)
                .Include(m => m.Dependencies)
                .Include(m => m.Updates.OrderByDescending(u => u.CreatedAt).Take(3))
                .Where(m => m.IsPublic);

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<MilestoneStatus>(status, true, out var statusEnum))
            {
                query = query.Where(m => m.Status == statusEnum);
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(m => m.Category.ToLower() == category.ToLower());
            }

            if (!string.IsNullOrEmpty(priority) && Enum.TryParse<MilestonePriority>(priority, true, out var priorityEnum))
            {
                query = query.Where(m => m.Priority == priorityEnum);
            }

            return await query
                .OrderBy(m => m.SortOrder)
                .ThenBy(m => m.EstimatedCompletionDate)
                .ToListAsync();
        }

        public async Task<Milestone?> GetMilestoneByIdAsync(int milestoneId)
        {
            return await _context.Milestones
                .Include(m => m.Tasks)
                .Include(m => m.Dependencies)
                    .ThenInclude(d => d.DependentMilestone)
                .Include(m => m.Updates)
                .FirstOrDefaultAsync(m => m.Id == milestoneId && m.IsPublic);
        }

        public async Task<Milestone?> GetMilestoneWithDetailsAsync(int milestoneId)
        {
            return await _context.Milestones
                .Include(m => m.Tasks.OrderBy(t => t.SortOrder))
                .Include(m => m.Dependencies)
                    .ThenInclude(d => d.DependentMilestone)
                .Include(m => m.Updates.OrderByDescending(u => u.CreatedAt))
                .FirstOrDefaultAsync(m => m.Id == milestoneId && m.IsPublic);
        }

        public async Task<IEnumerable<Milestone>> GetMilestonesByPriorityAsync(MilestonePriority priority)
        {
            return await _context.Milestones
                .Include(m => m.Tasks)
                .Where(m => m.Priority == priority && m.IsPublic)
                .OrderBy(m => m.EstimatedCompletionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Milestone>> GetMilestonesByCategoryAsync(string category)
        {
            return await _context.Milestones
                .Include(m => m.Tasks)
                .Where(m => m.Category.ToLower() == category.ToLower() && m.IsPublic)
                .OrderBy(m => m.SortOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<Milestone>> GetOverdueMilestonesAsync()
        {
            var today = DateTime.UtcNow;
            return await _context.Milestones
                .Include(m => m.Tasks)
                .Where(m => m.EstimatedCompletionDate < today &&
                           m.Status != MilestoneStatus.Completed &&
                           m.IsPublic)
                .OrderBy(m => m.EstimatedCompletionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Milestone>> GetUpcomingMilestonesAsync(int days = 30)
        {
            var endDate = DateTime.UtcNow.AddDays(days);
            return await _context.Milestones
                .Include(m => m.Tasks)
                .Where(m => m.EstimatedCompletionDate <= endDate &&
                           m.Status != MilestoneStatus.Completed &&
                           m.IsPublic)
                .OrderBy(m => m.EstimatedCompletionDate)
                .ToListAsync();
        }

        public async Task<Milestone> CreateMilestoneAsync(Milestone milestone)
        {
            milestone.CreatedAt = DateTime.UtcNow;
            milestone.UpdatedAt = DateTime.UtcNow;
            milestone.IsPublic = true; // Default to public

            _context.Milestones.Add(milestone);
            await _context.SaveChangesAsync();
            return milestone;
        }

        public async Task<Milestone> UpdateMilestoneAsync(Milestone milestone)
        {
            milestone.UpdatedAt = DateTime.UtcNow;
            _context.Milestones.Update(milestone);
            await _context.SaveChangesAsync();
            return milestone;
        }

        public async Task<bool> DeleteMilestoneAsync(int milestoneId)
        {
            var milestone = await _context.Milestones.FindAsync(milestoneId);
            if (milestone != null)
            {
                _context.Milestones.Remove(milestone);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateMilestoneProgressAsync(int milestoneId, decimal progressPercentage)
        {
            var milestone = await _context.Milestones.FindAsync(milestoneId);
            if (milestone != null)
            {
                milestone.ProgressPercentage = progressPercentage;
                milestone.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateMilestoneStatusAsync(int milestoneId, MilestoneStatus status)
        {
            var milestone = await _context.Milestones.FindAsync(milestoneId);
            if (milestone != null)
            {
                milestone.Status = status;
                milestone.UpdatedAt = DateTime.UtcNow;

                if (status == MilestoneStatus.Completed)
                {
                    milestone.ActualCompletionDate = DateTime.UtcNow;
                    milestone.ProgressPercentage = 100;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        #endregion

        #region Task Operations

        public async Task<IEnumerable<Core.Data.Entities.Task>> GetAllTasksAsync()
        {
            return await _context.DevelopmentTasks
                .Include(t => t.Milestone)
                .OrderBy(t => t.MilestoneId)
                .ThenBy(t => t.SortOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Data.Entities.Task>> GetTasksByMilestoneAsync(int milestoneId)
        {
            return await _context.DevelopmentTasks
                .Include(t => t.Milestone)
                .Where(t => t.MilestoneId == milestoneId)
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Data.Entities.Task>> GetTasksByStatusAsync(TaskStatus status)
        {
            return await _context.DevelopmentTasks
                .Include(t => t.Milestone)
                .Where(t => t.Status == status)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Data.Entities.Task>> GetTasksByAssigneeAsync(string assignee)
        {
            return await _context.DevelopmentTasks
                .Include(t => t.Milestone)
                .Where(t => t.Assignee == assignee)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<Core.Data.Entities.Task?> GetTaskByIdAsync(int taskId)
        {
            return await _context.DevelopmentTasks
                .Include(t => t.Milestone)
                .FirstOrDefaultAsync(t => t.Id == taskId);
        }

        public async Task<IEnumerable<Core.Data.Entities.Task>> GetOverdueTasksAsync()
        {
            var today = DateTime.UtcNow;
            return await _context.DevelopmentTasks
                .Include(t => t.Milestone)
                .Where(t => t.DueDate < today && t.Status != TaskStatus.Completed)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Data.Entities.Task>> GetTasksByPriorityAsync(TaskPriority priority)
        {
            return await _context.DevelopmentTasks
                .Include(t => t.Milestone)
                .Where(t => t.Priority == priority)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<Core.Data.Entities.Task> CreateTaskAsync(Core.Data.Entities.Task task)
        {
            task.CreatedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;

            _context.DevelopmentTasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<Core.Data.Entities.Task> UpdateTaskAsync(Core.Data.Entities.Task task)
        {
            task.UpdatedAt = DateTime.UtcNow;
            _context.DevelopmentTasks.Update(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            var task = await _context.DevelopmentTasks.FindAsync(taskId);
            if (task != null)
            {
                _context.DevelopmentTasks.Remove(task);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateTaskProgressAsync(int taskId, decimal progressPercentage)
        {
            var task = await _context.DevelopmentTasks.FindAsync(taskId);
            if (task != null)
            {
                task.ProgressPercentage = progressPercentage;
                task.UpdatedAt = DateTime.UtcNow;

                if (progressPercentage == 100 && task.Status != TaskStatus.Completed)
                {
                    task.Status = TaskStatus.Completed;
                    task.CompletionDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateTaskStatusAsync(int taskId, TaskStatus status)
        {
            var task = await _context.DevelopmentTasks.FindAsync(taskId);
            if (task != null)
            {
                task.Status = status;
                task.UpdatedAt = DateTime.UtcNow;

                if (status == TaskStatus.Completed)
                {
                    task.CompletionDate = DateTime.UtcNow;
                    task.ProgressPercentage = 100;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> AssignTaskAsync(int taskId, string assignee)
        {
            var task = await _context.DevelopmentTasks.FindAsync(taskId);
            if (task != null)
            {
                task.Assignee = assignee;
                task.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        #endregion

        #region Update Operations

        public async Task<IEnumerable<Update>> GetAllUpdatesAsync()
        {
            return await _context.Updates
                .Include(u => u.Milestone)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Update>> GetUpdatesByMilestoneAsync(int milestoneId)
        {
            return await _context.Updates
                .Include(u => u.Milestone)
                .Where(u => u.MilestoneId == milestoneId)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Update>> GetUpdatesByTypeAsync(UpdateType updateType)
        {
            return await _context.Updates
                .Include(u => u.Milestone)
                .Where(u => u.UpdateType == updateType)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Update>> GetUpdatesByAuthorAsync(string author)
        {
            return await _context.Updates
                .Include(u => u.Milestone)
                .Where(u => u.Author == author)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<Update?> GetUpdateByIdAsync(int updateId)
        {
            return await _context.Updates
                .Include(u => u.Milestone)
                .FirstOrDefaultAsync(u => u.Id == updateId);
        }

        public async Task<IEnumerable<Update>> GetRecentUpdatesAsync(int count = 10)
        {
            return await _context.Updates
                .Include(u => u.Milestone)
                .OrderByDescending(u => u.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Update> CreateUpdateAsync(Update update)
        {
            update.CreatedAt = DateTime.UtcNow;
            update.UpdatedAt = DateTime.UtcNow;

            _context.Updates.Add(update);
            await _context.SaveChangesAsync();
            return update;
        }

        public async Task<Update> UpdateUpdateAsync(Update update)
        {
            update.UpdatedAt = DateTime.UtcNow;
            _context.Updates.Update(update);
            await _context.SaveChangesAsync();
            return update;
        }

        public async Task<bool> DeleteUpdateAsync(int updateId)
        {
            var update = await _context.Updates.FindAsync(updateId);
            if (update != null)
            {
                _context.Updates.Remove(update);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        #endregion

        #region Release Operations

        public async Task<IEnumerable<Release>> GetAllReleasesAsync()
        {
            return await _context.Releases
                .OrderByDescending(r => r.ReleaseDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Release>> GetRecentReleasesAsync(int count = 10)
        {
            return await _context.Releases
                .Where(r => !r.IsDraft)
                .OrderByDescending(r => r.ReleaseDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Release>> GetReleasesByTypeAsync(string releaseType)
        {
            return await _context.Releases
                .Where(r => r.ReleaseType == releaseType)
                .OrderByDescending(r => r.ReleaseDate)
                .ToListAsync();
        }

        public async Task<Release?> GetReleaseByIdAsync(int releaseId)
        {
            return await _context.Releases
                .FirstOrDefaultAsync(r => r.Id == releaseId);
        }

        public async Task<Release?> GetReleaseByVersionAsync(string version)
        {
            return await _context.Releases
                .FirstOrDefaultAsync(r => r.Version == version);
        }

        public async Task<IEnumerable<Release>> GetPreReleasesAsync()
        {
            return await _context.Releases
                .Where(r => r.IsPreRelease)
                .OrderByDescending(r => r.ReleaseDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Release>> GetDraftReleasesAsync()
        {
            return await _context.Releases
                .Where(r => r.IsDraft)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Release> CreateReleaseAsync(Release release)
        {
            release.CreatedAt = DateTime.UtcNow;
            release.UpdatedAt = DateTime.UtcNow;

            _context.Releases.Add(release);
            await _context.SaveChangesAsync();
            return release;
        }

        public async Task<Release> UpdateReleaseAsync(Release release)
        {
            release.UpdatedAt = DateTime.UtcNow;
            _context.Releases.Update(release);
            await _context.SaveChangesAsync();
            return release;
        }

        public async Task<bool> DeleteReleaseAsync(int releaseId)
        {
            var release = await _context.Releases.FindAsync(releaseId);
            if (release != null)
            {
                _context.Releases.Remove(release);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> PublishReleaseAsync(int releaseId)
        {
            var release = await _context.Releases.FindAsync(releaseId);
            if (release != null)
            {
                release.IsDraft = false;
                release.ReleaseDate = DateTime.UtcNow;
                release.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        #endregion

        #region Dependency Operations

        public async Task<IEnumerable<Dependency>> GetAllDependenciesAsync()
        {
            return await _context.Dependencies
                .Include(d => d.Milestone)
                .Include(d => d.DependentMilestone)
                .ToListAsync();
        }

        public async Task<IEnumerable<Dependency>> GetDependenciesByMilestoneAsync(int milestoneId)
        {
            return await _context.Dependencies
                .Include(d => d.Milestone)
                .Include(d => d.DependentMilestone)
                .Where(d => d.MilestoneId == milestoneId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Dependency>> GetDependentMilestonesAsync(int milestoneId)
        {
            return await _context.Dependencies
                .Include(d => d.Milestone)
                .Include(d => d.DependentMilestone)
                .Where(d => d.DependentMilestoneId == milestoneId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Dependency>> GetBlockingDependenciesAsync(int milestoneId)
        {
            return await _context.Dependencies
                .Include(d => d.Milestone)
                .Include(d => d.DependentMilestone)
                .Where(d => d.DependentMilestoneId == milestoneId &&
                           d.DependencyType == DependencyType.Blocking &&
                           d.IsActive)
                .ToListAsync();
        }

        public async Task<Dependency?> GetDependencyByIdAsync(int dependencyId)
        {
            return await _context.Dependencies
                .Include(d => d.Milestone)
                .Include(d => d.DependentMilestone)
                .FirstOrDefaultAsync(d => d.Id == dependencyId);
        }

        public async Task<IEnumerable<Dependency>> GetActiveDependenciesAsync()
        {
            return await _context.Dependencies
                .Include(d => d.Milestone)
                .Include(d => d.DependentMilestone)
                .Where(d => d.IsActive)
                .ToListAsync();
        }

        public async Task<Dependency> CreateDependencyAsync(Dependency dependency)
        {
            dependency.CreatedAt = DateTime.UtcNow;
            dependency.UpdatedAt = DateTime.UtcNow;

            _context.Dependencies.Add(dependency);
            await _context.SaveChangesAsync();
            return dependency;
        }

        public async Task<Dependency> UpdateDependencyAsync(Dependency dependency)
        {
            dependency.UpdatedAt = DateTime.UtcNow;
            _context.Dependencies.Update(dependency);
            await _context.SaveChangesAsync();
            return dependency;
        }

        public async Task<bool> DeleteDependencyAsync(int dependencyId)
        {
            var dependency = await _context.Dependencies.FindAsync(dependencyId);
            if (dependency != null)
            {
                _context.Dependencies.Remove(dependency);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> ActivateDependencyAsync(int dependencyId)
        {
            var dependency = await _context.Dependencies.FindAsync(dependencyId);
            if (dependency != null)
            {
                dependency.IsActive = true;
                dependency.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> DeactivateDependencyAsync(int dependencyId)
        {
            var dependency = await _context.Dependencies.FindAsync(dependencyId);
            if (dependency != null)
            {
                dependency.IsActive = false;
                dependency.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        #endregion

        #region Analytics and Reporting

        public async Task<int> GetTotalMilestonesCountAsync()
        {
            return await _context.Milestones.CountAsync(m => m.IsPublic);
        }

        public async Task<int> GetCompletedMilestonesCountAsync()
        {
            return await _context.Milestones.CountAsync(m => m.Status == MilestoneStatus.Completed && m.IsPublic);
        }

        public async Task<int> GetInProgressMilestonesCountAsync()
        {
            return await _context.Milestones.CountAsync(m => m.Status == MilestoneStatus.InProgress && m.IsPublic);
        }

        public async Task<int> GetTotalTasksCountAsync()
        {
            return await _context.DevelopmentTasks.CountAsync();
        }

        public async Task<int> GetCompletedTasksCountAsync()
        {
            return await _context.DevelopmentTasks.CountAsync(t => t.Status == TaskStatus.Completed);
        }

        public async Task<int> GetOverdueTasksCountAsync()
        {
            var today = DateTime.UtcNow;
            return await _context.DevelopmentTasks.CountAsync(t => t.DueDate < today && t.Status != TaskStatus.Completed);
        }

        public async Task<decimal> GetOverallProgressPercentageAsync()
        {
            var milestones = await _context.Milestones.Where(m => m.IsPublic).ToListAsync();
            if (!milestones.Any()) return 0;

            return milestones.Average(m => m.ProgressPercentage);
        }

        public async Task<TimeSpan> GetAverageCompletionTimeAsync()
        {
            var completedMilestones = await _context.Milestones
                .Where(m => m.Status == MilestoneStatus.Completed &&
                           m.StartDate.HasValue &&
                           m.ActualCompletionDate.HasValue &&
                           m.IsPublic)
                .ToListAsync();

            if (!completedMilestones.Any()) return TimeSpan.Zero;

            var avgTicks = completedMilestones
                .Average(m => (m.ActualCompletionDate!.Value - m.StartDate!.Value).Ticks);

            return new TimeSpan((long)avgTicks);
        }

        public async Task<IEnumerable<Milestone>> GetMilestonesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Milestones
                .Include(m => m.Tasks)
                .Where(m => m.CreatedAt >= startDate && m.CreatedAt <= endDate && m.IsPublic)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Data.Entities.Task>> GetTasksByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.DevelopmentTasks
                .Include(t => t.Milestone)
                .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetMilestonesByCategoryStatsAsync()
        {
            return await _context.Milestones
                .Where(m => m.IsPublic)
                .GroupBy(m => m.Category)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<MilestoneStatus, int>> GetMilestonesByStatusStatsAsync()
        {
            return await _context.Milestones
                .Where(m => m.IsPublic)
                .GroupBy(m => m.Status)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<string, int>> GetTasksByAssigneeStatsAsync()
        {
            return await _context.DevelopmentTasks
                .Where(t => !string.IsNullOrEmpty(t.Assignee))
                .GroupBy(t => t.Assignee!)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<IEnumerable<Milestone>> GetMilestonesCompletedInPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Milestones
                .Include(m => m.Tasks)
                .Where(m => m.ActualCompletionDate >= startDate &&
                           m.ActualCompletionDate <= endDate &&
                           m.Status == MilestoneStatus.Completed &&
                           m.IsPublic)
                .OrderBy(m => m.ActualCompletionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Data.Entities.Task>> GetTasksCompletedInPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.DevelopmentTasks
                .Include(t => t.Milestone)
                .Where(t => t.CompletionDate >= startDate &&
                           t.CompletionDate <= endDate &&
                           t.Status == TaskStatus.Completed)
                .OrderBy(t => t.CompletionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Update>> GetUpdatesInPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Updates
                .Include(u => u.Milestone)
                .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
                .OrderBy(u => u.CreatedAt)
                .ToListAsync();
        }

        #endregion

        #region Search and Filtering

        public async Task<IEnumerable<Milestone>> SearchMilestonesAsync(string searchTerm)
        {
            return await _context.Milestones
                .Include(m => m.Tasks)
                .Where(m => (m.Title.Contains(searchTerm) || m.Description.Contains(searchTerm)) && m.IsPublic)
                .OrderBy(m => m.Title)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Data.Entities.Task>> SearchTasksAsync(string searchTerm)
        {
            return await _context.DevelopmentTasks
                .Include(t => t.Milestone)
                .Where(t => t.Title.Contains(searchTerm) ||
                           (t.Description != null && t.Description.Contains(searchTerm)))
                .OrderBy(t => t.Title)
                .ToListAsync();
        }

        public async Task<IEnumerable<Update>> SearchUpdatesAsync(string searchTerm)
        {
            return await _context.Updates
                .Include(u => u.Milestone)
                .Where(u => u.Title.Contains(searchTerm) || u.Content.Contains(searchTerm))
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Milestone>> GetMilestonesWithFiltersAsync(
            MilestoneStatus? status = null,
            MilestonePriority? priority = null,
            string? category = null,
            DateTime? startDateFrom = null,
            DateTime? startDateTo = null,
            DateTime? dueDateFrom = null,
            DateTime? dueDateTo = null,
            decimal? minProgress = null,
            decimal? maxProgress = null)
        {
            var query = _context.Milestones
                .Include(m => m.Tasks)
                .Include(m => m.Dependencies)
                .Where(m => m.IsPublic);

            if (status.HasValue)
                query = query.Where(m => m.Status == status.Value);

            if (priority.HasValue)
                query = query.Where(m => m.Priority == priority.Value);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(m => m.Category.ToLower() == category.ToLower());

            if (startDateFrom.HasValue)
                query = query.Where(m => m.StartDate >= startDateFrom.Value);

            if (startDateTo.HasValue)
                query = query.Where(m => m.StartDate <= startDateTo.Value);

            if (dueDateFrom.HasValue)
                query = query.Where(m => m.EstimatedCompletionDate >= dueDateFrom.Value);

            if (dueDateTo.HasValue)
                query = query.Where(m => m.EstimatedCompletionDate <= dueDateTo.Value);

            if (minProgress.HasValue)
                query = query.Where(m => m.ProgressPercentage >= minProgress.Value);

            if (maxProgress.HasValue)
                query = query.Where(m => m.ProgressPercentage <= maxProgress.Value);

            return await query
                .OrderBy(m => m.SortOrder)
                .ThenBy(m => m.EstimatedCompletionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Data.Entities.Task>> GetTasksWithFiltersAsync(
            TaskStatus? status = null,
            TaskPriority? priority = null,
            string? assignee = null,
            int? milestoneId = null,
            DateTime? dueDateFrom = null,
            DateTime? dueDateTo = null,
            decimal? minProgress = null,
            decimal? maxProgress = null)
        {
            var query = _context.DevelopmentTasks
                .Include(t => t.Milestone);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            if (!string.IsNullOrEmpty(assignee))
                query = query.Where(t => t.Assignee == assignee);

            if (milestoneId.HasValue)
                query = query.Where(t => t.MilestoneId == milestoneId.Value);

            if (dueDateFrom.HasValue)
                query = query.Where(t => t.DueDate >= dueDateFrom.Value);

            if (dueDateTo.HasValue)
                query = query.Where(t => t.DueDate <= dueDateTo.Value);

            if (minProgress.HasValue)
                query = query.Where(t => t.ProgressPercentage >= minProgress.Value);

            if (maxProgress.HasValue)
                query = query.Where(t => t.ProgressPercentage <= maxProgress.Value);

            return await query
                .OrderBy(t => t.DueDate)
                .ThenBy(t => t.SortOrder)
                .ToListAsync();
        }

        #endregion

        #region Bulk Operations

        public async Task<bool> BulkUpdateMilestoneStatusAsync(IEnumerable<int> milestoneIds, MilestoneStatus status)
        {
            var milestones = await _context.Milestones
                .Where(m => milestoneIds.Contains(m.Id))
                .ToListAsync();

            foreach (var milestone in milestones)
            {
                milestone.Status = status;
                milestone.UpdatedAt = DateTime.UtcNow;

                if (status == MilestoneStatus.Completed)
                {
                    milestone.ActualCompletionDate = DateTime.UtcNow;
                    milestone.ProgressPercentage = 100;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BulkUpdateTaskStatusAsync(IEnumerable<int> taskIds, TaskStatus status)
        {
            var tasks = await _context.DevelopmentTasks
                .Where(t => taskIds.Contains(t.Id))
                .ToListAsync();

            foreach (var task in tasks)
            {
                task.Status = status;
                task.UpdatedAt = DateTime.UtcNow;

                if (status == TaskStatus.Completed)
                {
                    task.CompletionDate = DateTime.UtcNow;
                    task.ProgressPercentage = 100;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BulkUpdateTaskAssigneeAsync(IEnumerable<int> taskIds, string assignee)
        {
            var tasks = await _context.DevelopmentTasks
                .Where(t => taskIds.Contains(t.Id))
                .ToListAsync();

            foreach (var task in tasks)
            {
                task.Assignee = assignee;
                task.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BulkDeleteMilestonesAsync(IEnumerable<int> milestoneIds)
        {
            var milestones = await _context.Milestones
                .Where(m => milestoneIds.Contains(m.Id))
                .ToListAsync();

            _context.Milestones.RemoveRange(milestones);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BulkDeleteTasksAsync(IEnumerable<int> taskIds)
        {
            var tasks = await _context.DevelopmentTasks
                .Where(t => taskIds.Contains(t.Id))
                .ToListAsync();

            _context.DevelopmentTasks.RemoveRange(tasks);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Milestone>> BulkCreateMilestonesAsync(IEnumerable<Milestone> milestones)
        {
            var now = DateTime.UtcNow;
            foreach (var milestone in milestones)
            {
                milestone.CreatedAt = now;
                milestone.UpdatedAt = now;
                milestone.IsPublic = true;
            }

            _context.Milestones.AddRange(milestones);
            await _context.SaveChangesAsync();
            return milestones;
        }

        public async Task<IEnumerable<Core.Data.Entities.Task>> BulkCreateTasksAsync(IEnumerable<Core.Data.Entities.Task> tasks)
        {
            var now = DateTime.UtcNow;
            foreach (var task in tasks)
            {
                task.CreatedAt = now;
                task.UpdatedAt = now;
            }

            _context.DevelopmentTasks.AddRange(tasks);
            await _context.SaveChangesAsync();
            return tasks;
        }

        #endregion

        #region Data Validation and Integrity

        public async Task<bool> ValidateMilestoneDependenciesAsync(int milestoneId)
        {
            var dependencies = await _context.Dependencies
                .Where(d => d.MilestoneId == milestoneId && d.IsActive)
                .Include(d => d.DependentMilestone)
                .ToListAsync();

            foreach (var dependency in dependencies)
            {
                if (dependency.DependentMilestone?.Status != MilestoneStatus.Completed &&
                    dependency.DependencyType == DependencyType.Blocking)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> CheckForCircularDependenciesAsync(int milestoneId, int dependentMilestoneId)
        {
            // Check if adding this dependency would create a circular reference
            var visited = new HashSet<int>();
            var recursionStack = new HashSet<int>();

            return !await HasCircularDependencyAsync(dependentMilestoneId, milestoneId, visited, recursionStack);
        }

        private async Task<bool> HasCircularDependencyAsync(int currentId, int targetId, HashSet<int> visited, HashSet<int> recursionStack)
        {
            if (recursionStack.Contains(currentId))
                return true;

            if (visited.Contains(currentId))
                return false;

            visited.Add(currentId);
            recursionStack.Add(currentId);

            var dependencies = await _context.Dependencies
                .Where(d => d.MilestoneId == currentId && d.IsActive)
                .Select(d => d.DependentMilestoneId)
                .ToListAsync();

            foreach (var depId in dependencies)
            {
                if (depId == targetId || await HasCircularDependencyAsync(depId, targetId, visited, recursionStack))
                {
                    return true;
                }
            }

            recursionStack.Remove(currentId);
            return false;
        }

        public async Task<bool> CanDeleteMilestoneAsync(int milestoneId)
        {
            // Check if milestone has dependent milestones
            var hasDependents = await _context.Dependencies
                .AnyAsync(d => d.MilestoneId == milestoneId && d.IsActive);

            return !hasDependents;
        }

        public async Task<bool> CanDeleteTaskAsync(int taskId)
        {
            // Tasks can generally be deleted unless they have specific business rules
            // For now, return true, but this could be extended with business logic
            return true;
        }

        public async Task<IEnumerable<Milestone>> GetOrphanedMilestonesAsync()
        {
            // Milestones without any tasks (potential orphans)
            return await _context.Milestones
                .Where(m => !m.Tasks.Any() && m.IsPublic)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Data.Entities.Task>> GetOrphanedTasksAsync()
        {
            // Tasks without a valid milestone
            return await _context.DevelopmentTasks
                .Where(t => t.Milestone == null)
                .ToListAsync();
        }

        public async Task<IEnumerable<Dependency>> GetInvalidDependenciesAsync()
        {
            // Dependencies where either milestone doesn't exist or is not public
            return await _context.Dependencies
                .Where(d => d.Milestone == null ||
                           d.DependentMilestone == null ||
                           !d.Milestone.IsPublic ||
                           !d.DependentMilestone.IsPublic)
                .ToListAsync();
        }

        #endregion

        #region Import/Export Support

        public async Task<IEnumerable<Milestone>> ExportMilestonesAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Milestones
                .Include(m => m.Tasks)
                .Include(m => m.Dependencies)
                .Include(m => m.Updates)
                .Where(m => m.IsPublic);

            if (fromDate.HasValue)
                query = query.Where(m => m.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(m => m.CreatedAt <= toDate.Value);

            return await query
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Data.Entities.Task>> ExportTasksAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.DevelopmentTasks
                .Include(t => t.Milestone);

            if (fromDate.HasValue)
                query = query.Where(t => t.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(t => t.CreatedAt <= toDate.Value);

            return await query
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Update>> ExportUpdatesAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Updates
                .Include(u => u.Milestone);

            if (fromDate.HasValue)
                query = query.Where(u => u.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(u => u.CreatedAt <= toDate.Value);

            return await query
                .OrderBy(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ArchiveCompletedMilestonesAsync(DateTime beforeDate)
        {
            var milestonesToArchive = await _context.Milestones
                .Where(m => m.Status == MilestoneStatus.Completed &&
                           m.ActualCompletionDate < beforeDate &&
                           m.IsPublic)
                .ToListAsync();

            foreach (var milestone in milestonesToArchive)
            {
                milestone.IsPublic = false; // Archive by making non-public
                milestone.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ArchiveCompletedTasksAsync(DateTime beforeDate)
        {
            var tasksToArchive = await _context.DevelopmentTasks
                .Where(t => t.Status == TaskStatus.Completed &&
                           t.CompletionDate < beforeDate)
                .ToListAsync();

            // For tasks, we might move them to an archive table or mark them as archived
            // For now, we'll just update a flag (assuming there's an IsArchived property)
            foreach (var task in tasksToArchive)
            {
                // task.IsArchived = true; // Uncomment if this property exists
                task.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Milestone>> GetArchivedMilestonesAsync()
        {
            return await _context.Milestones
                .Include(m => m.Tasks)
                .Where(m => !m.IsPublic) // Archived milestones are non-public
                .OrderBy(m => m.ActualCompletionDate)
                .ToListAsync();
        }

        #endregion

        #region Legacy Support Methods (for backward compatibility)

        // These methods maintain compatibility with the existing codebase
        public async Task<List<Milestone>> GetMilestonesAsync(string? status = null, string? category = null)
        {
            var result = await GetFilteredMilestonesAsync(status, category, null);
            return result.ToList();
        }

        public async Task<Milestone?> GetMilestoneAsync(int id)
        {
            return await GetMilestoneWithDetailsAsync(id);
        }

        public async Task<List<Core.Data.Entities.Task>> GetTasksAsync(int milestoneId)
        {
            var result = await GetTasksByMilestoneAsync(milestoneId);
            return result.ToList();
        }

        public async Task<Core.Data.Entities.Task?> GetTaskAsync(int id)
        {
            return await GetTaskByIdAsync(id);
        }

        public async Task<List<Update>> GetUpdatesAsync(int? milestoneId = null, int count = 10)
        {
            if (milestoneId.HasValue)
            {
                var result = await GetUpdatesByMilestoneAsync(milestoneId.Value);
                return result.Take(count).ToList();
            }
            else
            {
                var result = await GetRecentUpdatesAsync(count);
                return result.ToList();
            }
        }

        public async Task<List<Release>> GetReleasesAsync()
        {
            var result = await GetAllReleasesAsync();
            return result.ToList();
        }

        public async Task<Release?> GetReleaseAsync(int id)
        {
            return await GetReleaseByIdAsync(id);
        }

        public async Task<List<Dependency>> GetDependenciesAsync(int milestoneId)
        {
            var result = await GetDependenciesByMilestoneAsync(milestoneId);
            return result.ToList();
        }

        public async System.Threading.Tasks.Task DeleteMilestoneAsync(int id)
        {
            await DeleteMilestoneAsync(id);
        }

        public async System.Threading.Tasks.Task DeleteTaskAsync(int id)
        {
            await DeleteTaskAsync(id);
        }

        public async System.Threading.Tasks.Task DeleteDependencyAsync(int id)
        {
            await DeleteDependencyAsync(id);
        }

        // Placeholder methods for development stats and other features
        public async Task<DevelopmentStatsData> GetDevelopmentStatsAsync()
        {
            // TODO: Implement actual development stats calculation
            // For now, return empty/default data
            return new DevelopmentStatsData();
        }

        public async Task<List<ProgressHistoryData>> GetProgressHistoryAsync(int milestoneId)
        {
            // TODO: Implement progress history tracking
            // For now, return empty list
            return new List<ProgressHistoryData>();
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            return await _context.Milestones
                .Where(m => m.IsPublic && !string.IsNullOrEmpty(m.Category))
                .Select(m => m.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<List<string>> GetDevelopersAsync()
        {
            return await _context.DevelopmentTasks
                .Where(t => !string.IsNullOrEmpty(t.Assignee))
                .Select(t => t.Assignee!)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();
        }

        #endregion
    }

    #region Supporting Data Models

    // Temporary placeholder classes for compatibility
    // These should be moved to appropriate model files
    public class DevelopmentStatsData
    {
        public int TotalCommits { get; set; }
        public int OpenIssues { get; set; }
        public int ClosedIssues { get; set; }
        public decimal CodeCoverage { get; set; }
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
    }

    public class ProgressHistoryData
    {
        public DateTime Date { get; set; }
        public decimal ProgressPercentage { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    #endregion
}