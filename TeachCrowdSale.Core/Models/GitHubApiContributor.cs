namespace TeachCrowdSale.Core.Models
{
    public class GitHubApiContributor
    {
        public string Login { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public int Contributions { get; set; }
    }
}
