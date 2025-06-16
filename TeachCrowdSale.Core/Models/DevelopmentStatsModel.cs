// TeachCrowdSale.Core/Models/DevelopmentStatsModel.cs
using System.ComponentModel.DataAnnotations;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Consolidated development statistics model for web display
    /// </summary>
    public class DevelopmentStatsModel
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

        // Display helper properties
        public string LinesOfCodeFormatted => TotalLinesOfCode.ToString("N0");
        public string CodeCoverageFormatted => $"{CodeCoverage:F1}%";
        public string TestStatusText => $"{PassingTests}/{TestsCount} passing";
        public string BuildStatusClass => BuildStatus.ToLower() switch
        {
            "passing" => "success",
            "failing" => "danger",
            "building" => "warning",
            _ => "info"
        };
        public string LastBuildFormatted => LastBuildDate?.ToString("MMM dd 'at' h:mm tt") ?? "No recent builds";
        public string TopContributorsText => TopContributors.Count > 0 ? string.Join(", ", TopContributors.Take(3)) : "No contributors";
        public string ActivitySummary => $"{CommitsThisWeek} commits, {FilesChanged} files changed this week";
        public string CodeQualityStatus => CodeCoverage >= 80 ? "excellent" : CodeCoverage >= 60 ? "good" : "needs-improvement";
        public string TechnicalDebtFormatted => $"{TechnicalDebtRatio:F1}%";
        public string LastDeploymentFormatted => LastDeploymentDate?.ToString("MMM dd 'at' h:mm tt") ?? "No recent deployments";
        public bool HasFailingTests => FailingTests > 0;
        public bool IsBuildHealthy => BuildStatus.Equals("passing", StringComparison.OrdinalIgnoreCase) && FailingTests == 0;
    }
}