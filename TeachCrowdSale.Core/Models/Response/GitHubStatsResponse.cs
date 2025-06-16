using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Response model for GitHub statistics
    /// </summary>
    public class GitHubStatsResponse
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
    }
}
