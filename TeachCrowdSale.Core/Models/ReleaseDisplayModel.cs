﻿namespace TeachCrowdSale.Core.Models {

    /// <summary>
    /// Release display model for UI
    /// </summary>
    /// <!-- This model is used to represent a software release in the UI, including its details, status, and related information. -->-->
    public class ReleaseDisplayModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = string.Empty;
        public string TypeClass { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;

        public DateTime? PlannedReleaseDate { get; set; }
        public DateTime? ActualReleaseDate { get; set; }

        public string? ReleaseNotes { get; set; }
        public string? GitHubReleaseUrl { get; set; }
        public string? DocumentationUrl { get; set; }

        public List<string> IncludedMilestones { get; set; } = new();

        // Calculated fields
        public string FormattedReleaseDate { get; set; } = string.Empty;
        public bool IsOverdue { get; set; }
        public string TimeToRelease { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty; 
        public DateTime? ReleaseDate { get; set; } 
        public bool IsPreRelease { get; set; } 
        public string? DownloadUrl { get; set; } 
    }
}
