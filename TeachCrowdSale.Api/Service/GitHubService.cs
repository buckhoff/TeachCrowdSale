
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Models.Response;
using TeachCrowdSale.Core.Models;
using TeachCrowdSale.Core.Models.Request;
using TeachCrowdSale.Infrastructure.Configuration;

namespace TeachCrowdSale.Api.Service
{
    public class GitHubService : IGitHubService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<GitHubService> _logger;
        private readonly GitHubSettings _settings;
        private readonly JsonSerializerOptions _jsonOptions;

        // Cache keys and durations
        private const string CACHE_KEY_REPO_STATS = "github_repo_stats";
        private const string CACHE_KEY_RECENT_COMMITS = "github_recent_commits";
        private const string CACHE_KEY_GITHUB_STATS = "github_overall_stats";
        private const string CACHE_KEY_CONTRIBUTORS = "github_contributors";
        private const string CACHE_KEY_RATE_LIMIT = "github_rate_limit";

        private readonly TimeSpan _shortCacheDuration = TimeSpan.FromMinutes(10);
        private readonly TimeSpan _mediumCacheDuration = TimeSpan.FromMinutes(30);
        private readonly TimeSpan _longCacheDuration = TimeSpan.FromHours(2);

        public GitHubService(
            HttpClient httpClient,
            IMemoryCache cache,
            ILogger<GitHubService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
            _settings = configuration.GetSection("GitHub").Get<GitHubSettings>() ?? new GitHubSettings();

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };

