using TeachCrowdSale.Core.Data.Enum;
using TeachCrowdSale.Core.Helper;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Update display model for UI
    /// </summary>
    public class UpdateModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string UpdateType { get; set; } = string.Empty;
        public string UpdateTypeClass { get; set; } = string.Empty;
        public int MilestoneId { get; set; }
        public string MilestoneTitle { get; set; } = string.Empty;
        public string? Author { get; set; }
        public List<string>? Tags { get; set; }
        public List<string>? Attachments { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Display helper properties
        public string CreatedAtFormatted => CreatedAt.ToString("MMM dd, yyyy 'at' h:mm tt");
        public string AuthorDisplay => !string.IsNullOrEmpty(Author) ? Author : "System";
        public string UpdateTypeIcon => DisplayHelpers.GetUpdateTypeIcon(UpdateType);
        public bool HasTags => Tags?.Any() == true;
        public bool HasAttachments => Attachments?.Any() == true;
        public string ContentPreview => Content.Length > 150 ? Content[..150] + "..." : Content;
    }
}
