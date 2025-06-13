using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// GitHub commit model for display
    /// </summary>
    public class GitHubCommitModel
    {
        public string Sha { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ShortMessage { get; set; } = string.Empty; // Truncated
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorEmail { get; set; } = string.Empty;
        public string AuthorAvatar { get; set; } = string.Empty;
        public DateTime CommitDate { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string CommitUrl { get; set; } = string.Empty;
        public List<string> ChangedFiles { get; set; } = new();
        public int AdditionsCount { get; set; }
        public int DeletionsCount { get; set; }
    }
}
