using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Data.Enum;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Response model for milestone update data
    /// </summary>
    public class UpdateResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public UpdateType UpdateType { get; set; }
        public int MilestoneId { get; set; }
        public string? Author { get; set; }
        public List<string>? Tags { get; set; }
        public List<string>? Attachments { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string MilestoneTitle { get; set; } = string.Empty;
    }
}
