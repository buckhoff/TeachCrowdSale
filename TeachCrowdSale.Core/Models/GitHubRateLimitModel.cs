// TeachCrowdSale.Core/Interfaces/IRoadmapService.cs
public class GitHubRateLimitModel
    {
        public int Limit { get; set; }
        public int Remaining { get; set; }
        public DateTime ResetTime { get; set; }
        public bool IsNearLimit { get; set; }
    }
}