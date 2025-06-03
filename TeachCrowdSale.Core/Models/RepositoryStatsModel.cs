using System.ComponentModel.DataAnnotations;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Repository statistics model
    /// </summary>
    public class RepositoryStatsModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int Stars { get; set; }

        [Range(0, int.MaxValue)]
        public int Forks { get; set; }

        [Range(0, int.MaxValue)]
        public int OpenIssues { get; set; }

        public DateTime LastCommit { get; set; }
        public bool IsActive { get; set; }
    }
}
