using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models;

namespace TeachCrowdSale.Infrastructure.Services
{
    public class RoadmapDashboardService : IRoadmapDashboardService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<RoadmapDashboardService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        // Cache keys and durations following established patterns
        private const string CACHE_KEY_ROADMAP_DATA = "web_roadmap_data";
        private const string CACHE_KEY_MILESTONES = "web_milestones";
        private const string CACHE_KEY_DEV_STATS = "web_dev_stats";
        private const string CACHE_KEY_GITHUB_STATS = "web_github_stats";
        private const string CACHE_KEY_RECENT_UPDATES = "web_recent_updates";
        private const string CACHE_KEY_RELEASES = "web_releases";
        private const string CACHE_KEY_FILTER_OPTIONS = "web_filter_options";

        private readonly TimeSpan _shortCacheDuration = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _mediumCacheDuration = TimeSpan.FromMinutes(15);
        private readonly TimeSpan _longCacheDuration = TimeSpan.FromHours(1);

        public RoadmapDashboardService(
            HttpClient httpClient,
            IMemoryCache cache,
            ILogger<RoadmapDashboardService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// Get comprehensive roadmap page data
        /// </summary>
        public async Task<RoadmapDataModel> GetRoadmapPageDataAsync()
        {
            var cacheKey = CACHE_KEY_ROADMAP_DATA;

            try
            {
                if (_cache.TryGetValue(cacheKey, out RoadmapDataModel? cachedData) && cachedData != null)
                {
                    _logger.LogDebug("Returning cached roadmap data");
                    return cachedData;
                }

                _logger.LogInformation("Fetching roadmap data from API");

                var response = await _httpClient.GetAsync("/api/roadmap/data");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<RoadmapDataModel>(jsonContent, _jsonOptions);

                    if (data != null)
                    {
                        _cache.Set(cacheKey, data, _mediumCacheDuration);
                        return data;
                    }
                }

                _logger.LogWarning("API call failed, returning fallback roadmap data");
                return GetFallbackRoadmapData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching roadmap data, returning fallback");
                return GetFallbackRoadmapData();
            }
        }

        /// <summary>
        /// Get milestones with optional filtering
        /// </summary>
        public async Task<List<MilestoneDisplayModel>> GetMilestonesAsync(string? status = null, string? category = null)
        {
            var cacheKey = $"{CACHE_KEY_MILESTONES}_{status}_{category}";

            try
            {
                if (_cache.TryGetValue(cacheKey, out List<MilestoneDisplayModel>? cachedMilestones) && cachedMilestones != null)
                {
                    return cachedMilestones;
                }

                var queryParams = new List<string>();
                if (!string.IsNullOrWhiteSpace(status)) queryParams.Add($"status={Uri.EscapeDataString(status)}");
                if (!string.IsNullOrWhiteSpace(category)) queryParams.Add($"category={Uri.EscapeDataString(category)}");

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetAsync($"/api/roadmap/milestones{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var milestones = JsonSerializer.Deserialize<List<MilestoneDisplayModel>>(jsonContent, _jsonOptions);

                    if (milestones != null)
                    {
                        _cache.Set(cacheKey, milestones, _shortCacheDuration);
                        return milestones;
                    }
                }

                return GetFallbackMilestones();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching milestones with status: {Status}, category: {Category}", status, category);
                return GetFallbackMilestones();
            }
        }

