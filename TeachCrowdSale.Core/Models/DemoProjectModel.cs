using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    public class DemoProjectModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ShortDescription => Description.Length > 120 ? Description.Substring(0, 120) + "..." : Description;
        public string Category { get; set; } = string.Empty;
        public decimal FundingGoal { get; set; }
        public decimal CurrentFunding { get; set; }
        public decimal FundingProgress => FundingGoal > 0 ? (CurrentFunding / FundingGoal) * 100 : 0;
        public string SchoolName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string Location => $"{City}, {State}";
        public string State { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int StudentsImpacted { get; set; }
        public string GradeLevel { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsUrgent { get; set; }
        public bool IsFeatured { get; set; }
        public int DaysRemaining { get; set; }
        public string UrgencyText => IsUrgent ? "Urgent" : $"{DaysRemaining} days left";
    }
}
