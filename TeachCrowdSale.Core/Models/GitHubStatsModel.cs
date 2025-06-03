using System.ComponentModel.DataAnnotations;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// GitHub statistics model
    /// </summary>
    public class GitHubStatsModel
    {
        [Range(0, int.MaxValue)]
        public int TotalCommits { get; set; }

        [Range(0, int.MaxValue)]
        public int CommitsThisWeek { get; set; }

        [Range(0, int.MaxValue)]
        public int OpenIssues { get; set; }

        [Range(0, int.MaxValue)]
        public int ClosedIssues { get; set; }

        [Range(0, int.MaxValue)]
        public int OpenPullRequests { get; set; }

        [Range(0, int.MaxValue)]
        public int MergedPullRequests { get; set; }

        [Range(0, int.MaxValue)]
        public int Contributors { get; set; }

        public List<RepositoryStatsModel> Repositories { get; set; } = new();

        public List<CommitActivityModel> RecentActivity { get; set; } = new();

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
