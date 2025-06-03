// TeachCrowdSale.Core/Interfaces/IRoadmapService.cs
// GitHub-specific models
public class GitHubIssueModel
    {
        public int Number { get; set; }
        public string Title { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public List<string> Labels { get; set; } = new();
        public string Url { get; set; } = string.Empty;
    }
