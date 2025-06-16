using TeachCrowdSale.Core.Data.Enum;
using TeachCrowdSale.Core.Helper;

namespace TeachCrowdSale.Core.Models {

    /// <summary>
    /// Release display model for UI
    /// </summary>
    /// <!-- This model is used to represent a software release in the UI, including its details, status, and related information. -->-->
    public class ReleaseModel
    {
        public int Id { get; set; }
        public string Version { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string ReleaseType { get; set; } = string.Empty;
        public string ReleaseTypeClass { get; set; } = string.Empty;
        public bool IsPreRelease { get; set; }
        public bool IsDraft { get; set; }
        public string? TagName { get; set; }
        public string? GitHubUrl { get; set; }
        public string? DownloadUrl { get; set; }
        public string? ReleaseNotes { get; set; }
        public List<string>? Assets { get; set; }
        public DateTime CreatedAt { get; set; }

        // Display helper properties
        public string ReleaseDateFormatted => ReleaseDate.ToString("MMM dd, yyyy");
        public string VersionDisplay => IsPreRelease ? $"{Version} (Pre-release)" : Version;
        public string ReleaseTypeIcon => DisplayHelpers.GetReleaseTypeIcon(ReleaseType);
        public string StatusBadge => IsPreRelease ? "pre-release" : IsDraft ? "draft" : "stable";
        public bool HasAssets => Assets?.Any() == true;
        public bool HasGitHubLink => !string.IsNullOrEmpty(GitHubUrl);
        public bool HasDownload => !string.IsNullOrEmpty(DownloadUrl);
        public string DescriptionPreview => !string.IsNullOrEmpty(Description) && Description.Length > 100
            ? Description[..100] + "..." : Description ?? "";
    }
}
