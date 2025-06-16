using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Response model for development statistics
    /// </summary>
    public class DevelopmentStatsResponse
    {
        public int TotalLinesOfCode { get; set; }
        public int FilesChanged { get; set; }
        public int CommitsThisWeek { get; set; }
        public int CommitsThisMonth { get; set; }
        public int ActiveBranches { get; set; }
        public decimal CodeCoverage { get; set; }
        public int TestsCount { get; set; }
        public int PassingTests { get; set; }
        public int FailingTests { get; set; }
        public string BuildStatus { get; set; } = string.Empty;
        public DateTime? LastBuildDate { get; set; }
        public int AverageCommitSize { get; set; }
        public List<string> TopContributors { get; set; } = new();
        public string MostActiveRepository { get; set; } = string.Empty;
        public int TotalRepositories { get; set; }
        public int OpenPullRequests { get; set; }
        public int CodeReviewsCompleted { get; set; }
        public string DeploymentFrequency { get; set; } = string.Empty;
        public DateTime? LastDeploymentDate { get; set; }
        public decimal TechnicalDebtRatio { get; set; }
    }
}
