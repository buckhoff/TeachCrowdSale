namespace TeachCrowdSale.Core.Models
{
    using System;
    using System.Collections.Generic;
    /// <summary>
    /// Represents a record of progress history for a milestone or task.
    /// </summary>
    /// <remarks>
    /// This model is used to track the progress of milestones or tasks over time,
    /// including the date, percentage completed, and any notes.
    /// </remarks>
    public class ProgressHistoryData
    {
        public DateTime Date { get; set; }
        public decimal ProgressPercentage { get; set; }
        public string? Note { get; set; }
    }
}
