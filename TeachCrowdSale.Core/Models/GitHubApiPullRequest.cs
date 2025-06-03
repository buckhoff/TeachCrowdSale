namespace TeachCrowdSale.Core.Models
{
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
    }
}
