using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// GitHub API milestone model
    /// </summary>
    public class GitHubApiMilestone
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public DateTime? DueOn { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int OpenIssues { get; set; }
        public int ClosedIssues { get; set; }
    }
}
