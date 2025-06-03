namespace TeachCrowdSale.Core.Models
{
    public class GitHubApiCommit
    {
        public string Sha { get; set; } = string.Empty;
        public string HtmlUrl { get; set; } = string.Empty;
        public GitHubApiCommitDetails Commit { get; set; } = new();
    }
}
