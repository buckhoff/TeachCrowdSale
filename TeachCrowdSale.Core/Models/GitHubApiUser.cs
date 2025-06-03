namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// GitHub API user model
    /// </summary>
    public class GitHubApiUser
    {
        public string Login { get; set; } = string.Empty;
        public int Id { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string HtmlUrl { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool SiteAdmin { get; set; }
    }
}