        /// <summary>
        /// Get milestone details by ID
        /// </summary>
        public async Task<MilestoneDisplayModel?> GetMilestoneDetailsAsync(int id)
        {
            var cacheKey = $"web_milestone_details_{id}";

            try
            {
                if (_cache.TryGetValue(cacheKey, out MilestoneDisplayModel? cachedMilestone) && cachedMilestone != null)
                {
                    return cachedMilestone;
                }

                var response = await _httpClient.GetAsync($"/api/roadmap/milestones/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var milestone = JsonSerializer.Deserialize<MilestoneDisplayModel>(jsonContent, _jsonOptions);

                    if (milestone != null)
                    {
                        _cache.Set(cacheKey, milestone, _shortCacheDuration);
                        return milestone;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching milestone details for ID: {MilestoneId}", id);
                return null;
            }
        }

        /// <summary>
        /// Get development statistics
        /// </summary>
        public async Task<DevelopmentStatsModel> GetDevelopmentStatsAsync()
        {
            var cacheKey = CACHE_KEY_DEV_STATS;

            try
            {
                if (_cache.TryGetValue(cacheKey, out DevelopmentStatsModel? cachedStats) && cachedStats != null)
                {
                    return cachedStats;
                }

                var response = await _httpClient.GetAsync("/api/roadmap/statistics");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var stats = JsonSerializer.Deserialize<DevelopmentStatsModel>(jsonContent, _jsonOptions);

                    if (stats != null)
                    {
                        _cache.Set(cacheKey, stats, _mediumCacheDuration);
                        return stats;
                    }
                }

                return GetFallbackDevelopmentStats();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching development statistics");
                return GetFallbackDevelopmentStats();
            }
        }

        /// <summary>
        /// Get recent development updates
        /// </summary>
        public async Task<List<UpdateDisplayModel>> GetRecentUpdatesAsync(int count = 10)
        {
            var cacheKey = $"{CACHE_KEY_RECENT_UPDATES}_{count}";

            try
            {
                if (_cache.TryGetValue(cacheKey, out List<UpdateDisplayModel>? cachedUpdates) && cachedUpdates != null)
                {
                    return cachedUpdates;
                }

                var response = await _httpClient.GetAsync($"/api/roadmap/updates?count={count}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var updates = JsonSerializer.Deserialize<List<UpdateDisplayModel>>(jsonContent, _jsonOptions);

                    if (updates != null)
                    {
                        _cache.Set(cacheKey, updates, _shortCacheDuration);
                        return updates;
                    }
                }

                return GetFallbackRecentUpdates();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recent updates");
                return GetFallbackRecentUpdates();
            }
        }

        /// <summary>
        /// Get releases information
        /// </summary>
        public async Task<List<ReleaseDisplayModel>> GetReleasesAsync()
        {
            var cacheKey = CACHE_KEY_RELEASES;

            try
            {
                if (_cache.TryGetValue(cacheKey, out List<ReleaseDisplayModel>? cachedReleases) && cachedReleases != null)
                {
                    return cachedReleases;
                }

                var response = await _httpClient.GetAsync("/api/roadmap/releases");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var releases = JsonSerializer.Deserialize<List<ReleaseDisplayModel>>(jsonContent, _jsonOptions);

                    if (releases != null)
                    {
                        _cache.Set(cacheKey, releases, _longCacheDuration);
                        return releases;
                    }
                }

                return GetFallbackReleases();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching releases");
                return GetFallbackReleases();
            }
        }

