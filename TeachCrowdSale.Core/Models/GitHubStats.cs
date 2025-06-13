using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// GitHub statistics model
    /// </summary>
    public class GitHubStats
    {
        public int Stars { get; set; }
        public int Forks { get; set; }
        public int Contributors { get; set; }
        public int RecentCommits { get; set; }
        public int OpenIssues { get; set; }
        public int ClosedIssues { get; set; }
        public DateTime LastCommitDate { get; set; }
        public string Repository { get; set; } = string.Empty;
        public string MainBranch { get; set; } = "main";
        public string RepositoryUrl { get; set; } = string.Empty;
        public decimal CommitFrequency { get; set; } // Commits per week
        public string HealthScore { get; set; } = "Excellent";
        public string HealthScoreClass { get; set; } = "health-excellent";
    }
}
