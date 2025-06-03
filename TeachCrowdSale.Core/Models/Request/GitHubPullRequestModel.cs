namespace TeachCrowdSale.Core.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Display model for GitHub pull requests - used for UI rendering
/// </summary>
public class GitHubPullRequestModel
{
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string AuthorAvatarUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? MergedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<string> Labels { get; set; } = new();
    public string Url { get; set; } = string.Empty;
    public bool IsDraft { get; set; }
    public int Additions { get; set; }
    public int Deletions { get; set; }
    public string? Description { get; set; }
    public string Repository { get; set; } = string.Empty;
    public string FormattedCreatedDate { get; set; } = string.Empty;
    public string FormattedMergedDate { get; set; } = string.Empty;
    public bool IsMerged => MergedAt.HasValue;
    public bool IsOpen => State.Equals("open", StringComparison.OrdinalIgnoreCase);
    public bool IsClosed => State.Equals("closed", StringComparison.OrdinalIgnoreCase);
    public int TotalChanges => Additions + Deletions;
    public string StateDisplay => IsMerged ? "Merged" : State;
    public string StateClass => IsMerged ? "merged" : State.ToLower();
    public int Comments { get; set; }
    public int ReviewComments { get; set; }
    public int Commits { get; set; }
    public int ChangedFiles { get; set; }
    public string? AssignedTo { get; set; }
    public List<string> Assignees { get; set; } = new();
    public string? MilestoneTitle { get; set; }
}