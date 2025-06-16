using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Data.Enum;
using TeachCrowdSale.Core.Interfaces.Repositories;
using TeachCrowdSale.Core.Models.Request;
using TeachCrowdSale.Core.Models.Response;
using TeachCrowdSale.Infrastructure.Repositories;
using TaskStatus = TeachCrowdSale.Core.Data.Enum.TaskStatus;

namespace TeachCrowdSale.Infrastructure.Services
{

    public class RoadmapService : IRoadmapService
    {
        private readonly IRoadmapRepository _repository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<RoadmapService> _logger;

        // Cache keys
        private const string MILESTONES_CACHE_KEY_PREFIX = "milestones_";
        private const string MILESTONE_DETAILS_CACHE_KEY_PREFIX = "milestone_details_";
        private const string PROGRESS_SUMMARY_CACHE_KEY = "progress_summary";
        private const string GITHUB_STATS_CACHE_KEY = "github_stats";
        private const string DEV_STATS_CACHE_KEY = "dev_stats";
        private const string RECENT_RELEASES_CACHE_KEY = "recent_releases";

        // Cache durations
        private readonly TimeSpan _shortCacheDuration = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _mediumCacheDuration = TimeSpan.FromMinutes(15);
        private readonly TimeSpan _longCacheDuration = TimeSpan.FromHours(1);

        public RoadmapService(
            IRoadmapRepository repository,
            IMemoryCache cache,
            ILogger<RoadmapService> logger)
        {
            _repository = repository;
            _cache = cache;
            _logger = logger;
        }

        #region Read Operations

        public async Task<IEnumerable<MilestoneResponse>> GetMilestonesByStatusAsync(MilestoneStatus status)
        {
            var cacheKey = $"{MILESTONES_CACHE_KEY_PREFIX}status_{status}";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<MilestoneResponse>? cached) && cached != null)
            {
                _logger.LogDebug("Returning cached milestones for status: {Status}", status);
                return cached;
            }

            _logger.LogInformation("Fetching milestones for status: {Status}", status);

            var milestones = await _repository.GetMilestonesByStatusAsync(status);

            _cache.Set(cacheKey, milestones, _mediumCacheDuration);

