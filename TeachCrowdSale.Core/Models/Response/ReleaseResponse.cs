using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Response model for release data
    /// </summary>
    public class ReleaseResponse
    {
        public int Id { get; set; }
        public string Version { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string ReleaseType { get; set; } = string.Empty;
        public bool IsPreRelease { get; set; }
        public bool IsDraft { get; set; }
        public string? TagName { get; set; }
        public string? GitHubUrl { get; set; }
        public string? DownloadUrl { get; set; }
        public string? ReleaseNotes { get; set; }
        public List<string>? Assets { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<MilestoneResponse>? Milestones { get; set; }
    }
}
