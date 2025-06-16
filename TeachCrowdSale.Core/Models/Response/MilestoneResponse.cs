using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Data.Enum;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Response model for milestone data
    /// </summary>
    public class MilestoneResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public MilestoneStatus Status { get; set; }
        public MilestonePriority Priority { get; set; }
        public decimal ProgressPercentage { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }
        public string? DurationEstimate { get; set; }
        public int CompletedTasksCount { get; set; }
        public int TotalTasksCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<TaskResponse>? Tasks { get; set; }
        public List<DependencyResponse>? Dependencies { get; set; }
        public List<UpdateResponse>? Updates { get; set; }
    }
}
