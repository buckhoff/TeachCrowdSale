namespace TeachCrowdSale.Core.Models;

using System;
using System.Collections.Generic;
/// <summary>
/// Represents a GitHub pull request model for API responses.
/// </summary>
public class GitHubPullRequestModel
{
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? MergedAt { get; set; }
    public List<string> Labels { get; set; } = new();
    public string Url { get; set; } = string.Empty;
    public bool IsDraft { get; set; }
    public int Additions { get; set; }
    public int Deletions { get; set; }
}