using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// GitHub contributor model for display
    /// </summary>
    public class GitHubContributorModel
    {
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string ProfileUrl { get; set; } = string.Empty;
        public int Contributions { get; set; }
        public int CommitsCount { get; set; }
        public DateTime LastContribution { get; set; }
        public string Role { get; set; } = "Contributor";
        public string RoleClass { get; set; } = "contributor";
    }
}
