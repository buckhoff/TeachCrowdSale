namespace TeachCrowdSale.Core.Models
{ /// <summary>
  /// GitHub API label model
  /// </summary>
    public class GitHubApiLabel
    {
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Default { get; set; }
    }
}
