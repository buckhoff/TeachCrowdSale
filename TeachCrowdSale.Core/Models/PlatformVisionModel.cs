using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    public class PlatformVisionModel
    {
        public string VisionStatement { get; set; } = "Creating a world where every educator has the resources they need to inspire and educate the next generation.";
        public List<VisionMetric> Metrics { get; set; } = new()
        {
            new() { Label = "Schools Supported", Value = "10,000+", Icon = "🏫" },
            new() { Label = "Annual Funding", Value = "$100M+", Icon = "💰" },
            new() { Label = "Students Impacted", Value = "5M+", Icon = "🎓" },
            new() { Label = "Platform Launch", Value = "Q2 2026", Icon = "🚀" }
        };
        public List<PlatformFeature> Features { get; set; } = new()
        {
            new() { Title = "AI-Powered Matching", Description = "Smart algorithms connect donors with projects that align with their values", Icon = "🤖", ComingSoon = true },
            new() { Title = "Real-Time Impact", Description = "See exactly how your donations are making a difference", Icon = "📊", ComingSoon = false },
            new() { Title = "School Verification", Description = "Every school and teacher is verified through official databases", Icon = "✅", ComingSoon = false },
            new() { Title = "Annual Pencil Drive", Description = "Community-wide initiative to provide pencils to schools in need", Icon = "✏️", ComingSoon = false }
        };
    }

    public class VisionMetric
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }

    public class PlatformFeature
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public bool ComingSoon { get; set; }
    }
}
