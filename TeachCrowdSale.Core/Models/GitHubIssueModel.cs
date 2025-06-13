// TeachCrowdSale.Core/Interfaces/IRoadmapService.cs
// GitHub-specific models
public class GitHubIssueModel
    {
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string StateClass { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string TimeAgo { get; set; } = string.Empty;
    public string IssueUrl { get; set; } = string.Empty;
    public List<string> Labels { get; set; } = new();
    public bool IsPullRequest { get; set; }
    public string TypeIcon { get; set; } = string.Empty;
}
