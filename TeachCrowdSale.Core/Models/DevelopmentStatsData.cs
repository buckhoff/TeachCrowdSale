// TeachCrowdSale.Core/Interfaces/IRoadmapService.cs
// Helper data classes for statistics
public class DevelopmentStatsData
    {
        public int TotalMilestones { get; set; }
        public int CompletedMilestones { get; set; }
        public int InProgressMilestones { get; set; }
        public int PlannedMilestones { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int BlockedTasks { get; set; }
        public int ActiveDevelopers { get; set; }
        public double AverageCompletionTime { get; set; }
        public DateTime LastUpdated { get; set; }
    }
