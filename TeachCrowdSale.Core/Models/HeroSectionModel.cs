using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    public class HeroSectionModel
    {
        public string MainHeadline { get; set; } = "Small Tokens, Big Impacts";
        public string SubHeadline { get; set; } = "Revolutionizing education funding with transparent, efficient, and community-driven support for teachers and students.";
        public string CtaText { get; set; } = "Join the Waitlist";
        public string SecondaryCtaText { get; set; } = "Explore Demo";
        public List<HeroBenefit> Benefits { get; set; } = new()
        {
            new() { Icon = "🎓", Text = "Direct Teacher Support" },
            new() { Icon = "🪙", Text = "TEACH Token Rewards" },
            new() { Icon = "📊", Text = "Transparent Impact" },
            new() { Icon = "🌟", Text = "Community Governance" }
        };
    }
    public class HeroBenefit
    {
        public string Icon { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
