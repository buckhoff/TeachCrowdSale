using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Display model for PencilImpact home page
    /// </summary>
    public class PlatformHomeModel
    {
        public string PageTitle { get; set; } = "PencilImpact - Small Tokens, Big Impacts";
        public string MetaDescription { get; set; } = "Join the revolution in education funding. Use TEACH tokens to support teachers and students while earning governance rights and staking rewards.";

        // Hero Section Data
        public HeroSectionModel Hero { get; set; } = new();

        // Token Integration Data
        public TokenIntegrationModel TokenIntegration { get; set; } = new();

        // Platform Vision Data
        public PlatformVisionModel Vision { get; set; } = new();

        // Featured Projects
        public List<DemoProjectModel> FeaturedProjects { get; set; } = new();

        // Pencil Drive Showcase
        public PencilDriveModel PencilDrive { get; set; } = new();

        // Waitlist Form Data
        public WaitlistFormModel WaitlistForm { get; set; } = new();

        // Analytics Data (for JavaScript)
        public string AnalyticsData { get; set; } = "{}";
    }

   

   

   

   

    

   

    



    

    

   
}
