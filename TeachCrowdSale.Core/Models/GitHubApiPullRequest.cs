namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// GitHub API response model for pull requests
    /// </summary>
    public class GitHubApiPullRequest
    {
        public int Number { get; set; }
        public string Title { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public GitHubApiUser User { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? MergedAt { get; set; }
        public List<GitHubApiLabel>? Labels { get; set; }
        public string HtmlUrl { get; set; } = string.Empty;
        public bool Draft { get; set; }
        public int Additions { get; set; }
        public int Deletions { get; set; }
        public string? Body { get; set; }
        public DateTime UpdatedAt { get; set; }
        public GitHubApiUser? Assignee { get; set; }
        public List<GitHubApiUser>? Assignees { get; set; }
        public GitHubApiMilestone? Milestone { get; set; }
        public bool Locked { get; set; }
        public string? ActiveLockReason { get; set; }
        public int Comments { get; set; }
        public int ReviewComments { get; set; }
        public int Commits { get; set; }
        public int ChangedFiles { get; set; }
    }
}