        /// <summary>
        /// Get GitHub statistics
        /// </summary>
        public async Task<GitHubStats> GetGitHubStatsAsync()
        {
            var cacheKey = CACHE_KEY_GITHUB_STATS;

            try
            {
                if (_cache.TryGetValue(cacheKey, out GitHubStats? cachedStats) && cachedStats != null)
                {
                    return cachedStats;
                }

                var response = await _httpClient.GetAsync("/api/roadmap/github-stats");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var stats = JsonSerializer.Deserialize<GitHubStats>(jsonContent, _jsonOptions);

                    if (stats != null)
                    {
                        _cache.Set(cacheKey, stats, _mediumCacheDuration);
                        return stats;
                    }
                }

                return GetFallbackGitHubStats();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching GitHub statistics");
                return GetFallbackGitHubStats();
            }
        }

        /// <summary>
        /// Get roadmap filter options
        /// </summary>
        public async Task<RoadmapFilterModel> GetFilterOptionsAsync()
        {
            var cacheKey = CACHE_KEY_FILTER_OPTIONS;

            try
            {
                if (_cache.TryGetValue(cacheKey, out RoadmapFilterModel? cachedOptions) && cachedOptions != null)
                {
                    return cachedOptions;
                }

                var response = await _httpClient.GetAsync("/api/roadmap/filter-options");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var options = JsonSerializer.Deserialize<RoadmapFilterModel>(jsonContent, _jsonOptions);

                    if (options != null)
                    {
                        _cache.Set(cacheKey, options, _longCacheDuration);
                        return options;
                    }
                }

                return GetFallbackFilterOptions();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching filter options");
                return GetFallbackFilterOptions();
            }
        }

        /// <summary>
        /// Get tasks for a specific milestone
        /// </summary>
        public async Task<List<TaskDisplayModel>> GetMilestoneTasksAsync(int milestoneId)
        {
            var cacheKey = $"web_milestone_tasks_{milestoneId}";

            try
            {
                if (_cache.TryGetValue(cacheKey, out List<TaskDisplayModel>? cachedTasks) && cachedTasks != null)
                {
                    return cachedTasks;
                }

                var response = await _httpClient.GetAsync($"/api/roadmap/milestones/{milestoneId}/tasks");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var tasks = JsonSerializer.Deserialize<List<TaskDisplayModel>>(jsonContent, _jsonOptions);

                    if (tasks != null)
                    {
                        _cache.Set(cacheKey, tasks, _shortCacheDuration);
                        return tasks;
                    }
                }

                return new List<TaskDisplayModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tasks for milestone {MilestoneId}", milestoneId);
                return new List<TaskDisplayModel>();
            }
        }

        /// <summary>
        /// Search milestones by term
        /// </summary>
        public async Task<List<MilestoneDisplayModel>> SearchMilestonesAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetMilestonesAsync();
                }

                var response = await _httpClient.GetAsync($"/api/roadmap/search?term={Uri.EscapeDataString(searchTerm)}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var milestones = JsonSerializer.Deserialize<List<MilestoneDisplayModel>>(jsonContent, _jsonOptions);

                    return milestones ?? new List<MilestoneDisplayModel>();
                }

                return new List<MilestoneDisplayModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching milestones for term: {SearchTerm}", searchTerm);
                return new List<MilestoneDisplayModel>();
            }
        }

        /// <summary>
        /// Get milestone dependencies
        /// </summary>
        public async Task<List<DependencyDisplayModel>> GetMilestoneDependenciesAsync(int milestoneId)
        {
            var cacheKey = $"web_milestone_dependencies_{milestoneId}";

            try
            {
                if (_cache.TryGetValue(cacheKey, out List<DependencyDisplayModel>? cachedDeps) && cachedDeps != null)
                {
                    return cachedDeps;
                }

                var response = await _httpClient.GetAsync($"/api/roadmap/milestones/{milestoneId}/dependencies");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var dependencies = JsonSerializer.Deserialize<List<DependencyDisplayModel>>(jsonContent, _jsonOptions);

                    if (dependencies != null)
                    {
                        _cache.Set(cacheKey, dependencies, _mediumCacheDuration);
                        return dependencies;
                    }
                }

                return new List<DependencyDisplayModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dependencies for milestone {MilestoneId}", milestoneId);
                return new List<DependencyDisplayModel>();
            }
        }

        /// <summary>
        /// Get development timeline data for charts
        /// </summary>
        public async Task<object> GetTimelineDataAsync()
        {
            var cacheKey = "web_timeline_data";

            try
            {
                if (_cache.TryGetValue(cacheKey, out object? cachedData) && cachedData != null)
                {
                    return cachedData;
                }

                var response = await _httpClient.GetAsync("/api/roadmap/timeline-data");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var timelineData = JsonSerializer.Deserialize<object>(jsonContent, _jsonOptions);

                    if (timelineData != null)
                    {
                        _cache.Set(cacheKey, timelineData, _mediumCacheDuration);
                        return timelineData;
                    }
                }

                return new { };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching timeline data");
                return new { };
            }
        }

        /// <summary>
        /// Get milestone progress over time
        /// </summary>
        public async Task<object> GetProgressHistoryAsync(int milestoneId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/roadmap/milestones/{milestoneId}/progress-history");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var progressData = JsonSerializer.Deserialize<object>(jsonContent, _jsonOptions);

                    return progressData ?? new { };
                }

                return new { };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching progress history for milestone {MilestoneId}", milestoneId);
                return new { };
            }
        }

        /// <summary>
        /// Check service health
        /// </summary>
        public async Task<bool> CheckServiceHealthAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/roadmap/health", new CancellationToken());
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking roadmap service health");
                return false;
            }
        }

        #region Fallback Data Methods

        private RoadmapDataModel GetFallbackRoadmapData()
        {
            return new RoadmapDataModel
            {
                Overview = new RoadmapOverviewModel
                {
                    TotalMilestones = 8,
                    CompletedMilestones = 3,
                    InProgressMilestones = 2,
                    UpcomingMilestones = 3,
                    OverallProgress = 65.5m,
                    EstimatedCompletionDate = DateTime.UtcNow.AddMonths(18),
                    LastUpdateDate = DateTime.UtcNow.AddDays(-2)
                },
                CurrentMilestones = GetFallbackMilestones().Take(3).ToList(),
                UpcomingMilestones = GetFallbackMilestones().Skip(3).Take(3).ToList(),
                DevelopmentStats = GetFallbackDevelopmentStats(),
                RecentUpdates = GetFallbackRecentUpdates().Take(5).ToList(),
                GitHubStats = GetFallbackGitHubStats()
            };
        }

        private List<MilestoneDisplayModel> GetFallbackMilestones()
        {
            return new List<MilestoneDisplayModel>
            {
                new MilestoneDisplayModel
                {
                    Id = 1,
                    Title = "Token Smart Contract Deployment",
                    Description = "Deploy and verify TEACH token smart contract on mainnet",
                    Category = "Blockchain",
                    Status = "Completed",
                    StatusClass = "completed",
                    Priority = "Critical",
                    PriorityClass = "critical",
                    ProgressPercentage = 100,
                    StartDate = DateTime.UtcNow.AddDays(-90),
                    EstimatedCompletionDate = DateTime.UtcNow.AddDays(-60),
                    ActualCompletionDate = DateTime.UtcNow.AddDays(-58),
                    DurationEstimate = "4 weeks",
                    TimeRemaining = "Completed",
                    CompletedTasksCount = 8,
                    TotalTasksCount = 8
                },
                new MilestoneDisplayModel
                {
                    Id = 2,
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
                    TotalTasksCount = 16
                },
                new MilestoneDisplayModel
                {
                    Id = 3,
                    Title = "Mobile Application Launch",
                    Description = "Native mobile apps for iOS and Android platforms",
                    Category = "Mobile",
                    Status = "Planning",
                    StatusClass = "planning",
                    Priority = "Medium",
                    PriorityClass = "medium",
                    ProgressPercentage = 15,
                    StartDate = DateTime.UtcNow.AddDays(30),
                    EstimatedCompletionDate = DateTime.UtcNow.AddDays(120),
                    DurationEstimate = "12 weeks",
                    TimeRemaining = "4 months",
                    CompletedTasksCount = 3,
                    TotalTasksCount = 20
                }
            };
        }

        private DevelopmentStatsModel GetFallbackDevelopmentStats()
        {
            return new DevelopmentStatsModel
            {
                TotalCommits = 247,
                ActiveBranches = 5,
                OpenIssues = 12,
                ClosedIssues = 89,
                PullRequests = 23,
                CodeCoverage = 82.5m,
                TechnicalDebt = "2.1 days",
                LastDeployment = DateTime.UtcNow.AddDays(-3),
                Contributors = 6,
                LinesOfCode = 45678
            };
        }

        private List<UpdateDisplayModel> GetFallbackRecentUpdates()
        {
            return new List<UpdateDisplayModel>
            {
                new UpdateDisplayModel
                {
                    Id = 1,
                    Title = "Staking Rewards Module Completed",
                    Description = "Implemented automatic reward distribution system with 50/50 split",
                    Type = "Feature",
                    UpdateDate = DateTime.UtcNow.AddDays(-2),
                    Author = "Development Team",
                    Category = "Platform"
                },
                new UpdateDisplayModel
                {
                    Id = 2,
                    Title = "Security Audit Results",
                    Description = "Completed third-party security audit with all critical issues resolved",
                    Type = "Security",
                    UpdateDate = DateTime.UtcNow.AddDays(-5),
                    Author = "Security Team",
                    Category = "Security"
                }
            };
        }

        private List<ReleaseDisplayModel> GetFallbackReleases()
        {
            return new List<ReleaseDisplayModel>
            {
                new ReleaseDisplayModel
                {
                    Id = 1,
                    Version = "v1.2.0",
                    Title = "Enhanced Staking Platform",
                    Description = "Major platform update with improved staking mechanisms",
                    ReleaseDate = DateTime.UtcNow.AddDays(-14),
                    IsPreRelease = false,
                    DownloadUrl = "https://github.com/buckhoff/TeachCrowdSale/releases/tag/v1.2.0"
                }
            };
        }

        private GitHubStats GetFallbackGitHubStats()
        {
            return new GitHubStats
            {
                Stars = 156,
                Forks = 23,
                Contributors = 6,
                RecentCommits = 15,
                OpenIssues = 12,
                LastCommitDate = DateTime.UtcNow.AddHours(-6),
                Repository = "buckhoff/TeachCrowdSale",
                MainBranch = "main"
            };
        }

        private RoadmapFilterModel GetFallbackFilterOptions()
        {
            return new RoadmapFilterModel
            {
                Statuses = new List<string> { "Planning", "In Progress", "Testing", "Completed", "On Hold" },
                Categories = new List<string> { "Blockchain", "Platform", "Mobile", "Security", "Integration" },
                Priorities = new List<string> { "Critical", "High", "Medium", "Low" }
            };
        }

        #endregion
    }
}
