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

        public DateFilterModel DateRange { get; set; } = new();

        public string SelectedStatus { get; set; } = string.Empty;
        public string SelectedCategory { get; set; } = string.Empty;
        public string SelectedPriority { get; set; } = string.Empty;
        public string SearchTerm { get; set; } = string.Empty;
        public bool ShowCompleted { get; set; } = true;
        public bool ShowOnHold { get; set; } = false;
    }
}