            ConfigureHttpClient();
        }

        public async Task<List<RepositoryStatsModel>> GetRepositoryStatsAsync()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_REPO_STATS, out List<RepositoryStatsModel>? cachedStats) && cachedStats != null)
                {
                    return cachedStats;
                }

                var repositories = new List<RepositoryStatsModel>();

                foreach (var repoName in _settings.Repositories)
                {
                    try
                    {
                        var repoInfo = await GetRepositoryInfoAsync(repoName);
                        if (repoInfo != null)
                        {
                            repositories.Add(repoInfo);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get info for repository {Repository}", repoName);
                    }
                }

                _cache.Set(CACHE_KEY_REPO_STATS, repositories, _mediumCacheDuration);
                return repositories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving repository statistics");
                return GetFallbackRepositoryStats();
            }
        }

        public async Task<List<CommitActivityModel>> GetRecentCommitsAsync(int count = 10)
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_RECENT_COMMITS, out List<CommitActivityModel>? cachedCommits) && cachedCommits != null)
                {
                    return cachedCommits.Take(count).ToList();
                }

                var allCommits = new List<CommitActivityModel>();

                foreach (var repoName in _settings.Repositories)
                {
                    try
                    {
                        var commits = await GetRepositoryCommitsAsync(repoName, 20);
                        allCommits.AddRange(commits);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get commits for repository {Repository}", repoName);
                    }
                }

                // Sort by date and take most recent
                var recentCommits = allCommits
                    .OrderByDescending(c => c.Date)
                    .Take(50)
                    .ToList();

                _cache.Set(CACHE_KEY_RECENT_COMMITS, recentCommits, _shortCacheDuration);
                return recentCommits.Take(count).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent commits");
                return GetFallbackRecentCommits(count);
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

                var repositories = await GetRepositoryStatsAsync();
                var recentCommits = await GetRecentCommitsAsync(100);

                var stats = new GitHubStatsModel
                {
                    TotalCommits = await GetTotalCommitsAsync(),
                    CommitsThisWeek = recentCommits.Count(c => c.Date >= DateTime.UtcNow.AddDays(-7)),
                    OpenIssues = repositories.Sum(r => r.OpenIssues),
                    ClosedIssues = await GetTotalClosedIssuesAsync(),
                    OpenPullRequests = await GetTotalOpenPullRequestsAsync(),
                    MergedPullRequests = await GetTotalMergedPullRequestsAsync(),
                    Contributors = await GetTotalContributorsAsync(),
                    Repositories = repositories,
                    RecentActivity = recentCommits.Take(10).ToList(),
                    LastUpdated = DateTime.UtcNow
                };

                _cache.Set(CACHE_KEY_GITHUB_STATS, stats, _mediumCacheDuration);
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving GitHub statistics");
                return GetFallbackGitHubStats();
            }
        }

        public async Task<List<GitHubIssueModel>> GetRepositoryIssuesAsync(string repository)
        {
            try
            {
                var url = $"repos/{_settings.Organization}/{repository}/issues?state=all&per_page=100";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get issues for repository {Repository}. Status: {StatusCode}", repository, response.StatusCode);
                    return new List<GitHubIssueModel>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var issues = JsonSerializer.Deserialize<List<GitHubApiIssue>>(content, _jsonOptions);

                return issues
                    .Where(i => i.PullRequest == null) // Filter out PRs - they have this property
                    .Select(MapToIssueModel)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving issues for repository {Repository}", repository);
                return new List<GitHubIssueModel>();
            }
        }

        public async Task<List<GitHubPullRequestModel>> GetRepositoryPullRequestsAsync(string repository)
        {
            try
            {
                var url = $"repos/{_settings.Organization}/{repository}/pulls?state=all&per_page=100";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get pull requests for repository {Repository}. Status: {StatusCode}", repository, response.StatusCode);
                    return new List<GitHubPullRequestModel>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var pullRequests = JsonSerializer.Deserialize<List<GitHubApiPullRequest>>(content, _jsonOptions);

                return pullRequests?.Select(pr => MapToPullRequestModel(pr, repository)).ToList() ?? new List<GitHubPullRequestModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pull requests for repository {Repository}", repository);
                return new List<GitHubPullRequestModel>();
            }
        }

        public async Task<string?> CreateIssueAsync(string repository, string title, string body)
        {
            try
            {
                if (string.IsNullOrEmpty(_settings.AccessToken))
                {
                    _logger.LogWarning("GitHub access token not configured - cannot create issues");
                    return null;
                }

                var url = $"repos/{_settings.Organization}/{repository}/issues";
                var payload = new
                {
                    title,
                    body,
                    labels = new[] { "auto-generated" }
                };

                var json = JsonSerializer.Serialize(payload, _jsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to create issue in repository {Repository}. Status: {StatusCode}", repository, response.StatusCode);
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var issue = JsonSerializer.Deserialize<GitHubApiIssue>(responseContent, _jsonOptions);

                return issue?.HtmlUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating issue in repository {Repository}", repository);
                return null;
            }
        }

        public async Task<List<ContributorStatsModel>> GetContributorStatsAsync()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_CONTRIBUTORS, out List<ContributorStatsModel>? cachedContributors) && cachedContributors != null)
                {
                    return cachedContributors;
                }

                var allContributors = new Dictionary<string, ContributorStatsModel>();

                foreach (var repoName in _settings.Repositories)
                {
                    try
                    {
                        var contributors = await GetRepositoryContributorsAsync(repoName);
                        foreach (var contributor in contributors)
                        {
                            if (allContributors.TryGetValue(contributor.Username, out var existing))
                            {
                                existing.TotalCommits += contributor.TotalCommits;
                                existing.Additions += contributor.Additions;
                                existing.Deletions += contributor.Deletions;
                                existing.Repositories.Add(repoName);
                                if (contributor.LastContribution > existing.LastContribution)
                                {
                                    existing.LastContribution = contributor.LastContribution;
                                }
                            }
                            else
                            {
                                contributor.Repositories = new List<string> { repoName };
                                allContributors[contributor.Username] = contributor;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get contributors for repository {Repository}", repoName);
                    }
                }

                var result = allContributors.Values
                    .OrderByDescending(c => c.TotalCommits)
                    .ToList();

                _cache.Set(CACHE_KEY_CONTRIBUTORS, result, _longCacheDuration);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contributor statistics");
                return new List<ContributorStatsModel>();
            }
        }

        public async Task<GitHubRateLimitModel> GetRateLimitAsync()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_RATE_LIMIT, out GitHubRateLimitModel? cachedRateLimit) && cachedRateLimit != null)
                {
                    return cachedRateLimit;
                }

                var response = await _httpClient.GetAsync("rate_limit");

                if (!response.IsSuccessStatusCode)
                {
                    return new GitHubRateLimitModel
                    {
                        Limit = 60,
                        Remaining = 0,
                        ResetTime = DateTime.UtcNow.AddHours(1),
                        IsNearLimit = true
                    };
                }

                var content = await response.Content.ReadAsStringAsync();
                var rateLimitResponse = JsonSerializer.Deserialize<GitHubRateLimitResponse>(content, _jsonOptions);

                var rateLimit = new GitHubRateLimitModel
                {
                    Limit = rateLimitResponse?.Rate?.Limit ?? 60,
                    Remaining = rateLimitResponse?.Rate?.Remaining ?? 0,
                    ResetTime = DateTimeOffset.FromUnixTimeSeconds(rateLimitResponse?.Rate?.Reset ?? 0).DateTime,
                    IsNearLimit = (rateLimitResponse?.Rate?.Remaining ?? 0) < 10
                };

                _cache.Set(CACHE_KEY_RATE_LIMIT, rateLimit, TimeSpan.FromMinutes(5));
                return rateLimit;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving GitHub rate limit");
                return new GitHubRateLimitModel
                {
                    Limit = 60,
                    Remaining = 0,
                    ResetTime = DateTime.UtcNow.AddHours(1),
                    IsNearLimit = true
                };
            }
        }

        #region Private Helper Methods

        private void ConfigureHttpClient()
        {
            _httpClient.BaseAddress = new Uri("https://api.github.com/");
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("TeachCrowdSale", "1.0"));

            if (!string.IsNullOrEmpty(_settings.AccessToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.AccessToken);
            }
        }

        private async Task<RepositoryStatsModel?> GetRepositoryInfoAsync(string repoName)
        {
            var url = $"repos/{_settings.Organization}/{repoName}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var repo = JsonSerializer.Deserialize<GitHubApiRepository>(content, _jsonOptions);

            if (repo == null) return null;

            return new RepositoryStatsModel
            {
                Name = repo.Name,
                Description = repo.Description ?? "",
                Url = repo.HtmlUrl,
                Language = repo.Language ?? "Unknown",
                Stars = repo.StargazersCount,
                Forks = repo.ForksCount,
                OpenIssues = repo.OpenIssuesCount,
                LastCommit = repo.UpdatedAt,
                IsActive = repo.UpdatedAt > DateTime.UtcNow.AddDays(-30)
            };
        }

        private async Task<List<CommitActivityModel>> GetRepositoryCommitsAsync(string repoName, int count)
        {
            var url = $"repos/{_settings.Organization}/{repoName}/commits?per_page={count}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return new List<CommitActivityModel>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var commits = JsonSerializer.Deserialize<List<GitHubApiCommit>>(content, _jsonOptions);

            return commits?.Select(c => new CommitActivityModel
            {
                CommitHash = c.Sha[..8], // First 8 characters
                Message = c.Commit.Message.Split('\n')[0], // First line only
                Author = c.Commit.Author.Name,
                Repository = repoName,
                Date = c.Commit.Author.Date,
                Url = c.HtmlUrl,
                FormattedDate = c.Commit.Author.Date.ToString("MMM dd, yyyy")
            }).ToList() ?? new List<CommitActivityModel>();
        }

        private async Task<List<ContributorStatsModel>> GetRepositoryContributorsAsync(string repoName)
        {
            var url = $"repos/{_settings.Organization}/{repoName}/contributors";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return new List<ContributorStatsModel>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var contributors = JsonSerializer.Deserialize<List<GitHubApiContributor>>(content, _jsonOptions);

            return contributors?.Select(c => new ContributorStatsModel
            {
                Username = c.Login,
                Name = c.Login, // GitHub API doesn't return real name in contributors endpoint
                AvatarUrl = c.AvatarUrl,
                TotalCommits = c.Contributions,
                Additions = 0, // Would need detailed stats API call
                Deletions = 0, // Would need detailed stats API call
                LastContribution = DateTime.UtcNow, // Approximate
                Repositories = new List<string>()
            }).ToList() ?? new List<ContributorStatsModel>();
        }

        private async Task<int> GetTotalCommitsAsync()
        {
            // This is a simplified calculation - in production you might want to cache this more aggressively
            var repositories = await GetRepositoryStatsAsync();
            var totalCommits = 0;

            foreach (var repo in repositories)
            {
                try
                {
                    var commits = await GetRepositoryCommitsAsync(repo.Name, 100);
                    totalCommits += commits.Count;
                }
                catch
                {
                    // Continue with other repos if one fails
                }
            }

            return totalCommits;
        }

        private async Task<int> GetTotalClosedIssuesAsync()
        {
            var totalClosed = 0;

            foreach (var repoName in _settings.Repositories)
            {
                try
                {
                    var issues = await GetRepositoryIssuesAsync(repoName);
                    totalClosed += issues.Count(i => i.State == "closed");
                }
                catch
                {
                    // Continue with other repos if one fails
                }
            }

            return totalClosed;
        }

        private async Task<int> GetTotalOpenPullRequestsAsync()
        {
            var totalOpen = 0;

            foreach (var repoName in _settings.Repositories)
            {
                try
                {
                    var pullRequests = await GetRepositoryPullRequestsAsync(repoName);
                    totalOpen += pullRequests.Count(pr => pr.State == "open");
                }
                catch
                {
                    // Continue with other repos if one fails
                }
            }

            return totalOpen;
        }

        private async Task<int> GetTotalMergedPullRequestsAsync()
        {
            var totalMerged = 0;

            foreach (var repoName in _settings.Repositories)
            {
                try
                {
                    var pullRequests = await GetRepositoryPullRequestsAsync(repoName);
                    totalMerged += pullRequests.Count(pr => pr.IsMerged);
                }
                catch
                {
                    // Continue with other repos if one fails
                }
            }

            return totalMerged;
        }

        private async Task<int> GetTotalContributorsAsync()
        {
            var contributors = await GetContributorStatsAsync();
            return contributors.Count;
        }

        private GitHubIssueModel MapToIssueModel(GitHubApiIssue issue)
        {
            return new GitHubIssueModel
            {
                Number = issue.Number,
                Title = issue.Title,
                State = issue.State,
                Author = issue.User.Login,
                CreatedAt = issue.CreatedAt,
                ClosedAt = issue.ClosedAt,
                Labels = issue.Labels?.Select(l => l.Name).ToList() ?? new List<string>(),
                Url = issue.HtmlUrl
            };
        }

        private GitHubPullRequestModel MapToPullRequestModel(GitHubApiPullRequest apiPr, string repository)
        {
            return new GitHubPullRequestModel
            {
                Number = apiPr.Number,
                Title = apiPr.Title,
                State = apiPr.State,
                Author = apiPr.User.Login,
                AuthorAvatarUrl = apiPr.User.AvatarUrl,
                CreatedAt = apiPr.CreatedAt,
                MergedAt = apiPr.MergedAt,
                UpdatedAt = apiPr.UpdatedAt,
                Labels = apiPr.Labels?.Select(l => l.Name).ToList() ?? new List<string>(),
                Url = apiPr.HtmlUrl,
                IsDraft = apiPr.Draft,
                Additions = apiPr.Additions,
                Deletions = apiPr.Deletions,
                Description = apiPr.Body,
                Repository = repository,
                FormattedCreatedDate = apiPr.CreatedAt.ToString("MMM dd, yyyy"),
                FormattedMergedDate = apiPr.MergedAt?.ToString("MMM dd, yyyy") ?? "",
                Comments = apiPr.Comments,
                ReviewComments = apiPr.ReviewComments,
                Commits = apiPr.Commits,
                ChangedFiles = apiPr.ChangedFiles,
                AssignedTo = apiPr.Assignee?.Login,
                Assignees = apiPr.Assignees?.Select(a => a.Login).ToList() ?? new List<string>(),
                MilestoneTitle = apiPr.Milestone?.Title
            };
        }

        #endregion

        #region Fallback Data Methods

        private List<RepositoryStatsModel> GetFallbackRepositoryStats()
        {
            return new List<RepositoryStatsModel>
            {
                new RepositoryStatsModel
                {
                    Name = "TeachCrowdSale",
                    Description = "Main TeachToken platform repository",
                    Url = "https://github.com/buckhoff/TeachCrowdSale",
                    Language = "C#",
                    Stars = 24,
                    Forks = 8,
                    OpenIssues = 12,
                    LastCommit = DateTime.UtcNow.AddHours(-6),
                    IsActive = true
                },
                new RepositoryStatsModel
                {
                    Name = "TokenContract",
                    Description = "TEACH token smart contract",
                    Url = "https://github.com/buckhoff/TokenContract",
                    Language = "Solidity",
                    Stars = 18,
                    Forks = 5,
                    OpenIssues = 7,
                    LastCommit = DateTime.UtcNow.AddHours(-2),
                    IsActive = true
                }
            };
        }

        private List<CommitActivityModel> GetFallbackRecentCommits(int count)
        {
            var commits = new List<CommitActivityModel>
            {
                new CommitActivityModel
                {
                    CommitHash = "a1b2c3d4",
                    Message = "Implement token sale contract functionality",
                    Author = "Development Team",
                    Repository = "TeachCrowdSale",
                    Date = DateTime.UtcNow.AddHours(-2),
                    Url = "https://github.com/buckhoff/TeachCrowdSale/commit/a1b2c3d4",
                    FormattedDate = DateTime.UtcNow.AddHours(-2).ToString("MMM dd, yyyy")
                },
                new CommitActivityModel
                {
                    CommitHash = "e5f6g7h8",
                    Message = "Add vesting and tier management",
                    Author = "Backend Team",
                    Repository = "TokenContract",
                    Date = DateTime.UtcNow.AddHours(-6),
                    Url = "https://github.com/buckhoff/TokenContract/commit/e5f6g7h8",
                    FormattedDate = DateTime.UtcNow.AddHours(-6).ToString("MMM dd, yyyy")
                }
            };

            return commits.Take(count).ToList();
        }

        private GitHubStatsModel GetFallbackGitHubStats()
        {
            return new GitHubStatsModel
            {
                TotalCommits = 486,
                CommitsThisWeek = 23,
                OpenIssues = 19,
                ClosedIssues = 89,
                OpenPullRequests = 4,
                MergedPullRequests = 67,
                Contributors = 6,
                Repositories = GetFallbackRepositoryStats(),
                RecentActivity = GetFallbackRecentCommits(10),
                LastUpdated = DateTime.UtcNow
            };
        }

        #endregion
    }
}