            return milestones;
        }

        public async Task<IEnumerable<MilestoneResponse>> GetFilteredMilestonesAsync(string? status, string? category, string? priority)
        {
            var cacheKey = $"{MILESTONES_CACHE_KEY_PREFIX}filtered_{status}_{category}_{priority}";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<MilestoneResponse>? cached) && cached != null)
            {
                _logger.LogDebug("Returning cached filtered milestones");
                return cached;
            }

            _logger.LogInformation("Fetching filtered milestones - Status: {Status}, Category: {Category}, Priority: {Priority}",
                status, category, priority);

            var milestones = await _repository.GetFilteredMilestonesAsync(status, category, priority);

            _cache.Set(cacheKey, milestones, _mediumCacheDuration);

            return milestones;
        }

        public async Task<MilestoneResponse?> GetMilestoneWithDetailsAsync(int milestoneId)
        {
            var cacheKey = $"{MILESTONE_DETAILS_CACHE_KEY_PREFIX}{milestoneId}";

            if (_cache.TryGetValue(cacheKey, out MilestoneResponse? cached) && cached != null)
            {
                _logger.LogDebug("Returning cached milestone details for ID: {MilestoneId}", milestoneId);
                return cached;
            }

            _logger.LogInformation("Fetching milestone details for ID: {MilestoneId}", milestoneId);

            var milestone = await _repository.GetMilestoneWithDetailsAsync(milestoneId);

            if (milestone != null)
            {
                _cache.Set(cacheKey, milestone, _shortCacheDuration);
            }

            return milestone;
        }

        public async Task<IEnumerable<TaskResponse>> GetTasksByMilestoneAsync(int milestoneId)
        {
            var cacheKey = $"tasks_milestone_{milestoneId}";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<TaskResponse>? cached) && cached != null)
            {
                return cached;
            }

            var tasks = await _repository.GetTasksByMilestoneAsync(milestoneId);

            _cache.Set(cacheKey, tasks, _shortCacheDuration);

            return tasks;
        }

        public async Task<IEnumerable<UpdateResponse>> GetUpdatesByMilestoneAsync(int milestoneId)
        {
            var cacheKey = $"updates_milestone_{milestoneId}";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<UpdateResponse>? cached) && cached != null)
            {
                return cached;
            }

            var updates = await _repository.GetUpdatesByMilestoneAsync(milestoneId);

            _cache.Set(cacheKey, updates, _shortCacheDuration);

            return updates;
        }

        public async Task<IEnumerable<ReleaseResponse>> GetRecentReleasesAsync(int count)
        {
            var cacheKey = $"{RECENT_RELEASES_CACHE_KEY}_{count}";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<ReleaseResponse>? cached) && cached != null)
            {
                return cached;
            }

            var releases = await _repository.GetRecentReleasesAsync(count);

            _cache.Set(cacheKey, releases, _longCacheDuration);

            return releases;
        }

        public async Task<ProgressSummaryResponse> GetProgressSummaryAsync()
        {
            if (_cache.TryGetValue(PROGRESS_SUMMARY_CACHE_KEY, out ProgressSummaryResponse? cached) && cached != null)
            {
                return cached;
            }

            _logger.LogInformation("Calculating progress summary");

            var progressSummary = await CalculateProgressSummaryAsync();

            _cache.Set(PROGRESS_SUMMARY_CACHE_KEY, progressSummary, _mediumCacheDuration);

            return progressSummary;
        }

        public async Task<GitHubStatsResponse> GetGitHubStatsAsync()
        {
            if (_cache.TryGetValue(GITHUB_STATS_CACHE_KEY, out GitHubStatsResponse? cached) && cached != null)
            {
                return cached;
            }

            _logger.LogInformation("Fetching GitHub statistics");

            var githubStats = await FetchGitHubStatsAsync();

            _cache.Set(GITHUB_STATS_CACHE_KEY, githubStats, _longCacheDuration);

            return githubStats;
        }

        public async Task<DevelopmentStatsResponse> GetDevelopmentStatsAsync()
        {
            if (_cache.TryGetValue(DEV_STATS_CACHE_KEY, out DevelopmentStatsResponse? cached) && cached != null)
            {
                return cached;
            }

            _logger.LogInformation("Calculating development statistics");

            var devStats = await CalculateDevelopmentStatsAsync();

            _cache.Set(DEV_STATS_CACHE_KEY, devStats, _longCacheDuration);

            return devStats;
        }

        #endregion

        #region Write Operations

        public async Task<Milestone> CreateMilestoneAsync(MilestoneRequest request)
        {
            _logger.LogInformation("Creating new milestone: {Title}", request.Title);

            var milestone = new MilestoneResponse
            {
                Title = request.Title,
                Description = request.Description,
                Category = request.Category,
                Status = request.Status,
                Priority = request.Priority,
                ProgressPercentage = request.ProgressPercentage,
                StartDate = request.StartDate,
                EstimatedCompletionDate = request.EstimatedCompletionDate,
                ActualCompletionDate = request.ActualCompletionDate,
                DurationEstimate = request.DurationEstimate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdMilestone = await _repository.CreateMilestoneAsync(milestone);

            // Create associated tasks if provided
            if (request.Tasks?.Any() == true)
            {
                foreach (var taskRequest in request.Tasks)
                {
                    taskRequest.MilestoneId = createdMilestone.Id;
                    await CreateTaskAsync(taskRequest);
                }
            }

            // Create dependencies if provided
            if (request.DependencyIds?.Any() == true)
            {
                foreach (var dependencyId in request.DependencyIds)
                {
                    var dependencyRequest = new DependencyRequest
                    {
                        MilestoneId = dependencyId,
                        DependentMilestoneId = createdMilestone.Id,
                        DependencyType = DependencyType.Prerequisite,
                        IsActive = true
                    };
                    await CreateDependencyAsync(dependencyRequest);
                }
            }

            // Clear relevant caches
            ClearMilestonesCaches();

            return createdMilestone;
        }

        public async Task<MilestoneResponse?> UpdateMilestoneAsync(int milestoneId, MilestoneRequest request)
        {
            _logger.LogInformation("Updating milestone {MilestoneId}: {Title}", milestoneId, request.Title);

            var existingMilestone = await _repository.GetMilestoneByIdAsync(milestoneId);
            if (existingMilestone == null)
            {
                _logger.LogWarning("Milestone {MilestoneId} not found for update", milestoneId);
                return null;
            }

            existingMilestone.Title = request.Title;
            existingMilestone.Description = request.Description;
            existingMilestone.Category = request.Category;
            existingMilestone.Status = request.Status;
            existingMilestone.Priority = request.Priority;
            existingMilestone.ProgressPercentage = request.ProgressPercentage;
            existingMilestone.StartDate = request.StartDate;
            existingMilestone.EstimatedCompletionDate = request.EstimatedCompletionDate;
            existingMilestone.ActualCompletionDate = request.ActualCompletionDate;
            existingMilestone.DurationEstimate = request.DurationEstimate;
            existingMilestone.UpdatedAt = DateTime.UtcNow;

            var updatedMilestone = await _repository.UpdateMilestoneAsync(existingMilestone);

            // Clear relevant caches
            ClearMilestonesCaches();
            _cache.Remove($"{MILESTONE_DETAILS_CACHE_KEY_PREFIX}{milestoneId}");

            return updatedMilestone;
        }

        public async Task<bool> DeleteMilestoneAsync(int milestoneId)
        {
            _logger.LogInformation("Deleting milestone {MilestoneId}", milestoneId);

            var result = await _repository.DeleteMilestoneAsync(milestoneId);

            if (result)
            {
                // Clear relevant caches
                ClearMilestonesCaches();
                _cache.Remove($"{MILESTONE_DETAILS_CACHE_KEY_PREFIX}{milestoneId}");
            }

            return result;
        }

        public async Task<TaskResponse> CreateTaskAsync(TaskRequest request)
        {
            _logger.LogInformation("Creating new task: {Title} for milestone {MilestoneId}", request.Title, request.MilestoneId);

            var task = new TaskResponse
            {
                Title = request.Title,
                Description = request.Description,
                Status = request.Status,
                Priority = request.Priority,
                ProgressPercentage = request.ProgressPercentage,
                StartDate = request.StartDate,
                DueDate = request.DueDate,
                CompletionDate = request.CompletionDate,
                MilestoneId = request.MilestoneId,
                Assignee = request.Assignee,
                EstimatedHours = request.EstimatedHours,
                ActualHours = request.ActualHours,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdTask = await _repository.CreateTaskAsync(task);

            // Clear relevant caches
            _cache.Remove($"tasks_milestone_{request.MilestoneId}");
            _cache.Remove($"{MILESTONE_DETAILS_CACHE_KEY_PREFIX}{request.MilestoneId}");

            return createdTask;
        }

        public async Task<TaskResponse?> UpdateTaskAsync(int taskId, TaskRequest request)
        {
            _logger.LogInformation("Updating task {TaskId}: {Title}", taskId, request.Title);

            var existingTask = await _repository.GetTaskByIdAsync(taskId);
            if (existingTask == null)
            {
                _logger.LogWarning("Task {TaskId} not found for update", taskId);
                return null;
            }

            existingTask.Title = request.Title;
            existingTask.Description = request.Description;
            existingTask.Status = request.Status;
            existingTask.Priority = request.Priority;
            existingTask.ProgressPercentage = request.ProgressPercentage;
            existingTask.StartDate = request.StartDate;
            existingTask.DueDate = request.DueDate;
            existingTask.CompletionDate = request.CompletionDate;
            existingTask.Assignee = request.Assignee;
            existingTask.EstimatedHours = request.EstimatedHours;
            existingTask.ActualHours = request.ActualHours;
            existingTask.UpdatedAt = DateTime.UtcNow;

            var updatedTask = await _repository.UpdateTaskAsync(existingTask);

            // Clear relevant caches
            _cache.Remove($"tasks_milestone_{existingTask.MilestoneId}");
            _cache.Remove($"{MILESTONE_DETAILS_CACHE_KEY_PREFIX}{existingTask.MilestoneId}");

            return updatedTask;
        }

        public async Task<UpdateResponse> CreateUpdateAsync(UpdateRequest request)
        {
            _logger.LogInformation("Creating new update: {Title} for milestone {MilestoneId}", request.Title, request.MilestoneId);

            var update = new UpdateResponse
            {
                Title = request.Title,
                Content = request.Content,
                UpdateType = request.UpdateType,
                MilestoneId = request.MilestoneId,
                Author = request.Author,
                Tags = request.Tags,
                Attachments = request.Attachments,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdUpdate = await _repository.CreateUpdateAsync(update);

            // Clear relevant caches
            _cache.Remove($"updates_milestone_{request.MilestoneId}");
            _cache.Remove($"{MILESTONE_DETAILS_CACHE_KEY_PREFIX}{request.MilestoneId}");

            return createdUpdate;
        }

        public async Task<ReleaseResponse> CreateReleaseAsync(ReleaseRequest request)
        {
            _logger.LogInformation("Creating new release: {Version} - {Title}", request.Version, request.Title);

            var release = new ReleaseResponse
            {
                Version = request.Version,
                Title = request.Title,
                Description = request.Description,
                ReleaseDate = request.ReleaseDate,
                ReleaseType = request.ReleaseType,
                IsPreRelease = request.IsPreRelease,
                IsDraft = request.IsDraft,
                TagName = request.TagName,
                GitHubUrl = request.GitHubUrl,
                DownloadUrl = request.DownloadUrl,
                ReleaseNotes = request.ReleaseNotes,
                Assets = request.Assets,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdRelease = await _repository.CreateReleaseAsync(release);

            // Clear releases cache
            ClearReleasesCaches();

            return createdRelease;
        }

        public async Task<DependencyResponse> CreateDependencyAsync(DependencyRequest request)
        {
            _logger.LogInformation("Creating dependency: Milestone {MilestoneId} -> {DependentMilestoneId}",
                request.MilestoneId, request.DependentMilestoneId);

            var dependency = new DependencyResponse
            {
                MilestoneId = request.MilestoneId,
                DependentMilestoneId = request.DependentMilestoneId,
                DependencyType = request.DependencyType,
                Description = request.Description,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdDependency = await _repository.CreateDependencyAsync(dependency);

            // Clear relevant caches
            _cache.Remove($"{MILESTONE_DETAILS_CACHE_KEY_PREFIX}{request.MilestoneId}");
            _cache.Remove($"{MILESTONE_DETAILS_CACHE_KEY_PREFIX}{request.DependentMilestoneId}");

            return createdDependency;
        }

        #endregion

        #region Private Helper Methods

        private async Task<ProgressSummaryResponse> CalculateProgressSummaryAsync()
        {
            var allMilestones = await _repository.GetAllMilestonesAsync();
            var allTasks = await _repository.GetAllTasksAsync();

            var totalMilestones = allMilestones.Count();
            var completedMilestones = allMilestones.Count(m => m.Status == MilestoneStatus.Completed);
            var inProgressMilestones = allMilestones.Count(m => m.Status == MilestoneStatus.InProgress);
            var upcomingMilestones = allMilestones.Count(m => m.Status == MilestoneStatus.Planning);

            var totalTasks = allTasks.Count();
            var completedTasks = allTasks.Count(t => t.Status == TaskStatus.Completed);
            var activeTasks = allTasks.Count(t => t.Status == TaskStatus.InProgress);
            var overdueTasks = allTasks.Count(t => t.DueDate.HasValue && t.DueDate < DateTime.UtcNow && t.Status != TaskStatus.Completed);

            var overallProgress = totalMilestones > 0 ? (decimal)completedMilestones / totalMilestones * 100 : 0;

            // Calculate average completion time
            var completedMilestonesWithDates = allMilestones
                .Where(m => m.Status == MilestoneStatus.Completed && m.StartDate.HasValue && m.ActualCompletionDate.HasValue)
                .ToList();

            var averageCompletionTime = "N/A";
            if (completedMilestonesWithDates.Any())
            {
                var avgDays = completedMilestonesWithDates
                    .Average(m => (m.ActualCompletionDate!.Value - m.StartDate!.Value).TotalDays);
                averageCompletionTime = $"{avgDays:F1} days";
            }

            // Find next milestone
            var nextMilestone = allMilestones
                .Where(m => m.Status == MilestoneStatus.Planning || m.Status == MilestoneStatus.InProgress)
                .OrderBy(m => m.EstimatedCompletionDate ?? DateTime.MaxValue)
                .FirstOrDefault();

            // Estimate project completion
            var remainingMilestones = allMilestones.Where(m => m.Status != MilestoneStatus.Completed);
            DateTime? estimatedCompletion = null;
            if (remainingMilestones.Any())
            {
                estimatedCompletion = remainingMilestones
                    .Where(m => m.EstimatedCompletionDate.HasValue)
                    .Max(m => m.EstimatedCompletionDate);
            }

            return new ProgressSummaryResponse
            {
                OverallProgress = overallProgress,
                TotalMilestones = totalMilestones,
                CompletedMilestones = completedMilestones,
                InProgressMilestones = inProgressMilestones,
                UpcomingMilestones = upcomingMilestones,
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                ActiveTasks = activeTasks,
                OverdueTasks = overdueTasks,
                AverageCompletionTime = averageCompletionTime,
                CurrentPhase = DetermineCurrentPhase(overallProgress),
                NextMilestone = nextMilestone?.Title ?? "No upcoming milestones",
                EstimatedProjectCompletion = estimatedCompletion
            };
        }

        private async Task<GitHubStatsResponse> FetchGitHubStatsAsync()
        {
            // TODO: Implement actual GitHub API integration
            // For now, return mock data that matches the expected structure
            return new GitHubStatsResponse
            {
                TotalCommits = 1247,
                CommitsThisMonth = 89,
                CommitsThisWeek = 23,
                TotalContributors = 8,
                ActiveContributors = 4,
                TotalPullRequests = 156,
                OpenPullRequests = 7,
                MergedPullRequests = 142,
                TotalIssues = 89,
                OpenIssues = 12,
                ClosedIssues = 77,
                CodeFrequency = 2840,
                LastCommitDate = DateTime.UtcNow.AddHours(-3),
                LastCommitMessage = "feat: Add roadmap dashboard components",
                LastCommitAuthor = "TeachToken Dev Team",
                RepositoryUrl = "https://github.com/buckhoff/TeachCrowdSale",
                DefaultBranch = "main",
                LinesOfCode = 47500,
                StarCount = 23,
                ForkCount = 8,
                WatcherCount = 15
            };
        }

        private async Task<DevelopmentStatsResponse> CalculateDevelopmentStatsAsync()
        {
            // TODO: Implement actual development metrics calculation
            // For now, return mock data that matches the expected structure
            return new DevelopmentStatsResponse
            {
                TotalLinesOfCode = 47500,
                FilesChanged = 234,
                CommitsThisWeek = 23,
                CommitsThisMonth = 89,
                ActiveBranches = 8,
                CodeCoverage = 78.5m,
                TestsCount = 312,
                PassingTests = 298,
                FailingTests = 3,
                BuildStatus = "Passing",
                LastBuildDate = DateTime.UtcNow.AddHours(-2),
                AverageCommitSize = 127,
                TopContributors = new List<string> { "buckhoff", "dev-team-lead", "frontend-dev" },
                MostActiveRepository = "TeachCrowdSale",
                TotalRepositories = 4,
                OpenPullRequests = 7,
                CodeReviewsCompleted = 45,
                DeploymentFrequency = "Daily",
                LastDeploymentDate = DateTime.UtcNow.AddHours(-6),
                TechnicalDebtRatio = 12.3m
            };
        }

        private string DetermineCurrentPhase(decimal overallProgress)
        {
            return overallProgress switch
            {
                < 25 => "Foundation Phase",
                < 50 => "Development Phase 1",
                < 75 => "Development Phase 2",
                < 90 => "Testing & Refinement",
                < 100 => "Launch Preparation",
                _ => "Maintenance & Growth"
            };
        }

        private void ClearMilestonesCaches()
        {
            // Clear all milestone-related caches
            var cacheKeys = new[]
            {
                $"{MILESTONES_CACHE_KEY_PREFIX}status_Completed",
                $"{MILESTONES_CACHE_KEY_PREFIX}status_InProgress",
                $"{MILESTONES_CACHE_KEY_PREFIX}status_Planning",
                $"{MILESTONES_CACHE_KEY_PREFIX}status_OnHold",
                $"{MILESTONES_CACHE_KEY_PREFIX}status_Cancelled",
                PROGRESS_SUMMARY_CACHE_KEY
            };

            foreach (var key in cacheKeys)
            {
                _cache.Remove(key);
            }

            // Clear filtered cache entries (this is a simplified approach)
            // In production, you might want to use a more sophisticated cache invalidation strategy
            _logger.LogDebug("Cleared milestone caches");
        }

        private void ClearReleasesCaches()
        {
            var cacheKeys = new[]
            {
                $"{RECENT_RELEASES_CACHE_KEY}_5",
                $"{RECENT_RELEASES_CACHE_KEY}_10",
                $"{RECENT_RELEASES_CACHE_KEY}_20"
            };

            foreach (var key in cacheKeys)
            {
                _cache.Remove(key);
            }

            _logger.LogDebug("Cleared release caches");
        }

        #endregion
    }
}