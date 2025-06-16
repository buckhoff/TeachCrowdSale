using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Request
{
    /// <summary>
    /// Request model for creating/updating tasks
    /// </summary>
    public class TaskRequest
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public Data.Enum.TaskStatus Status { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        public Data.Enum.MilestonePriority Priority { get; set; }

        [Range(0, 100, ErrorMessage = "Progress percentage must be between 0 and 100")]
        public decimal ProgressPercentage { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? CompletionDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Milestone ID is required")]
        public int MilestoneId { get; set; }

        [StringLength(100, ErrorMessage = "Assignee cannot exceed 100 characters")]
        public string? Assignee { get; set; }

        [Range(0, 40, ErrorMessage = "Estimated hours must be between 0 and 40")]
        public decimal? EstimatedHours { get; set; }

        [Range(0, 100, ErrorMessage = "Actual hours must be between 0 and 100")]
        public decimal? ActualHours { get; set; }
    }
}
