namespace TeachCrowdSale.Core.Models
{
    public class GitHubRateLimit
    {
        public int Limit { get; set; }
        public int Remaining { get; set; }
        public long Reset { get; set; }
    }
}
