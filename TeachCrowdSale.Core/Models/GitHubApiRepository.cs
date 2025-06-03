using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{

    public class GitHubApiRepository
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string HtmlUrl { get; set; } = string.Empty;
        public string? Language { get; set; }
        public int StargazersCount { get; set; }
        public int ForksCount { get; set; }
        public int OpenIssuesCount { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
