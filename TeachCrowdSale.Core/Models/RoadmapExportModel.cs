using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Export data model for CSV/PDF exports
    /// </summary>
    public class RoadmapExportModel
    {
        public string ExportType { get; set; } = "csv"; // csv, pdf, json
        public List<MilestoneDisplayModel> Milestones { get; set; } = new();
        public RoadmapFilterModel AppliedFilters { get; set; } = new();
        public DateTime ExportDate { get; set; } = DateTime.UtcNow;
        public string ExportedBy { get; set; } = "Anonymous";
        public string FileName { get; set; } = string.Empty;
    }
}
