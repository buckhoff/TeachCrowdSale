using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// School beneficiary model
    /// </summary>
    public class SchoolBeneficiaryModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int StudentCount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalReceived { get; set; }

        [Range(0, int.MaxValue)]
        public int SupporterCount { get; set; }

        public bool IsVerified { get; set; }
        public bool IsSelected { get; set; }
        public string Location { get; set; } = string.Empty;
        public string ImpactSummary { get; set; } = string.Empty;
    }
}
