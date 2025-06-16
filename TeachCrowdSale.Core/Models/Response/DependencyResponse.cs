using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Data.Enum;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Response model for dependency data
    /// </summary>
    public class DependencyResponse
    {
        public int Id { get; set; }
        public int MilestoneId { get; set; }
        public int DependentMilestoneId { get; set; }
        public DependencyType DependencyType { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string MilestoneTitle { get; set; } = string.Empty;
        public string DependentMilestoneTitle { get; set; } = string.Empty;
    }
}
