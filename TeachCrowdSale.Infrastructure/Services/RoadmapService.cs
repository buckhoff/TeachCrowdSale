// TeachCrowdSale.Infrastructure/Services/RoadmapService.cs
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Models.Response;
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Data.Enum;
using TeachCrowdSale.Core.Interfaces.Repositories;
using TeachCrowdSale.Core.Models;
using TaskStatus = TeachCrowdSale.Core.Data.Enum.TaskStatus;
using Task = System.Threading.Tasks.Task;

namespace TeachCrowdSale.Infrastructure.Services
{
    public class RoadmapService : IRoadmapService
    {
        private readonly IRoadmapRepository _roadmapRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<RoadmapService> _logger;

        // Cache keys and durations
        private const string CACHE_KEY_ROADMAP_DATA = "roadmap_data";
        private const string CACHE_KEY_DEV_STATS = "development_stats";
        private const string CACHE_KEY_GITHUB_STATS = "github_stats";
        private const string CACHE_KEY_RECENT_UPDATES = "recent_updates";
        private const string CACHE_KEY_RELEASES = "releases";
        private const string CACHE_KEY_FILTER_OPTIONS = "filter_options";

        private readonly TimeSpan _shortCacheDuration = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _mediumCacheDuration = TimeSpan.FromMinutes(15);
        private readonly TimeSpan _longCacheDuration = TimeSpan.FromHours(1);

        public RoadmapService(
            IRoadmapRepository roadmapRepository,
            IMemoryCache cache,
            ILogger<RoadmapService> logger)
        {
            _roadmapRepository = roadmapRepository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<RoadmapDataModel> GetRoadmapDataAsync()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_ROADMAP_DATA, out RoadmapDataModel? cachedData) && cachedData != null)
                {
                    return cachedData;
                }

                var milestones = await _roadmapRepository.GetMilestonesAsync();
                var devStats = await GetDevelopmentStatsAsync();
                var updates = await GetRecentUpdatesAsync();
                var releases = await GetReleasesAsync();
                var gitHubStats = await GetGitHubStatsAsync();

                var currentMilestones = milestones.Where(m => m.Status == MilestoneStatus.InProgress).Select(MapToDisplayModel).ToList();
                var upcomingMilestones = milestones.Where(m => m.Status == MilestoneStatus.Planning).Select(MapToDisplayModel).ToList();
                var completedMilestones = milestones.Where(m => m.Status == MilestoneStatus.Completed).Select(MapToDisplayModel).ToList();

                var roadmapData = new RoadmapDataModel
                {
                    CurrentMilestones = currentMilestones,
                    UpcomingMilestones = upcomingMilestones,
                    CompletedMilestones = completedMilestones,
                    DevelopmentStats = devStats,
                    RecentUpdates = updates,
                    Releases = releases,
                    GitHubStats = gitHubStats,
                    LoadedAt = DateTime.UtcNow,

                    // ADD THIS:
                    Overview = new RoadmapOverviewModel
                    {
                        TotalMilestones = milestones.Count,
                        CompletedMilestones = completedMilestones.Count,
                        InProgressMilestones = currentMilestones.Count,
                        UpcomingMilestones = upcomingMilestones.Count,
                        OnHoldMilestones = milestones.Count(m => m.Status == MilestoneStatus.OnHold),
                        OverallProgress = CalculateOverallProgress(milestones),
                        EstimatedCompletionDate = upcomingMilestones.Where(m => m.EstimatedCompletionDate.HasValue)
                            .Select(m => m.EstimatedCompletionDate.Value).DefaultIfEmpty().Max(),
                        LastUpdateDate = updates.FirstOrDefault()?.CreatedAt ?? DateTime.UtcNow.AddDays(-1),
                        IsOnTrack = CheckIfOnTrack(currentMilestones),
                        ProjectHealthStatus = GetProjectHealthStatus(currentMilestones, devStats),
                        ProjectHealthClass = GetProjectHealthClass(currentMilestones, devStats)
                    }
                };

