using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Roadmap overview summary for dashboard header
    /// </summary>
    public class RoadmapOverviewModel
    {
        public int TotalMilestones { get; set; }
        public int CompletedMilestones { get; set; }
        public int InProgressMilestones { get; set; }
        public int UpcomingMilestones { get; set; }
        public int OnHoldMilestones { get; set; }

        [Range(0, 100)]
        public decimal OverallProgress { get; set; }

        public DateTime? EstimatedCompletionDate { get; set; }
        public DateTime LastUpdateDate { get; set; }

        // Calculated display properties
        public string ProgressDisplayText { get; set; } = string.Empty;
        public string CompletionTimeframe { get; set; } = string.Empty;
        public string LastUpdateTimeAgo { get; set; } = string.Empty;
        public bool IsOnTrack { get; set; } = true;
        public string ProjectHealthStatus { get; set; } = "Healthy";
        public string ProjectHealthClass { get; set; } = "healthy";
    }
}
