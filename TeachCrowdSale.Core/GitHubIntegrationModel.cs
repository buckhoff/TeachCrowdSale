using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Models;

namespace TeachCrowdSale.Core
{
    /// <summary>
    /// GitHub integration display model
    /// </summary>
    public class GitHubIntegrationModel
    {
        public GitHubStatsModel Stats { get; set; } = new();
        public List<GitHubCommitModel> RecentCommits { get; set; } = new();
        public List<GitHubIssueModel> RecentIssues { get; set; } = new();
        public List<GitHubContributorModel> TopContributors { get; set; } = new();
        public DateTime LastSyncDate { get; set; }
        public bool IsConnected { get; set; } = true;
        public string ConnectionStatus { get; set; } = "Connected";
        public string RepositoryUrl { get; set; } = "https://github.com/buckhoff/TeachCrowdSale";
    }
}
