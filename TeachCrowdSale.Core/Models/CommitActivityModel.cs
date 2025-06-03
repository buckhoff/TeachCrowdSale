namespace TeachCrowdSale.Core.Models { 
    /// <summary>
    /// Commit activity model
    /// </summary>
    public class CommitActivityModel
    {
        public string CommitHash { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Repository { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Url { get; set; } = string.Empty;
        public string FormattedDate { get; set; } = string.Empty;
    }
}
