// TeachCrowdSale.Infrastructure/Repositories/RoadmapRepository.cs
using Microsoft.EntityFrameworkCore;
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Data.Enum;
using TeachCrowdSale.Core.Interfaces.Repositories;
using TeachCrowdSale.Infrastructure.Data.Context;
using TaskStatus = TeachCrowdSale.Core.Data.Enum.TaskStatus;

namespace TeachCrowdSale.Infrastructure.Repositories
{
    public class RoadmapRepository : IRoadmapRepository
    {
        private readonly TeachCrowdSaleDbContext _context;

        public RoadmapRepository(TeachCrowdSaleDbContext context)
        {
            _context = context;
        }

        #region Milestone Operations

        public async Task<List<Milestone>> GetMilestonesAsync(string? status = null, string? category = null)
        {
            var query = _context.Milestones
                .Include(m => m.Tasks)
                .Include(m => m.Dependencies)
                    .ThenInclude(d => d.DependsOnMilestone)
                .Include(m => m.Updates.OrderByDescending(u => u.CreatedAt).Take(3))
                .Where(m => m.IsPublic);

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<MilestoneStatus>(status, true, out var statusEnum))
                {
                    query = query.Where(m => m.Status == statusEnum);
                }
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(m => m.Category.ToLower() == category.ToLower());
            }

