using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Response model for platform dashboard data
    /// </summary>
    public class PlatformDashboardResponse
    {
        public TokenSaleMetrics TokenSale { get; set; } = new();
        public PlatformVisionMetrics Vision { get; set; } = new();
        public List<DemoProjectResponse> FeaturedProjects { get; set; } = new();
        public PencilDriveResponse CurrentPencilDrive { get; set; } = new();
        public WaitlistStats WaitlistStats { get; set; } = new();
    }

    public class TokenSaleMetrics
    {
        public decimal TotalRaised { get; set; }
        public decimal TokenPrice { get; set; }
        public int TotalHolders { get; set; }
        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }
        public bool IsSaleActive { get; set; }
        public decimal SaleProgress { get; set; }
    }

    public class PlatformVisionMetrics
    {
        public int ProjectedSchools { get; set; } = 10000;
        public decimal ProjectedFunding { get; set; } = 100_000_000;
        public int ProjectedStudents { get; set; } = 5_000_000;
        public string LaunchTimeline { get; set; } = "Q2 2026";
    }

    public class DemoProjectResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal FundingGoal { get; set; }
        public decimal CurrentFunding { get; set; }
        public decimal FundingProgress { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int StudentsImpacted { get; set; }
        public string GradeLevel { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsUrgent { get; set; }
        public bool IsFeatured { get; set; }
        public int DaysRemaining { get; set; }
    }

    public class PencilDriveResponse
    {
        public bool IsActive { get; set; }
        public int Year { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TokensRaised { get; set; }
        public int PencilsSecured { get; set; }
        public int TotalPencilGoal { get; set; } = 2_000_000;
        public decimal ProgressPercentage { get; set; }
        public string PartnerName { get; set; } = string.Empty;
        public string? PartnerLogoUrl { get; set; }
        public int SchoolsApplied { get; set; }
        public int SchoolsApproved { get; set; }
        public int DaysRemaining { get; set; }
    }

    public class WaitlistStats
    {
        public int TotalSignups { get; set; }
        public int EducatorSignups { get; set; }
        public int DonorSignups { get; set; }
        public int PartnerSignups { get; set; }
        public int TEACHTokenInterested { get; set; }
        public DateTime? LastSignupDate { get; set; }
    }
}
