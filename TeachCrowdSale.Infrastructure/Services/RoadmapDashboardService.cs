using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models.Response;
using TeachCrowdSale.Core.Models;

public class RoadmapDashboardService : IRoadmapDashboardService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RoadmapDashboardService> _logger;

    // Cache keys - following existing pattern from home page
    private const string ROADMAP_DATA_CACHE_KEY = "roadmap_data";
    private const string MILESTONE_CACHE_KEY_PREFIX = "milestone_";
    private const string PROGRESS_SUMMARY_CACHE_KEY = "progress_summary";
    private const string GITHUB_STATS_CACHE_KEY = "github_stats";
    private const string RECENT_RELEASES_CACHE_KEY = "recent_releases";
    private const string DEV_STATS_CACHE_KEY = "development_stats";

    // Cache durations - following existing pattern
    private readonly TimeSpan _shortCacheDuration = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _mediumCacheDuration = TimeSpan.FromMinutes(15);
    private readonly TimeSpan _longCacheDuration = TimeSpan.FromHours(1);

    public RoadmapDashboardService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<RoadmapDashboardService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<RoadmapPageModel> GetRoadmapPageDataAsync()
    {
        try
        {
            // Check cache first
            if (_cache.TryGetValue(ROADMAP_DATA_CACHE_KEY, out RoadmapPageModel? cachedData) && cachedData != null)
            {
                _logger.LogDebug("Returning cached roadmap page data");
                return cachedData;
            }

            _logger.LogInformation("Fetching roadmap page data from API");

            // Fetch data from API
            var response = await _httpClient.GetAsync("/api/roadmap/data");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<RoadmapResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse == null)
            {
                _logger.LogWarning("API returned null response for roadmap data");
                return GetFallbackRoadmapData();
            }

            // Transform API response to web display model
            var pageModel = new RoadmapPageModel
            {
                ProgressSummary = TransformProgressSummary(apiResponse.ProgressSummary),
                ActiveMilestones = apiResponse.ActiveMilestones?.Select(TransformMilestone).ToList() ?? new List<MilestoneModel>(),
                UpcomingMilestones = apiResponse.UpcomingMilestones?.Select(TransformMilestone).ToList() ?? new List<MilestoneModel>(),
                CompletedMilestones = apiResponse.CompletedMilestones?.Select(TransformMilestone).ToList() ?? new List<MilestoneModel>(),
                RecentReleases = apiResponse.RecentReleases?.Select(TransformRelease).ToList() ?? new List<ReleaseModel>(),
                GitHubStats = TransformGitHubStats(apiResponse.GitHubStats),
                DevelopmentStats = TransformDevelopmentStats(apiResponse.DevelopmentStats),
                LastUpdated = DateTime.UtcNow
            };

            // Cache with medium duration
            _cache.Set(ROADMAP_DATA_CACHE_KEY, pageModel, _mediumCacheDuration);

            _logger.LogInformation("Successfully fetched and cached roadmap page data");
            return pageModel;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching roadmap data from API");
            return GetFallbackRoadmapData();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching roadmap data");
            return GetFallbackRoadmapData();
        }
    }

    public async Task<MilestoneModel?> GetMilestoneDetailsAsync(int milestoneId)
    {
        var cacheKey = $"{MILESTONE_CACHE_KEY_PREFIX}{milestoneId}";

        try
        {
            // Check cache first
            if (_cache.TryGetValue(cacheKey, out MilestoneModel? cachedMilestone) && cachedMilestone != null)
            {
                return cachedMilestone;
            }

            _logger.LogInformation("Fetching milestone {MilestoneId} from API", milestoneId);

            var response = await _httpClient.GetAsync($"/api/roadmap/milestones/{milestoneId}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<MilestoneResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse == null)
            {
                _logger.LogWarning("API returned null response for milestone {MilestoneId}", milestoneId);
                return null;
            }

            var milestoneModel = TransformMilestone(apiResponse);

            // Cache with short duration
            _cache.Set(cacheKey, milestoneModel, _shortCacheDuration);

            return milestoneModel;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching milestone {MilestoneId} from API", milestoneId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching milestone {MilestoneId}", milestoneId);
            return null;
        }
    }

    public async Task<IEnumerable<MilestoneModel>> GetFilteredMilestonesAsync(string? status, string? category)
    {
        try
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(status))
                queryParams.Add($"status={Uri.EscapeDataString(status)}");
            if (!string.IsNullOrEmpty(category))
                queryParams.Add($"category={Uri.EscapeDataString(category)}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetAsync($"/api/roadmap/milestones{queryString}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<IEnumerable<MilestoneResponse>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return apiResponse?.Select(TransformMilestone) ?? Enumerable.Empty<MilestoneModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching filtered milestones");
            return Enumerable.Empty<MilestoneModel>();
        }
    }

    public async Task<ProgressSummaryModel> GetProgressSummaryAsync()
    {
        try
        {
            if (_cache.TryGetValue(PROGRESS_SUMMARY_CACHE_KEY, out ProgressSummaryModel? cached) && cached != null)
            {
                return cached;
            }

            var response = await _httpClient.GetAsync("/api/roadmap/progress");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ProgressSummaryResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var progressSummary = TransformProgressSummary(apiResponse);
            _cache.Set(PROGRESS_SUMMARY_CACHE_KEY, progressSummary, _mediumCacheDuration);

            return progressSummary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching progress summary");
            return GetFallbackProgressSummary();
        }
    }

    public async Task<GitHubStatsModel> GetGitHubStatsAsync()
    {
        try
        {
            if (_cache.TryGetValue(GITHUB_STATS_CACHE_KEY, out GitHubStatsModel? cached) && cached != null)
            {
                return cached;
            }

            var response = await _httpClient.GetAsync("/api/roadmap/github-stats");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<GitHubStatsResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var githubStats = TransformGitHubStats(apiResponse);
            _cache.Set(GITHUB_STATS_CACHE_KEY, githubStats, _longCacheDuration);

            return githubStats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching GitHub stats");
            return GetFallbackGitHubStats();
        }
    }

    public async Task<IEnumerable<ReleaseModel>> GetRecentReleasesAsync(int count = 5)
    {
        try
        {
            if (_cache.TryGetValue(RECENT_RELEASES_CACHE_KEY, out IEnumerable<ReleaseModel>? cached) && cached != null)
            {
                return cached.Take(count);
            }

            var response = await _httpClient.GetAsync($"/api/roadmap/releases?limit={count}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<IEnumerable<ReleaseResponse>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var releases = apiResponse?.Select(TransformRelease) ?? Enumerable.Empty<ReleaseModel>();
            _cache.Set(RECENT_RELEASES_CACHE_KEY, releases, _mediumCacheDuration);

            return releases.Take(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching recent releases");
            return GetFallbackReleases();
        }
    }

    public async Task<DevelopmentStatsModel> GetDevelopmentStatsAsync()
    {
        try
        {
            if (_cache.TryGetValue(DEV_STATS_CACHE_KEY, out DevelopmentStatsModel? cached) && cached != null)
            {
                return cached;
            }

            var response = await _httpClient.GetAsync("/api/roadmap/development-stats");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<DevelopmentStatsResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var devStats = TransformDevelopmentStats(apiResponse);
            _cache.Set(DEV_STATS_CACHE_KEY, devStats, _longCacheDuration);

            return devStats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching development stats");
            return GetFallbackDevelopmentStats();
        }
    }

    #region Transformation Methods

    private MilestoneModel TransformMilestone(MilestoneResponse response)
    {
        var statusString = response.Status.ToString();
        var priorityString = response.Priority.ToString();

        return new MilestoneModel
        {
            Id = response.Id,
            Title = response.Title,
            Description = response.Description,
            Category = response.Category,
            Status = statusString,
            StatusClass = GetStatusClass(statusString),
            Priority = priorityString,
            PriorityClass = GetPriorityClass(priorityString),
            ProgressPercentage = response.ProgressPercentage,
            StartDate = response.StartDate,
            EstimatedCompletionDate = response.EstimatedCompletionDate,
            ActualCompletionDate = response.ActualCompletionDate,
            DurationEstimate = response.DurationEstimate,
            TimeRemaining = CalculateTimeRemaining(response.EstimatedCompletionDate, statusString),
            CompletedTasksCount = response.CompletedTasksCount,
            TotalTasksCount = response.TotalTasksCount,
            CreatedAt = response.CreatedAt,
            UpdatedAt = response.UpdatedAt,
            Tasks = response.Tasks?.Select(TransformTask).ToList(),
            Dependencies = response.Dependencies?.Select(TransformDependency).ToList(),
            Updates = response.Updates?.Select(TransformUpdate).ToList()
        };
    }

    private TaskModel TransformTask(TaskResponse response)
    {
        var statusString = response.Status.ToString();
        var priorityString = response.Priority.ToString();

        return new TaskModel
        {
            Id = response.Id,
            Title = response.Title,
            Description = response.Description,
            Status = statusString,
            StatusClass = GetStatusClass(statusString),
            Priority = priorityString,
            PriorityClass = GetPriorityClass(priorityString),
            ProgressPercentage = response.ProgressPercentage,
            StartDate = response.StartDate,
            DueDate = response.DueDate,
            CompletionDate = response.CompletionDate,
            MilestoneId = response.MilestoneId,
            MilestoneTitle = response.MilestoneTitle,
            Assignee = response.Assignee,
            EstimatedHours = response.EstimatedHours,
            ActualHours = response.ActualHours,
            CreatedAt = response.CreatedAt,
            UpdatedAt = response.UpdatedAt
        };
    }

    private UpdateModel TransformUpdate(UpdateResponse response)
    {
        var updateTypeString = response.UpdateType.ToString();

        return new UpdateModel
        {
            Id = response.Id,
            Title = response.Title,
            Content = response.Content,
            UpdateType = updateTypeString,
            UpdateTypeClass = GetUpdateTypeClass(updateTypeString),
            MilestoneId = response.MilestoneId,
            MilestoneTitle = response.MilestoneTitle,
            Author = response.Author,
            Tags = response.Tags,
            Attachments = response.Attachments,
            CreatedAt = response.CreatedAt,
            UpdatedAt = response.UpdatedAt
        };
    }

    private DependencyModel TransformDependency(DependencyResponse response)
    {
        var dependencyTypeString = response.DependencyType.ToString();

        return new DependencyModel
        {
            Id = response.Id,
            MilestoneId = response.MilestoneId,
            DependentMilestoneId = response.DependentMilestoneId,
            DependencyType = dependencyTypeString,
            DependencyTypeClass = GetDependencyTypeClass(dependencyTypeString),
            Description = response.Description,
            IsActive = response.IsActive,
            CreatedAt = response.CreatedAt,
            MilestoneTitle = response.MilestoneTitle,
            DependentMilestoneTitle = response.DependentMilestoneTitle
        };
    }

    private ProgressSummaryModel TransformProgressSummary(ProgressSummaryResponse? response)
    {
        if (response == null) return GetFallbackProgressSummary();

        return new ProgressSummaryModel
        {
            OverallProgress = response.OverallProgress,
            TotalMilestones = response.TotalMilestones,
            CompletedMilestones = response.CompletedMilestones,
            InProgressMilestones = response.InProgressMilestones,
            UpcomingMilestones = response.UpcomingMilestones,
            TotalTasks = response.TotalTasks,
            CompletedTasks = response.CompletedTasks,
            ActiveTasks = response.ActiveTasks,
            OverdueTasks = response.OverdueTasks,
            AverageCompletionTime = response.AverageCompletionTime,
            CurrentPhase = response.CurrentPhase,
            NextMilestone = response.NextMilestone,
            EstimatedProjectCompletion = response.EstimatedProjectCompletion
        };
    }

    private GitHubStatsModel TransformGitHubStats(GitHubStatsResponse? response)
    {
        if (response == null) return GetFallbackGitHubStats();

        return new GitHubStatsModel
        {
            TotalCommits = response.TotalCommits,
            CommitsThisMonth = response.CommitsThisMonth,
            CommitsThisWeek = response.CommitsThisWeek,
            TotalContributors = response.TotalContributors,
            ActiveContributors = response.ActiveContributors,
            TotalPullRequests = response.TotalPullRequests,
            OpenPullRequests = response.OpenPullRequests,
            MergedPullRequests = response.MergedPullRequests,
            TotalIssues = response.TotalIssues,
            OpenIssues = response.OpenIssues,
            ClosedIssues = response.ClosedIssues,
            CodeFrequency = response.CodeFrequency,
            LastCommitDate = response.LastCommitDate,
            LastCommitMessage = response.LastCommitMessage,
            LastCommitAuthor = response.LastCommitAuthor,
            RepositoryUrl = response.RepositoryUrl,
            DefaultBranch = response.DefaultBranch,
            LinesOfCode = response.LinesOfCode,
            StarCount = response.StarCount,
            ForkCount = response.ForkCount,
            WatcherCount = response.WatcherCount
        };
    }

    private ReleaseModel TransformRelease(ReleaseResponse response)
    {
        return new ReleaseModel
        {
            Id = response.Id,
            Version = response.Version,
            Title = response.Title,
            Description = response.Description,
            ReleaseDate = response.ReleaseDate,
            ReleaseType = response.ReleaseType,
            ReleaseTypeClass = GetReleaseTypeClass(response.ReleaseType),
            IsPreRelease = response.IsPreRelease,
            IsDraft = response.IsDraft,
            TagName = response.TagName,
            GitHubUrl = response.GitHubUrl,
            DownloadUrl = response.DownloadUrl,
            ReleaseNotes = response.ReleaseNotes,
            Assets = response.Assets,
            CreatedAt = response.CreatedAt
        };
    }

    private DevelopmentStatsModel TransformDevelopmentStats(DevelopmentStatsResponse? response)
    {
        if (response == null) return GetFallbackDevelopmentStats();

        return new DevelopmentStatsModel
        {
            TotalLinesOfCode = response.TotalLinesOfCode,
            FilesChanged = response.FilesChanged,
            CommitsThisWeek = response.CommitsThisWeek,
            CommitsThisMonth = response.CommitsThisMonth,
            ActiveBranches = response.ActiveBranches,
            CodeCoverage = response.CodeCoverage,
            TestsCount = response.TestsCount,
            PassingTests = response.PassingTests,
            FailingTests = response.FailingTests,
            BuildStatus = response.BuildStatus,
            LastBuildDate = response.LastBuildDate,
            AverageCommitSize = response.AverageCommitSize,
            TopContributors = response.TopContributors,
            MostActiveRepository = response.MostActiveRepository,
            TotalRepositories = response.TotalRepositories,
            OpenPullRequests = response.OpenPullRequests,
            CodeReviewsCompleted = response.CodeReviewsCompleted,
            DeploymentFrequency = response.DeploymentFrequency,
            LastDeploymentDate = response.LastDeploymentDate,
            TechnicalDebtRatio = response.TechnicalDebtRatio
        };
    }

    #endregion

    private string CalculateTimeRemaining(DateTime? estimatedCompletion, string status)
    {
        if (status.ToLower() == "completed")
            return "Completed";

        if (estimatedCompletion == null)
            return "TBD";

        var timeRemaining = estimatedCompletion.Value - DateTime.UtcNow;

        if (timeRemaining.TotalDays < 0)
            return "Overdue";

        if (timeRemaining.TotalDays < 1)
            return "Due today";

        if (timeRemaining.TotalDays < 7)
            return $"{(int)timeRemaining.TotalDays} days";

        if (timeRemaining.TotalDays < 30)
            return $"{(int)(timeRemaining.TotalDays / 7)} weeks";

        return $"{(int)(timeRemaining.TotalDays / 30)} months";
    }

    #region Fallback Data Methods

    private RoadmapPageModel GetFallbackRoadmapData()
    {
        return new RoadmapPageModel
        {
            ProgressSummary = GetFallbackProgressSummary(),
            ActiveMilestones = GetFallbackMilestones(),
            UpcomingMilestones = new List<MilestoneModel>(),
            CompletedMilestones = new List<MilestoneModel>(),
            RecentReleases = GetFallbackReleases(),
            GitHubStats = GetFallbackGitHubStats(),
            DevelopmentStats = GetFallbackDevelopmentStats(),
            LastUpdated = DateTime.UtcNow
        };
    }

    private ProgressSummaryModel GetFallbackProgressSummary()
    {
        return new ProgressSummaryModel
        {
            OverallProgress = 45.0m,
            TotalMilestones = 12,
            CompletedMilestones = 4,
            InProgressMilestones = 3,
            UpcomingMilestones = 5,
            TotalTasks = 84,
            CompletedTasks = 32,
            ActiveTasks = 15,
            OverdueTasks = 2,
            AverageCompletionTime = "3.2 weeks",
            CurrentPhase = "Development Phase 2",
            NextMilestone = "Staking Platform Launch",
            EstimatedProjectCompletion = DateTime.UtcNow.AddMonths(8)
        };
    }

    private List<MilestoneModel> GetFallbackMilestones()
    {
        return new List<MilestoneModel>
                {
                    new MilestoneModel
                    {
                        Id = 1,
                        Title = "Staking Platform Development",
                        Description = "Build comprehensive staking platform with education funding integration",
                        Category = "Platform",
                        Status = "In Progress",
                        StatusClass = "in-progress",
                        Priority = "High",
                        PriorityClass = "high",
                        ProgressPercentage = 75,
                        StartDate = DateTime.UtcNow.AddDays(-45),
                        EstimatedCompletionDate = DateTime.UtcNow.AddDays(15),
                        DurationEstimate = "8 weeks",
                        TimeRemaining = "2 weeks",
                        CompletedTasksCount = 12,
                        TotalTasksCount = 16,
                        CreatedAt = DateTime.UtcNow.AddDays(-45),
                        UpdatedAt = DateTime.UtcNow.AddDays(-1)
                    }
                };
    }

    private GitHubStatsModel GetFallbackGitHubStats()
    {
        return new GitHubStatsModel
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

    private List<ReleaseModel> GetFallbackReleases()
    {
        return new List<ReleaseModel>
                {
                    new ReleaseModel
                    {
                        Id = 1,
                        Version = "v1.2.0",
                        Title = "Staking Platform Beta",
                        Description = "Initial release of the staking platform with basic functionality",
                        ReleaseDate = DateTime.UtcNow.AddDays(-7),
                        ReleaseType = "Beta",
                        IsPreRelease = true,
                        IsDraft = false,
                        TagName = "v1.2.0-beta",
                        GitHubUrl = "https://github.com/buckhoff/TeachCrowdSale/releases/tag/v1.2.0-beta",
                        CreatedAt = DateTime.UtcNow.AddDays(-7)
                    }
                };
    }

    private DevelopmentStatsModel GetFallbackDevelopmentStats()
    {
        return new DevelopmentStatsModel
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
    #endregion
}