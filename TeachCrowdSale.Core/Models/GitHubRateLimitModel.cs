namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Represents the GitHub API rate limit model.
    /// </summary>
    /// <remarks>
    /// This model is used to track the rate limit status of GitHub API requests.
    /// </remarks>
    public class GitHubRateLimitModel
    {
        public int Limit { get; set; }
        public int Remaining { get; set; }
        public DateTime ResetTime { get; set; }
        public bool IsNearLimit { get; set; }
    }
}