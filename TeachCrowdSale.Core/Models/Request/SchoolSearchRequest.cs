using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Request
{
    /// <summary>
    /// School search request model
    /// </summary>
    public class SchoolSearchRequest
    {
        [StringLength(100, ErrorMessage = "Country name cannot exceed 100 characters")]
        public string? Country { get; set; }

        [StringLength(100, ErrorMessage = "State name cannot exceed 100 characters")]
        public string? State { get; set; }

        [StringLength(100, ErrorMessage = "City name cannot exceed 100 characters")]
        public string? City { get; set; }

        [StringLength(200, ErrorMessage = "Search term cannot exceed 200 characters")]
        public string? SearchTerm { get; set; }

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 20;

        [Range(1, int.MaxValue, ErrorMessage = "Page number must be a positive number")]
        public int PageNumber { get; set; } = 1;

        public bool VerifiedOnly { get; set; } = true;
    }
}
