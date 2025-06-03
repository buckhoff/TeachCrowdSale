// TeachCrowdSale.Core/Models/Response/RoadmapFilterModel.cs
namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Roadmap filter options model
    /// </summary>
    public class RoadmapFilterModel
    {
        public List<string> Categories { get; set; } = new();
        public List<string> Statuses { get; set; } = new();
        public List<string> Priorities { get; set; } = new();
        public List<string> Developers { get; set; } = new();
        public List<string> Types { get; set; } = new();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}