            return await query
                .OrderBy(m => m.SortOrder)
                .ThenBy(m => m.EstimatedCompletionDate)
                .ToListAsync();
        }

        public async Task<Milestone?> GetMilestoneAsync(int id)
        {
            return await _context.Milestones
                .Include(m => m.Tasks.OrderBy(t => t.SortOrder))
                .Include(m => m.Dependencies)
                    .ThenInclude(d => d.DependsOnMilestone)
                .Include(m => m.Updates.OrderByDescending(u => u.CreatedAt))
                .FirstOrDefaultAsync(m => m.Id == id && m.IsPublic);
        }

        public async Task<Milestone> CreateMilestoneAsync(Milestone milestone)
        {
            milestone.CreatedAt = DateTime.UtcNow;
            milestone.UpdatedAt = DateTime.UtcNow;

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

        public async System.Threading.Tasks.Task DeleteMilestoneAsync(int id)
        {
            var milestone = await _context.Milestones.FindAsync(id);
            if (milestone != null)
            {
                _context.Milestones.Remove(milestone);
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region Task Operations

        public async Task<List<Core.Data.Entities.Task>> GetTasksAsync(int milestoneId)
        {
            return await _context.DevelopmentTasks
                .Where(t => t.MilestoneId == milestoneId)
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Core.Data.Entities.Task?> GetTaskAsync(int id)
        {
            return await _context.DevelopmentTasks
                .Include(t => t.Milestone)
                .FirstOrDefaultAsync(t => t.Id == id);
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

        public async System.Threading.Tasks.Task DeleteTaskAsync(int id)
        {
            var task = await _context.DevelopmentTasks.FindAsync(id);
            if (task != null)
            {
                _context.DevelopmentTasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region Update Operations

        public async Task<List<Update>> GetUpdatesAsync(int? milestoneId = null, int count = 10)
        {
            var query = _context.Updates
                .Include(u => u.Milestone)
                .Where(u => u.IsPublic);

            if (milestoneId.HasValue)
            {
                query = query.Where(u => u.MilestoneId == milestoneId.Value);
            }

            return await query
                .OrderByDescending(u => u.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Update> CreateUpdateAsync(Update update)
        {
            update.CreatedAt = DateTime.UtcNow;

            _context.Updates.Add(update);
            await _context.SaveChangesAsync();
            return update;
        }

        #endregion

        #region Release Operations

        public async Task<List<Release>> GetReleasesAsync()
        {
            return await _context.Releases
                .Include(r => r.Milestones)
                .Where(r => r.IsPublic)
                .OrderByDescending(r => r.PlannedReleaseDate ?? r.ActualReleaseDate)
                .ToListAsync();
        }

        public async Task<Release?> GetReleaseAsync(int id)
        {
            return await _context.Releases
                .Include(r => r.Milestones)
                .FirstOrDefaultAsync(r => r.Id == id && r.IsPublic);
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

        #endregion

        #region Dependency Operations

        public async Task<List<Dependency>> GetDependenciesAsync(int milestoneId)
        {
            return await _context.Dependencies
                .Include(d => d.DependsOnMilestone)
                .Where(d => d.MilestoneId == milestoneId)
                .ToListAsync();
        }

        public async Task<Dependency> CreateDependencyAsync(Dependency dependency)
        {
            dependency.CreatedAt = DateTime.UtcNow;

            _context.Dependencies.Add(dependency);
            await _context.SaveChangesAsync();
            return dependency; 
        }

        public async System.Threading.Tasks.Task DeleteDependencyAsync(int id)
        {
            var dependency = await _context.Dependencies.FindAsync(id);
            if (dependency != null)
            {
                _context.Dependencies.Remove(dependency);
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region Statistics Operations

        public async Task<DevelopmentStatsData> GetDevelopmentStatsAsync()
        {
            var milestones = await _context.Milestones.Where(m => m.IsPublic).ToListAsync();
            var tasks = await _context.DevelopmentTasks
                .Include(t => t.Milestone)
                .Where(t => t.Milestone.IsPublic)
                .ToListAsync();

            var completedMilestones = milestones.Where(m => m.Status == MilestoneStatus.Completed).ToList();
            var completedTasks = tasks.Where(t => t.Status == TaskStatus.Completed).ToList();
            var blockedTasks = tasks.Where(t => t.Status == TaskStatus.Blocked).Count();

            // Calculate average completion time for completed milestones
            var avgCompletionTime = 0.0;
            if (completedMilestones.Any())
            {
                var completionTimes = completedMilestones
                    .Where(m => m.StartDate.HasValue && m.ActualCompletionDate.HasValue)
                    .Select(m => (m.ActualCompletionDate!.Value - m.StartDate!.Value).TotalDays)
                    .ToList();

                if (completionTimes.Any())
                {
                    avgCompletionTime = completionTimes.Average();
                }
            }

            var activeDevelopers = tasks
                .Where(t => !string.IsNullOrEmpty(t.AssignedDeveloper) &&
                           t.Status != TaskStatus.Completed)
                .Select(t => t.AssignedDeveloper)
                .Distinct()
                .Count();

            return new DevelopmentStatsData
            {
                TotalMilestones = milestones.Count,
                CompletedMilestones = completedMilestones.Count,
                InProgressMilestones = milestones.Count(m => m.Status == MilestoneStatus.InProgress),
                PlannedMilestones = milestones.Count(m => m.Status == MilestoneStatus.NotStarted || m.Status == MilestoneStatus.Planning),
                TotalTasks = tasks.Count,
                CompletedTasks = completedTasks.Count,
                BlockedTasks = blockedTasks,
                ActiveDevelopers = activeDevelopers,
                AverageCompletionTime = avgCompletionTime,
                LastUpdated = DateTime.UtcNow
            };
        }

        public async Task<List<ProgressHistoryData>> GetProgressHistoryAsync(int milestoneId)
        {
            // Get milestone updates that include progress changes
            var updates = await _context.Updates
                .Where(u => u.MilestoneId == milestoneId && u.ProgressChange.HasValue)
                .OrderBy(u => u.CreatedAt)
                .Select(u => new ProgressHistoryData
                {
                    Date = u.CreatedAt,
                    ProgressPercentage = u.ProgressChange!.Value,
                    Note = u.Title
                })
                .ToListAsync();

            // Add current milestone progress
            var milestone = await _context.Milestones.FindAsync(milestoneId);
            if (milestone != null)
            {
                updates.Add(new ProgressHistoryData
                {
                    Date = milestone.UpdatedAt,
                    ProgressPercentage = milestone.ProgressPercentage,
                    Note = "Current Progress"
                });
            }

            return updates;
        }

        #endregion

        #region Search Operations

        public async Task<List<Milestone>> SearchMilestonesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<Milestone>();
            }

            var term = searchTerm.ToLower();

            return await _context.Milestones
                .Include(m => m.Tasks)
                .Include(m => m.Dependencies)
                .Where(m => m.IsPublic &&
                           (m.Title.ToLower().Contains(term) ||
                            m.Description.ToLower().Contains(term) ||
                            m.Category.ToLower().Contains(term) ||
                            (m.TechnicalDetails != null && m.TechnicalDetails.ToLower().Contains(term))))
                .OrderBy(m => m.SortOrder)
                .ToListAsync();
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            return await _context.Milestones
                .Where(m => m.IsPublic)
                .Select(m => m.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<List<string>> GetDevelopersAsync()
        {
            return await _context.DevelopmentTasks
                .Include(t => t.Milestone)
                .Where(t => t.Milestone.IsPublic && !string.IsNullOrEmpty(t.AssignedDeveloper))
                .Select(t => t.AssignedDeveloper!)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();
        }

        #endregion
    }
}