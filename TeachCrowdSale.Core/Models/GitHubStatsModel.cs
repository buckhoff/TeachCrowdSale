// TeachCrowdSale.Core/Models/GitHubStatsModel.cs
using System.ComponentModel.DataAnnotations;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Consolidated GitHub statistics model for web display
    /// </summary>
    public class GitHubStatsModel
    {
        public int TotalCommits { get; set; }
        public int CommitsThisMonth { get; set; }
        public int CommitsThisWeek { get; set; }
        public int TotalContributors { get; set; }
        public int ActiveContributors { get; set; }
        public int TotalPullRequests { get; set; }
        public int OpenPullRequests { get; set; }
        public int MergedPullRequests { get; set; }
        public int TotalIssues { get; set; }
        public int OpenIssues { get; set; }
        public int ClosedIssues { get; set; }
        public int CodeFrequency { get; set; }
        public DateTime? LastCommitDate { get; set; }
        public string? LastCommitMessage { get; set; }
        public string? LastCommitAuthor { get; set; }
        public string? RepositoryUrl { get; set; }
        public string? DefaultBranch { get; set; }
        public int LinesOfCode { get; set; }
        public int StarCount { get; set; }
        public int ForkCount { get; set; }
        public int WatcherCount { get; set; }

        // Display helper properties
        public string LastCommitFormatted => LastCommitDate?.ToString("MMM dd 'at' h:mm tt") ?? "No recent commits";
        public string LastCommitMessageDisplay => !string.IsNullOrEmpty(LastCommitMessage) && LastCommitMessage.Length > 50
            ? LastCommitMessage[..50] + "..." : LastCommitMessage ?? "";
        public string LastCommitAuthorDisplay => !string.IsNullOrEmpty(LastCommitAuthor) ? LastCommitAuthor : "Unknown";
        public string ActivityLevel => CommitsThisWeek >= 10 ? "high" : CommitsThisWeek >= 5 ? "medium" : "low";
        public string CommitFrequencyText => $"{CommitsThisWeek} this week, {CommitsThisMonth} this month";
        public string PullRequestStatusText => $"{OpenPullRequests} open, {MergedPullRequests} merged";
        public string IssueStatusText => $"{OpenIssues} open, {ClosedIssues} closed";
        public string RepositoryStatsText => $"{StarCount} stars, {ForkCount} forks, {WatcherCount} watchers";
        public bool HasRecentActivity => LastCommitDate.HasValue && LastCommitDate.Value > DateTime.UtcNow.AddDays(-7);
    }
}