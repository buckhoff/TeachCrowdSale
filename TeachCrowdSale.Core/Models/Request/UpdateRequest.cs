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
    /// Request model for creating/updating milestone updates
    /// </summary>
    public class UpdateRequest
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required")]
        [StringLength(5000, ErrorMessage = "Content cannot exceed 5000 characters")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Update type is required")]
        public UpdateType UpdateType { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Milestone ID is required")]
        public int MilestoneId { get; set; }

        [StringLength(100, ErrorMessage = "Author cannot exceed 100 characters")]
        public string? Author { get; set; }

        public List<string>? Tags { get; set; }

        public List<string>? Attachments { get; set; }
    }
}
