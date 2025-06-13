using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Chart data model for timeline and progress charts
    /// </summary>
    public class RoadmapChartDataModel
    {
        public List<TimelineDataPoint> TimelineData { get; set; } = new();
        public List<ProgressDataPoint> ProgressData { get; set; } = new();
        public List<CategoryDataPoint> CategoryData { get; set; } = new();
        public List<VelocityDataPoint> VelocityData { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
