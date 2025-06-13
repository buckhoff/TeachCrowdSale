using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Development statistics model for widgets
    /// </summary>
    public class DevelopmentStatsModel
    {
        // Code metrics
        public int TotalCommits { get; set; }
        public int ActiveBranches { get; set; }
        public int OpenIssues { get; set; }
        public int ClosedIssues { get; set; }
        public int PullRequests { get; set; }
        public int LinesOfCode { get; set; }

        // Quality metrics
        public decimal CodeCoverage { get; set; }
        public string TechnicalDebt { get; set; } = string.Empty;
        public int SecurityVulnerabilities { get; set; }
        public string CodeQualityGrade { get; set; } = "A";

        // Team metrics
        public int Contributors { get; set; }
        public int ActiveContributors { get; set; }
        public DateTime LastDeployment { get; set; }
        public DateTime LastCommit { get; set; }

        // Display properties
        public string LastDeploymentText { get; set; } = string.Empty;
        public string LastCommitText { get; set; } = string.Empty;
        public string ActivityLevel { get; set; } = "High";
        public string ActivityLevelClass { get; set; } = "activity-high";
        public bool IsHealthy { get; set; } = true;
    }
}
