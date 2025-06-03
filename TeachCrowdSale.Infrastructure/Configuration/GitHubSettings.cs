// TeachCrowdSale.Infrastructure/Services/GitHubService.cs
// TeachCrowdSale.Infrastructure/Configuration/GitHubSettings.cs
namespace TeachCrowdSale.Infrastructure.Configuration
{
    public class GitHubSettings
    {
        public string Organization { get; set; } = "teachtoken";
        public string? AccessToken { get; set; }
        public List<string> Repositories { get; set; } = new()
        {
            "TeachCrowdSale",
            "TeacherSupport-Platform",
            "TeachToken"
        };
        public int RateLimitThreshold { get; set; } = 10;
        public bool EnableRateLimitChecking { get; set; } = true;
    }
}