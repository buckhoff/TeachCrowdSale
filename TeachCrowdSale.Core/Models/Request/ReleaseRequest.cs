using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Request
{
    /// <summary>
    /// Request model for creating/updating releases
    /// </summary>
    public class ReleaseRequest
    {
        [Required(ErrorMessage = "Version is required")]
        [StringLength(50, ErrorMessage = "Version cannot exceed 50 characters")]
        public string Version { get; set; } = string.Empty;

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Release date is required")]
        public DateTime ReleaseDate { get; set; }

        [Required(ErrorMessage = "Release type is required")]
        [StringLength(50, ErrorMessage = "Release type cannot exceed 50 characters")]
        public string ReleaseType { get; set; } = string.Empty;

        public bool IsPreRelease { get; set; }

        public bool IsDraft { get; set; }

        [StringLength(100, ErrorMessage = "Tag name cannot exceed 100 characters")]
        public string? TagName { get; set; }

        [StringLength(500, ErrorMessage = "GitHub URL cannot exceed 500 characters")]
        public string? GitHubUrl { get; set; }

        [StringLength(500, ErrorMessage = "Download URL cannot exceed 500 characters")]
        public string? DownloadUrl { get; set; }

        [StringLength(10000, ErrorMessage = "Release notes cannot exceed 10000 characters")]
        public string? ReleaseNotes { get; set; }

        public List<string>? Assets { get; set; }

        public List<int>? MilestoneIds { get; set; }
    }
}
