using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    public class PencilDriveModel
    {
        public bool IsActive { get; set; } = true;
        public int Year { get; set; } = DateTime.Now.Year;
        public string Title => $"{Year} Annual Pencil Drive";
        public decimal TokensRaised { get; set; }
        public int PencilsSecured { get; set; }
        public int TotalPencilGoal { get; set; } = 2_000_000;
        public decimal ProgressPercentage => TotalPencilGoal > 0 ? (decimal)PencilsSecured / TotalPencilGoal * 100 : 0;
        public string PartnerName { get; set; } = "Major Pencil Manufacturer";
        public int SchoolsApplied { get; set; }
        public int SchoolsApproved { get; set; }
        public int DaysRemaining { get; set; }
        public string StatusText => IsActive ? $"{DaysRemaining} days remaining" : "Drive Complete";

        public List<DriveContribution> Contributions { get; set; } = new()
        {
            new() { Source = "Corporate Partner", Amount = 500_000, Description = "500K pencils committed by our manufacturing partner" },
            new() { Source = "Platform Matching", Amount = 500_000, Description = "500K pencils provided by PencilImpact platform" },
            new() { Source = "TEACH Token Holders", Amount = 0, Description = "Up to 500K more pencils from community donations", IsDynamic = true }
        };
    }
    public class DriveContribution
    {
        public string Source { get; set; } = string.Empty;
        public int Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsDynamic { get; set; } = false;
    }
}
