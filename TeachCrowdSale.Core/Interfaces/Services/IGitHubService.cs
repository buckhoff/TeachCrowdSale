// TeachCrowdSale.Core/Interfaces/IRoadmapService.cs
using TeachCrowdSale.Core.Models;

namespace TeachCrowdSale.Core.Interfaces
{
    /// <summary>
    /// Service interface for GitHub integration
    /// </summary>
    public interface IGitHubService
    {
        /// <summary>
        /// Get repository statistics
        /// </summary>
        Task<List<RepositoryStatsModel>> GetRepositoryStatsAsync();

        /// <summary>
        /// Get recent commit activity
        /// </summary>
        Task<List<CommitActivityModel>> GetRecentCommitsAsync(int count = 10);

        /// <summary>
        /// Get overall GitHub statistics
        /// </summary>
        Task<GitHubStatsModel> GetGitHubStatsAsync();

        /// <summary>
        /// Get issues for a repository
        /// </summary>
        Task<List<GitHubIssueModel>> GetRepositoryIssuesAsync(string repository);

        /// <summary>
        /// Get pull requests for a repository
        /// </summary>
        Task<List<GitHubPullRequestModel>> GetRepositoryPullRequestsAsync(string repository);

        /// <summary>
        /// Create an issue (for internal tracking)
        /// </summary>
        Task<string?> CreateIssueAsync(string repository, string title, string body);

        /// <summary>
        /// Get contributor statistics
        /// </summary>
        Task<List<ContributorStatsModel>> GetContributorStatsAsync();

        /// <summary>
        /// Check GitHub API rate limits
        /// </summary>
        Task<GitHubRateLimitModel> GetRateLimitAsync();
    }
}