using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    public class FundAllocationResponse
    {
        public string Category { get; set; } = string.Empty; // Development, Marketing, Operations, etc.

        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        public decimal Percentage { get; set; }

        public string Status { get; set; } = string.Empty; // Allocated, Spent, Reserved

        public DateTime LastUpdated { get; set; }
    }
}
