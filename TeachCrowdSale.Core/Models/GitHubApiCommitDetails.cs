namespace TeachCrowdSale.Core.Models
{
    public class GitHubApiCommitDetails
    {
        public string Message { get; set; } = string.Empty;
        public GitHubApiCommitAuthor Author { get; set; } = new();
    }

    #endregion
}
