using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// School impact model
    /// </summary>
    public class SchoolImpactModel
    {
        public int SchoolId { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal TotalReceived { get; set; }

        [Range(0, int.MaxValue)]
        public int StudentCount { get; set; }

        [Range(0, int.MaxValue)]
        public int SupporterCount { get; set; }

        public DateTime LastDistribution { get; set; }
    }
}
