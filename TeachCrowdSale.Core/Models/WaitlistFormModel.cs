using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    public class WaitlistFormModel
    {
        public List<UserTypeOption> UserTypes { get; set; } = new()
        {
            new() { Value = "Educator", Label = "Educator/Teacher", Description = "I work in education and want to create funding projects" },
            new() { Value = "Donor", Label = "Donor/Supporter", Description = "I want to support educational projects and earn TEACH tokens" },
            new() { Value = "Partner", Label = "Organization/Partner", Description = "I represent an organization interested in partnerships" }
        };

        public List<SubjectOption> TeachingSubjects { get; set; } = new()
        {
            new() { Value = "STEM", Label = "STEM (Science, Technology, Engineering, Math)" },
            new() { Value = "Arts", Label = "Arts & Creativity" },
            new() { Value = "Literacy", Label = "Literacy & Language Arts" },
            new() { Value = "SocialStudies", Label = "Social Studies & History" },
            new() { Value = "PhysicalEd", Label = "Physical Education & Health" },
            new() { Value = "SpecialEd", Label = "Special Education" },
            new() { Value = "Multiple", Label = "Multiple Subjects" },
            new() { Value = "Other", Label = "Other" }
        };
    }

    public class UserTypeOption
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class SubjectOption
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
