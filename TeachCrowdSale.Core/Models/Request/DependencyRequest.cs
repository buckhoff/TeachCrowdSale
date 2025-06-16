using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Data.Enum;

namespace TeachCrowdSale.Core.Models.Request
{
    /// <summary>
    /// Request model for creating/updating dependencies
    /// </summary>
    public class DependencyRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Milestone ID is required")]
        public int MilestoneId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Dependent milestone ID is required")]
        public int DependentMilestoneId { get; set; }

        [Required(ErrorMessage = "Dependency type is required")]
        public DependencyType DependencyType { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