                _cache.Set(CACHE_KEY_ROADMAP_DATA, roadmapData, _mediumCacheDuration);
                return roadmapData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching roadmap data");
                return GetFallbackRoadmapData();
            }
        }

        public async Task<List<MilestoneDisplayModel>> GetMilestonesAsync(string? status = null, string? category = null)
        {
            try
            {
                var cacheKey = $"milestones_{status}_{category}";
                if (_cache.TryGetValue(cacheKey, out List<MilestoneDisplayModel>? cachedMilestones) && cachedMilestones != null)
                {
                    return cachedMilestones;
                }

                var milestones = await _roadmapRepository.GetMilestonesAsync(status, category);
                var displayModels = milestones.Select(MapToDisplayModel).ToList();

                _cache.Set(cacheKey, displayModels, _mediumCacheDuration);
                return displayModels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving milestones");
                return new List<MilestoneDisplayModel>();
            }
        }

        public async Task<MilestoneDisplayModel?> GetMilestoneAsync(int id)
        {
            try
            {
                var milestone = await _roadmapRepository.GetMilestoneAsync(id);
                return milestone != null ? MapToDisplayModel(milestone) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving milestone {MilestoneId}", id);
                return null;
            }
        }

        public async Task<GitHubDevelopmentStatsModel> GetDevelopmentStatsAsync()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_DEV_STATS, out GitHubDevelopmentStatsModel? cachedStats) && cachedStats != null)
                {
                    return cachedStats;
                }

                var statsData = await _roadmapRepository.GetDevelopmentStatsAsync();
                var stats = new GitHubDevelopmentStatsModel
                {
                    TotalMilestones = statsData.TotalMilestones,
                    CompletedMilestones = statsData.CompletedMilestones,
                    InProgressMilestones = statsData.InProgressMilestones,
                    PlannedMilestones = statsData.PlannedMilestones,
                    OverallProgress = statsData.TotalMilestones > 0 ?
                        (decimal)statsData.CompletedMilestones / statsData.TotalMilestones * 100 : 0,
                    TotalTasks = statsData.TotalTasks,
                    CompletedTasks = statsData.CompletedTasks,
                    BlockedTasks = statsData.BlockedTasks,
                    TaskCompletionRate = statsData.TotalTasks > 0 ?
                        (decimal)statsData.CompletedTasks / statsData.TotalTasks * 100 : 0,
                    ActiveDevelopers = statsData.ActiveDevelopers,
                    AverageCompletionTime = (decimal)statsData.AverageCompletionTime,
                    CurrentSprintName = "Sprint 2024.2",
                    CurrentSprintEndDate = DateTime.UtcNow.AddDays(14),
                    LastUpdated = statsData.LastUpdated
                };

                _cache.Set(CACHE_KEY_DEV_STATS, stats, _mediumCacheDuration);
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving development statistics");
                return GetFallbackDevelopmentStats();
            }
        }

        public async Task<List<UpdateDisplayModel>> GetRecentUpdatesAsync(int count = 10)
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_RECENT_UPDATES, out List<UpdateDisplayModel>? cachedUpdates) && cachedUpdates != null)
                {
                    return cachedUpdates.Take(count).ToList();
                }

                var updates = await _roadmapRepository.GetUpdatesAsync(null, count);
                var displayModels = updates.Select(MapToUpdateDisplayModel).ToList();

                _cache.Set(CACHE_KEY_RECENT_UPDATES, displayModels, _shortCacheDuration);
                return displayModels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent updates");
                return new List<UpdateDisplayModel>();
            }
        }

        public async Task<List<ReleaseDisplayModel>> GetReleasesAsync()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_RELEASES, out List<ReleaseDisplayModel>? cachedReleases) && cachedReleases != null)
                {
                    return cachedReleases;
                }

                var releases = await _roadmapRepository.GetReleasesAsync();
                var displayModels = releases.Select(MapToReleaseDisplayModel).ToList();

                _cache.Set(CACHE_KEY_RELEASES, displayModels, _longCacheDuration);
                return displayModels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving releases");
                return new List<ReleaseDisplayModel>();
            }
        }

        public async Task<GitHubStatsModel> GetGitHubStatsAsync()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_GITHUB_STATS, out GitHubStatsModel? cachedStats) && cachedStats != null)
                {
                    return cachedStats;
                }

                //var gitHubStats = await _gitHubService.GetGitHubStatsAsync();
                //_cache.Set(CACHE_KEY_GITHUB_STATS, gitHubStats, _mediumCacheDuration);
                //return gitHubStats;
                return new GitHubStatsModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving GitHub statistics");
                return GetFallbackGitHubStats();
            }
        }

        public async Task<RoadmapFilterModel> GetFilterOptionsAsync()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_FILTER_OPTIONS, out RoadmapFilterModel? cachedOptions) && cachedOptions != null)
                {
                    return cachedOptions;
                }

                var tasks = new[]
                {
                    _roadmapRepository.GetCategoriesAsync(),
                    _roadmapRepository.GetDevelopersAsync()
                };

                var results = await Task.WhenAll(tasks);

                var filterOptions = new RoadmapFilterModel
                {
                    Categories = results[0],
                    Developers = results[1],
                    Statuses = Enum.GetNames<MilestoneStatus>().ToList(),
                    Priorities = Enum.GetNames<MilestonePriority>().ToList(),
                    Types = Enum.GetNames<TaskType>().ToList()
                };

                _cache.Set(CACHE_KEY_FILTER_OPTIONS, filterOptions, _longCacheDuration);
                return filterOptions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving filter options");
                return new RoadmapFilterModel();
            }
        }

        public async Task<List<TaskDisplayModel>> GetMilestoneTasksAsync(int milestoneId)
        {
            try
            {
                var tasks = await _roadmapRepository.GetTasksAsync(milestoneId);
                return tasks.Select(MapToTaskDisplayModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks for milestone {MilestoneId}", milestoneId);
                return new List<TaskDisplayModel>();
            }
        }

        public async Task<List<MilestoneDisplayModel>> SearchMilestonesAsync(string searchTerm)
        {
            try
            {
                var milestones = await _roadmapRepository.SearchMilestonesAsync(searchTerm);
                return milestones.Select(MapToDisplayModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching milestones for term '{SearchTerm}'", searchTerm);
                return new List<MilestoneDisplayModel>();
            }
        }

        public async Task<List<DependencyDisplayModel>> GetMilestoneDependenciesAsync(int milestoneId)
        {
            try
            {
                var dependencies = await _roadmapRepository.GetDependenciesAsync(milestoneId);
                return dependencies.Select(MapToDependencyDisplayModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dependencies for milestone {MilestoneId}", milestoneId);
                return new List<DependencyDisplayModel>();
            }
        }

        public async Task<object> GetTimelineDataAsync()
        {
            try
            {
                var milestones = await _roadmapRepository.GetMilestonesAsync();

                return milestones
                    .Where(m => m.EstimatedCompletionDate.HasValue)
                    .Select(m => new
                    {
                        id = m.Id,
                        title = m.Title,
                        start = m.StartDate?.ToString("yyyy-MM-dd"),
                        end = m.EstimatedCompletionDate?.ToString("yyyy-MM-dd"),
                        progress = m.ProgressPercentage,
                        status = m.Status.ToString(),
                        category = m.Category
                    })
                    .OrderBy(m => m.start)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving timeline data");
                return new List<object>();
            }
        }

        public async Task<object> GetProgressHistoryAsync(int milestoneId)
        {
            try
            {
                var history = await _roadmapRepository.GetProgressHistoryAsync(milestoneId);

                return history.Select(h => new
                {
                    date = h.Date.ToString("yyyy-MM-dd"),
                    progress = h.ProgressPercentage,
                    note = h.Note
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving progress history for milestone {MilestoneId}", milestoneId);
                return new List<object>();
            }
        }

        #region Private Mapping Methods

        private MilestoneDisplayModel MapToDisplayModel(Milestone milestone)
        {
            var completedTasks = milestone.Tasks.Count(t => t.Status == TaskStatus.Completed);
            var totalTasks = milestone.Tasks.Count;
            var blockedTasks = milestone.Tasks.Any(t => t.Status == TaskStatus.Blocked);

            return new MilestoneDisplayModel
            {
                Id = milestone.Id,
                Title = milestone.Title,
                Description = milestone.Description,
                Category = milestone.Category,
                Status = milestone.Status.ToString(),
                StatusClass = GetStatusClass(milestone.Status),
                Priority = milestone.Priority.ToString(),
                PriorityClass = GetPriorityClass(milestone.Priority),
                StartDate = milestone.StartDate,
                EstimatedCompletionDate = milestone.EstimatedCompletionDate,
                ActualCompletionDate = milestone.ActualCompletionDate,
                ProgressPercentage = milestone.ProgressPercentage,
                TechnicalDetails = milestone.TechnicalDetails,
                GitHubIssueUrl = milestone.GitHubIssueUrl,
                DocumentationUrl = milestone.DocumentationUrl,
                Tasks = milestone.Tasks.Select(MapToTaskDisplayModel).ToList(),
                Dependencies = milestone.Dependencies.Select(MapToDependencyDisplayModel).ToList(),
                RecentUpdates = milestone.Updates.Take(3).Select(MapToUpdateDisplayModel).ToList(),
                DurationEstimate = CalculateDurationEstimate(milestone.StartDate, milestone.EstimatedCompletionDate),
                TimeRemaining = CalculateTimeRemaining(milestone.EstimatedCompletionDate),
                IsOverdue = IsOverdue(milestone.EstimatedCompletionDate, milestone.Status),
                IsBlocked = blockedTasks,
                CompletedTasksCount = completedTasks,
                TotalTasksCount = totalTasks
            };
        }

        private TaskDisplayModel MapToTaskDisplayModel(Core.Data.Entities.Task task)
        {
            return new TaskDisplayModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status.ToString(),
                StatusClass = GetTaskStatusClass(task.Status),
                Type = task.Type.ToString(),
                TypeClass = GetTaskTypeClass(task.Type),
                StartDate = task.StartDate,
                EstimatedCompletionDate = task.EstimatedCompletionDate,
                ActualCompletionDate = task.ActualCompletionDate,
                ProgressPercentage = task.ProgressPercentage,
                AssignedDeveloper = task.AssignedDeveloper,
                GitHubIssueUrl = task.GitHubIssueUrl,
                PullRequestUrl = task.PullRequestUrl,
                DurationEstimate = CalculateDurationEstimate(task.StartDate, task.EstimatedCompletionDate),
                IsOverdue = IsOverdue(task.EstimatedCompletionDate, task.Status),
                IsBlocked = task.Status == TaskStatus.Blocked
            };
        }

        private DependencyDisplayModel MapToDependencyDisplayModel(Dependency dependency)
        {
            return new DependencyDisplayModel
            {
                Id = dependency.Id,
                Type = dependency.Type.ToString(),
                Description = dependency.Description ?? "",
                DependsOnMilestoneId = dependency.DependsOnMilestoneId,
                DependsOnMilestoneTitle = dependency.DependsOnMilestone.Title,
                DependsOnMilestoneStatus = dependency.DependsOnMilestone.Status.ToString(),
                IsBlocking = dependency.Type == DependencyType.BlockedBy &&
                           dependency.DependsOnMilestone.Status != MilestoneStatus.Completed
            };
        }

        private UpdateDisplayModel MapToUpdateDisplayModel(Update update)
        {
            return new UpdateDisplayModel
            {
                Id = update.Id,
                Title = update.Title,
                Content = update.Content,
                Type = update.Type.ToString(),
                TypeClass = GetUpdateTypeClass(update.Type),
                ProgressChange = update.ProgressChange,
                AuthorName = update.AuthorName,
                CreatedAt = update.CreatedAt,
                MilestoneId = update.MilestoneId,
                MilestoneTitle = update.Milestone.Title,
                FormattedDate = update.CreatedAt.ToString("MMM dd, yyyy"),
                TimeAgo = CalculateTimeAgo(update.CreatedAt),
                UpdateDate = update.CreatedAt, 
                Description = update.Content, 
                Author = update.AuthorName ?? "Unknown", 
                Category = update.Milestone?.Category ?? "General" 
            };
        }

        private ReleaseDisplayModel MapToReleaseDisplayModel(Release release)
        {
            return new ReleaseDisplayModel
            {
                Id = release.Id,
                Name = release.Name,
                Version = release.Version,
                Description = release.Description,
                Type = release.Type.ToString(),
                TypeClass = GetReleaseTypeClass(release.Type),
                Status = release.Status.ToString(),
                StatusClass = GetReleaseStatusClass(release.Status),
                PlannedReleaseDate = release.PlannedReleaseDate,
                ActualReleaseDate = release.ActualReleaseDate,
                ReleaseNotes = release.ReleaseNotes,
                GitHubReleaseUrl = release.GitHubReleaseUrl,
                DocumentationUrl = release.DocumentationUrl,
                IncludedMilestones = release.Milestones?.Select(m => m.Title).ToList() ?? new List<string>(),
                FormattedReleaseDate = (release.ActualReleaseDate ?? release.PlannedReleaseDate)?.ToString("MMM dd, yyyy") ?? "",
                IsOverdue = release.PlannedReleaseDate.HasValue && release.ActualReleaseDate == null && release.PlannedReleaseDate < DateTime.UtcNow,
                TimeToRelease = CalculateTimeToRelease(release.PlannedReleaseDate, release.ActualReleaseDate),
                Title = release.Name, 
                ReleaseDate = release.ActualReleaseDate ?? release.PlannedReleaseDate, 
                IsPreRelease = release.Type == ReleaseType.Alpha || release.Type == ReleaseType.Beta || release.Version.Contains("beta") || release.Version.Contains("alpha"),
                DownloadUrl = release.GitHubReleaseUrl
            };
        }

        #endregion

        #region Helper Methods

        private string GetStatusClass(MilestoneStatus status)
        {
            return status switch
            {
                MilestoneStatus.NotStarted => "not-started",
                MilestoneStatus.Planning => "planning",
                MilestoneStatus.InProgress => "in-progress",
                MilestoneStatus.Testing => "testing",
                MilestoneStatus.Review => "review",
                MilestoneStatus.Completed => "completed",
                MilestoneStatus.Cancelled => "cancelled",
                MilestoneStatus.OnHold => "on-hold",
                _ => "unknown"
            };
        }

        private string GetPriorityClass(MilestonePriority priority)
        {
            return priority switch
            {
                MilestonePriority.Low => "low",
                MilestonePriority.Medium => "medium",
                MilestonePriority.High => "high",
                MilestonePriority.Critical => "critical",
                _ => "medium"
            };
        }

        private string GetTaskStatusClass(TaskStatus status)
        {
            return status switch
            {
                TaskStatus.NotStarted => "not-started",
                TaskStatus.InProgress => "in-progress",
                TaskStatus.InReview => "in-review",
                TaskStatus.Testing => "testing",
                TaskStatus.Completed => "completed",
                TaskStatus.Blocked => "blocked",
                _ => "unknown"
            };
        }

        private string GetTaskTypeClass(TaskType type)
        {
            return type switch
            {
                TaskType.Feature => "feature",
                TaskType.Bug => "bug",
                TaskType.Enhancement => "enhancement",
                TaskType.Research => "research",
                TaskType.Documentation => "documentation",
                TaskType.Testing => "testing",
                TaskType.Deployment => "deployment",
                TaskType.Infrastructure => "infrastructure",
                _ => "unknown"
            };
        }

        private string GetUpdateTypeClass(UpdateType type)
        {
            return type switch
            {
                UpdateType.Progress => "progress",
                UpdateType.StatusChange => "status-change",
                UpdateType.Blocker => "blocker",
                UpdateType.Completion => "completion",
                UpdateType.Delay => "delay",
                UpdateType.General => "general",
                _ => "general"
            };
        }

        private string GetReleaseTypeClass(ReleaseType type)
        {
            return type switch
            {
                ReleaseType.Major => "major",
                ReleaseType.Minor => "minor",
                ReleaseType.Patch => "patch",
                ReleaseType.Beta => "beta",
                ReleaseType.Alpha => "alpha",
                ReleaseType.Hotfix => "hotfix",
                _ => "minor"
            };
        }

        private string GetReleaseStatusClass(ReleaseStatus status)
        {
            return status switch
            {
                ReleaseStatus.Planned => "planned",
                ReleaseStatus.InDevelopment => "in-development",
                ReleaseStatus.Testing => "testing",
                ReleaseStatus.Staged => "staged",
                ReleaseStatus.Released => "released",
                ReleaseStatus.Cancelled => "cancelled",
                _ => "planned"
            };
        }

        private string CalculateDurationEstimate(DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue || !endDate.HasValue)
                return "TBD";

            var duration = endDate.Value - startDate.Value;
            if (duration.TotalDays < 1)
                return "< 1 day";
            if (duration.TotalDays < 7)
                return $"{(int)duration.TotalDays} days";
            if (duration.TotalDays < 30)
                return $"{(int)(duration.TotalDays / 7)} weeks";
            return $"{(int)(duration.TotalDays / 30)} months";
        }

        private string CalculateTimeRemaining(DateTime? endDate)
        {
            if (!endDate.HasValue)
                return "TBD";

            var remaining = endDate.Value - DateTime.UtcNow;
            if (remaining.TotalDays < 0)
                return "Overdue";
            if (remaining.TotalDays < 1)
                return "< 1 day";
            if (remaining.TotalDays < 7)
                return $"{(int)remaining.TotalDays} days";
            if (remaining.TotalDays < 30)
                return $"{(int)(remaining.TotalDays / 7)} weeks";
            return $"{(int)(remaining.TotalDays / 30)} months";
        }

        private bool IsOverdue(DateTime? endDate, MilestoneStatus status)
        {
            return endDate.HasValue &&
                   DateTime.UtcNow > endDate.Value &&
                   status != MilestoneStatus.Completed;
        }

        private bool IsOverdue(DateTime? endDate, TaskStatus status)
        {
            return endDate.HasValue &&
                   DateTime.UtcNow > endDate.Value &&
                   status != TaskStatus.Completed;
        }

        private bool IsReleaseOverdue(DateTime? plannedDate, ReleaseStatus status)
        {
            return plannedDate.HasValue &&
                   DateTime.UtcNow > plannedDate.Value &&
                   status != ReleaseStatus.Released;
        }

        private string CalculateTimeAgo(DateTime date)
        {
            var timeAgo = DateTime.UtcNow - date;
            if (timeAgo.TotalMinutes < 60)
                return $"{(int)timeAgo.TotalMinutes}m ago";
            if (timeAgo.TotalHours < 24)
                return $"{(int)timeAgo.TotalHours}h ago";
            if (timeAgo.TotalDays < 7)
                return $"{(int)timeAgo.TotalDays}d ago";
            return date.ToString("MMM dd");
        }

        private string FormatReleaseDate(DateTime? plannedDate, DateTime? actualDate)
        {
            if (actualDate.HasValue)
                return $"Released {actualDate.Value:MMM dd, yyyy}";
            if (plannedDate.HasValue)
                return $"Planned {plannedDate.Value:MMM dd, yyyy}";
            return "TBD";
        }

        private string CalculateTimeToRelease(DateTime? plannedDate)
        {
            if (!plannedDate.HasValue)
                return "TBD";

            var timeToRelease = plannedDate.Value - DateTime.UtcNow;
            if (timeToRelease.TotalDays < 0)
                return "Overdue";
            if (timeToRelease.TotalDays < 7)
                return $"{(int)timeToRelease.TotalDays} days";
            if (timeToRelease.TotalDays < 30)
                return $"{(int)(timeToRelease.TotalDays / 7)} weeks";
            return $"{(int)(timeToRelease.TotalDays / 30)} months";
        }

        #endregion

        #region Fallback Data Methods

        private RoadmapDataModel GetFallbackRoadmapData()
        {
            return new RoadmapDataModel
            {
                CurrentMilestones = GetFallbackCurrentMilestones(),
                UpcomingMilestones = GetFallbackUpcomingMilestones(),
                CompletedMilestones = GetFallbackCompletedMilestones(),
                DevelopmentStats = GetFallbackDevelopmentStats(),
                Releases = GetFallbackReleases(),
                RecentUpdates = GetFallbackUpdates(),
                GitHubStats = GetFallbackGitHubStats()
            };
        }

        private List<MilestoneDisplayModel> GetFallbackCurrentMilestones()
        {
            return new List<MilestoneDisplayModel>
            {
                new MilestoneDisplayModel
                {
                    Id = 1,
                    Title = "Teacher Verification System",
                    Description = "Build comprehensive teacher verification and onboarding system",
                    Category = "Core Platform",
                    Status = "InProgress",
                    StatusClass = "in-progress",
                    Priority = "High",
                    PriorityClass = "high",
                    ProgressPercentage = 75,
                    StartDate = DateTime.UtcNow.AddDays(-30),
                    EstimatedCompletionDate = DateTime.UtcNow.AddDays(15),
                    DurationEstimate = "6 weeks",
                    TimeRemaining = "2 weeks",
                    CompletedTasksCount = 6,
                    TotalTasksCount = 8
                },
                new MilestoneDisplayModel
                {
                    Id = 2,
                    Title = "Payment Processing Integration",
                    Description = "Integrate multiple payment processors for global accessibility",
                    Category = "Payment Systems",
                    Status = "Testing",
                    StatusClass = "testing",
                    Priority = "High",
                    PriorityClass = "high",
                    ProgressPercentage = 90,
                    StartDate = DateTime.UtcNow.AddDays(-45),
                    EstimatedCompletionDate = DateTime.UtcNow.AddDays(5),
                    DurationEstimate = "7 weeks",
                    TimeRemaining = "5 days",
                    CompletedTasksCount = 9,
                    TotalTasksCount = 10
                }
            };
        }

        private List<MilestoneDisplayModel> GetFallbackUpcomingMilestones()
        {
            return new List<MilestoneDisplayModel>
            {
                new MilestoneDisplayModel
                {
                    Id = 3,
                    Title = "Mobile Application Development",
                    Description = "Native mobile apps for iOS and Android",
                    Category = "Mobile",
                    Status = "Planning",
                    StatusClass = "planning",
                    Priority = "Medium",
                    PriorityClass = "medium",
                    ProgressPercentage = 0,
                    EstimatedCompletionDate = DateTime.UtcNow.AddDays(90),
                    DurationEstimate = "12 weeks",
                    TimeRemaining = "3 months",
                    CompletedTasksCount = 0,
                    TotalTasksCount = 15
                }
            };
        }

        private List<MilestoneDisplayModel> GetFallbackCompletedMilestones()
        {
            return new List<MilestoneDisplayModel>
            {
                new MilestoneDisplayModel
                {
                    Id = 4,
                    Title = "Smart Contract Development",
                    Description = "Core smart contracts for token mechanics",
                    Category = "Blockchain",
                    Status = "Completed",
                    StatusClass = "completed",
                    Priority = "Critical",
                    PriorityClass = "critical",
                    ProgressPercentage = 100,
                    StartDate = DateTime.UtcNow.AddDays(-120),
                    EstimatedCompletionDate = DateTime.UtcNow.AddDays(-90),
                    ActualCompletionDate = DateTime.UtcNow.AddDays(-85),
                    DurationEstimate = "4 weeks",
                    CompletedTasksCount = 12,
                    TotalTasksCount = 12
                }
            };
        }

        private GitHubDevelopmentStatsModel GetFallbackDevelopmentStats()
        {
            return new GitHubDevelopmentStatsModel
            {
                TotalMilestones = 25,
                CompletedMilestones = 8,
                InProgressMilestones = 4,
                PlannedMilestones = 13,
                OverallProgress = 32,
                TotalTasks = 147,
                CompletedTasks = 89,
                BlockedTasks = 3,
                TaskCompletionRate = 60.5m,
                ActiveDevelopers = 6,
                AverageCompletionTime = 28.5m,
                CurrentSprintName = "Sprint 2024.2",
                CurrentSprintEndDate = DateTime.UtcNow.AddDays(14),
                LastUpdated = DateTime.UtcNow
            };
        }

        private List<ReleaseDisplayModel> GetFallbackReleases()
        {
            return new List<ReleaseDisplayModel>
            {
                new ReleaseDisplayModel
                {
                    Id = 1,
                    Name = "TeacherSupport MVP",
                    Version = "v1.0.0",
                    Description = "Initial platform launch with core features",
                    Type = "Major",
                    TypeClass = "major",
                    Status = "InDevelopment",
                    StatusClass = "in-development",
                    PlannedReleaseDate = DateTime.UtcNow.AddDays(60),
                    FormattedReleaseDate = "Planned Q2 2024",
                    TimeToRelease = "2 months",
                    IncludedMilestones = new List<string> { "Teacher Verification", "Payment Processing", "Core Platform" }
                }
            };
        }

        private List<UpdateDisplayModel> GetFallbackUpdates()
        {
            return new List<UpdateDisplayModel>
            {
                new UpdateDisplayModel
                {
                    Id = 1,
                    Title = "Payment gateway integration completed",
                    Content = "Successfully integrated Stripe and PayPal for global payment processing",
                    Type = "Completion",
                    TypeClass = "completion",
                    ProgressChange = 15,
                    AuthorName = "Development Team",
                    CreatedAt = DateTime.UtcNow.AddHours(-2),
                    MilestoneTitle = "Payment Processing Integration",
                    FormattedDate = DateTime.UtcNow.AddHours(-2).ToString("MMM dd, yyyy"),
                    TimeAgo = "2h ago"
                }
            };
        }

        private GitHubStatsModel GetFallbackGitHubStats()
        {
            return new GitHubStatsModel
            {
                TotalCommits = 486,
                CommitsThisWeek = 23,
                OpenIssues = 12,
                ClosedIssues = 89,
                OpenPullRequests = 4,
                MergedPullRequests = 67,
                Contributors = 6,
                LastUpdated = DateTime.UtcNow
            };
        }

        #endregion
    }
}