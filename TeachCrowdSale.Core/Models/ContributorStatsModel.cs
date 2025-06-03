// TeachCrowdSale.Core/Interfaces/IRoadmapService.cs
public class ContributorStatsModel
    {
        public string Username { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public int TotalCommits { get; set; }
        public int Additions { get; set; }
        public int Deletions { get; set; }
        public DateTime LastContribution { get; set; }
        public List<string> Repositories { get; set; } = new();
    }
