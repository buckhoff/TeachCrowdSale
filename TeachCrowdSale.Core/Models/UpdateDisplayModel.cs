namespace TeachCrowdSale.Core.Models
{   
    /// <summary>
    /// Update display model for UI
    /// </summary>
    public class UpdateDisplayModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string TypeClass { get; set; } = string.Empty;
        public decimal? ProgressChange { get; set; }
        public string? AuthorName { get; set; }
        public DateTime CreatedAt { get; set; }

        // Related milestone info
        public int MilestoneId { get; set; }
        public string MilestoneTitle { get; set; } = string.Empty;

        // Formatted fields
        public string FormattedDate { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
    }
}
