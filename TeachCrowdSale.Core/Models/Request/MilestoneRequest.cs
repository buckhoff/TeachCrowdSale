using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Data.Enum;

namespace TeachCrowdSale.Core.Models.Request
{
    public class MilestoneRequest
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required")]
        public MilestoneStatus Status { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        public MilestonePriority Priority { get; set; }

        [Range(0, 100, ErrorMessage = "Progress percentage must be between 0 and 100")]
        public decimal ProgressPercentage { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EstimatedCompletionDate { get; set; }

        public DateTime? ActualCompletionDate { get; set; }

        [StringLength(100, ErrorMessage = "Duration estimate cannot exceed 100 characters")]
        public string? DurationEstimate { get; set; }

        public List<int>? DependencyIds { get; set; }

        public List<TaskRequest>? Tasks { get; set; }
    }
}
