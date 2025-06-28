using Microsoft.EntityFrameworkCore;
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Infrastructure.Data.Context;

namespace TeachCrowdSale.Infrastructure.Data.SeedData
{
    /// <summary>
    /// Seed data for PencilImpact concept site
    /// </summary>
    public static class PencilImpactSeeder
    {
        public static async Task SeedAsync(TeachCrowdSaleDbContext context)
        {
            // Check if data already exists
            if (await context.PencilDrives.AnyAsync() || await context.DemoProjects.AnyAsync())
            {
                return; // Data already seeded
            }

            await SeedPencilDrive(context);
            await SeedDemoProjects(context);
            await SeedImpactStories(context);

            await context.SaveChangesAsync();
        }

        private static async Task SeedPencilDrive(TeachCrowdSaleDbContext context)
        {
            var currentYear = DateTime.Now.Year;

            var pencilDrive = new PencilDrive
            {
                Id = Guid.NewGuid(),
                Year = currentYear,
                StartDate = new DateTime(currentYear, 9, 1), // September 1st
                EndDate = new DateTime(currentYear, 11, 30), // November 30th
                PencilGoal = 2_000_000,
                TokensRaised = 4250, // $42,500 worth of tokens
                PencilsDistributed = 0,
                SchoolsApplied = 147,
                SchoolsApproved = 89,
                PartnerName = "Ticonderoga Pencil Company",
                PartnerLogoUrl = "/images/partners/ticonderoga-logo.png",
                PartnerPencilsCommitted = 500_000,
                PlatformPencilsCommitted = 500_000,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.PencilDrives.Add(pencilDrive);
        }

        private static async Task SeedDemoProjects(TeachCrowdSaleDbContext context)
        {
            var demoProjects = new List<DemoProject>
            {
                new DemoProject
                {
                    Id = Guid.NewGuid(),
                    Title = "STEM Lab Equipment for Rural School",
                    Description = "Help us create a modern science laboratory for our students in rural Montana. We need microscopes, chemistry sets, and basic lab equipment to give our 8th graders hands-on learning experiences they've never had before. Our school serves a farming community where many students have never seen a microscope up close. With your support, we can open up the world of science and potentially inspire the next generation of researchers, doctors, and engineers.",
                    Category = "STEM",
                    FundingGoal = 2500.00m,
                    CurrentFunding = 1890.00m,
                    SchoolName = "Mountain View Middle School",
                    TeacherName = "Sarah Johnson",
                    State = "MT",
                    City = "Billings",
                    StudentsImpacted = 85,
                    GradeLevel = "6-8",
                    ImageUrl = "/images/projects/stem-lab-montana.jpg",
                    Deadline = DateTime.UtcNow.AddDays(23),
                    IsUrgent = false,
                    IsFeatured = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new DemoProject
                {
                    Id = Guid.NewGuid(),
                    Title = "Books for Classroom Library",
                    Description = "Our students deserve access to diverse, engaging books that reflect their experiences and interests. Help us stock our classroom library with 100 new books including graphic novels, diverse fiction, and non-fiction that will inspire a love of reading in our 3rd-5th graders. Many of our students don't have books at home, making our classroom library their primary access to literature.",
                    Category = "Literacy",
                    FundingGoal = 800.00m,
                    CurrentFunding = 320.00m,
                    SchoolName = "Lincoln Elementary",
                    TeacherName = "Maria Rodriguez",
                    State = "TX",
                    City = "Houston",
                    StudentsImpacted = 32,
                    GradeLevel = "3-5",
                    ImageUrl = "/images/projects/classroom-library-texas.jpg",
                    Deadline = DateTime.UtcNow.AddDays(8),
                    IsUrgent = true,
                    IsFeatured = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-22),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new DemoProject
                {
                    Id = Guid.NewGuid(),
                    Title = "Art Supplies for Creative Expression",
                    Description = "Give our students the tools to express their creativity and develop their artistic talents! We need paints, brushes, canvases, clay, and other art supplies for our elementary art program. Art education is crucial for developing creativity, problem-solving skills, and self-expression. With proper supplies, our students can explore different mediums and discover their artistic potential.",
                    Category = "Arts",
                    FundingGoal = 1200.00m,
                    CurrentFunding = 750.00m,
                    SchoolName = "Riverside Elementary",
                    TeacherName = "David Kim",
                    State = "CA",
                    City = "San Diego",
                    StudentsImpacted = 120,
                    GradeLevel = "K-5",
                    ImageUrl = "/images/projects/art-supplies-california.jpg",
                    Deadline = DateTime.UtcNow.AddDays(45),
                    IsUrgent = false,
                    IsFeatured = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-8),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new DemoProject
                {
                    Id = Guid.NewGuid(),
                    Title = "Chromebooks for Digital Learning",
                    Description = "Bridge the digital divide by helping us provide Chromebooks for students who don't have access to technology at home. Each device will serve multiple students throughout the school year, enabling them to complete digital assignments, research projects, and develop essential 21st-century skills. Our goal is to ensure every student has equal access to educational technology.",
                    Category = "Technology",
                    FundingGoal = 3500.00m,
                    CurrentFunding = 1200.00m,
                    SchoolName = "Jefferson High School",
                    TeacherName = "Amanda Chen",
                    State = "OH",
                    City = "Cleveland",
                    StudentsImpacted = 60,
                    GradeLevel = "9-12",
                    ImageUrl = "/images/projects/chromebooks-ohio.jpg",
                    Deadline = DateTime.UtcNow.AddDays(67),
                    IsUrgent = false,
                    IsFeatured = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow
                },
                new DemoProject
                {
                    Id = Guid.NewGuid(),
                    Title = "Musical Instruments for School Band",
                    Description = "Music education is being cut from many schools, but we believe in its power to transform lives. Help us purchase instruments for students who can't afford their own, including trumpets, clarinets, drums, and keyboards. Music teaches discipline, teamwork, and provides an outlet for creative expression that many of our students desperately need.",
                    Category = "Arts",
                    FundingGoal = 4200.00m,
                    CurrentFunding = 850.00m,
                    SchoolName = "Central High School",
                    TeacherName = "Robert Williams",
                    State = "FL",
                    City = "Tampa",
                    StudentsImpacted = 45,
                    GradeLevel = "6-12",
                    ImageUrl = "/images/projects/band-instruments-florida.jpg",
                    Deadline = DateTime.UtcNow.AddDays(34),
                    IsUrgent = false,
                    IsFeatured = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-12),
                    UpdatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new DemoProject
                {
                    Id = Guid.NewGuid(),
                    Title = "Garden Tools for Outdoor Learning",
                    Description = "Transform our unused courtyard into an educational garden where students can learn about biology, nutrition, and environmental science hands-on. We need gardening tools, seeds, soil, and raised bed materials. This outdoor classroom will provide real-world learning experiences and teach students about sustainable agriculture and healthy eating habits.",
                    Category = "STEM",
                    FundingGoal = 1800.00m,
                    CurrentFunding = 425.00m,
                    SchoolName = "Green Valley Elementary",
                    TeacherName = "Lisa Thompson",
                    State = "NY",
                    City = "Albany",
                    StudentsImpacted = 95,
                    GradeLevel = "K-6",
                    ImageUrl = "/images/projects/school-garden-newyork.jpg",
                    Deadline = DateTime.UtcNow.AddDays(52),
                    IsUrgent = false,
                    IsFeatured = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-18),
                    UpdatedAt = DateTime.UtcNow.AddDays(-4)
                },
                new DemoProject
                {
                    Id = Guid.NewGuid(),
                    Title = "Special Education Sensory Tools",
                    Description = "Our special education students need specialized tools to help them learn and regulate their emotions. We're requesting sensory balls, weighted blankets, fidget tools, and adaptive learning materials that will help our students with autism and other special needs succeed in the classroom. These tools are essential for creating an inclusive learning environment.",
                    Category = "Special Education",
                    FundingGoal = 1500.00m,
                    CurrentFunding = 1120.00m,
                    SchoolName = "Harmony Elementary",
                    TeacherName = "Jennifer Brown",
                    State = "WA",
                    City = "Seattle",
                    StudentsImpacted = 25,
                    GradeLevel = "K-8",
                    ImageUrl = "/images/projects/sensory-tools-washington.jpg",
                    Deadline = DateTime.UtcNow.AddDays(15),
                    IsUrgent = true,
                    IsFeatured = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-28),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new DemoProject
                {
                    Id = Guid.NewGuid(),
                    Title = "Sports Equipment for Physical Education",
                    Description = "Physical education is crucial for student health and development, but our equipment is outdated and falling apart. Help us purchase new basketballs, soccer balls, volleyball nets, and other sports equipment so our students can stay active and learn the importance of teamwork and healthy living. Every child deserves access to quality physical education.",
                    Category = "Physical Education",
                    FundingGoal = 2200.00m,
                    CurrentFunding = 340.00m,
                    SchoolName = "Westside Middle School",
                    TeacherName = "Coach Martinez",
                    State = "AZ",
                    City = "Phoenix",
                    StudentsImpacted = 200,
                    GradeLevel = "6-8",
                    ImageUrl = "/images/projects/sports-equipment-arizona.jpg",
                    Deadline = DateTime.UtcNow.AddDays(41),
                    IsUrgent = false,
                    IsFeatured = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.DemoProjects.AddRange(demoProjects);
        }

        private static async Task SeedImpactStories(TeachCrowdSaleDbContext context)
        {
            // Get the pencil drive we just created
            var pencilDrive = context.PencilDrives.Local.FirstOrDefault();
            if (pencilDrive == null) return;

            var impactStories = new List<PencilDriveImpactStory>
            {
                new PencilDriveImpactStory
                {
                    Id = Guid.NewGuid(),
                    PencilDriveId = pencilDrive.Id,
                    SchoolName = "Oakwood Elementary",
                    TeacherName = "Mrs. Patterson",
                    State = "KY",
                    City = "Louisville",
                    PencilsReceived = 2500,
                    StudentsImpacted = 150,
                    StoryText = "Thanks to the PencilImpact drive, our students started the school year with brand new pencils for the first time in years. The excitement on their faces when we distributed the pencils was priceless. One student told me, 'Mrs. Patterson, I've never had a pencil that was just mine!' This simple gesture made such a huge impact on their confidence and readiness to learn.",
                    ImageUrl = "/images/stories/oakwood-elementary-story.jpg",
                    VideoUrl = null,
                    IsFeatured = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-60)
                },
                new PencilDriveImpactStory
                {
                    Id = Guid.NewGuid(),
                    PencilDriveId = pencilDrive.Id,
                    SchoolName = "Rosa Parks Elementary",
                    TeacherName = "Mr. Johnson",
                    State = "MI",
                    City = "Detroit",
                    PencilsReceived = 1800,
                    StudentsImpacted = 120,
                    StoryText = "The pencil drive came at exactly the right time. Many of our families are struggling financially, and school supplies are often a luxury they can't afford. When we received the pencils from PencilImpact, we were able to ensure every student had the tools they needed to succeed. The relief on parents' faces during our supply pickup was evident - one less worry for families already facing so much.",
                    ImageUrl = "/images/stories/rosa-parks-elementary-story.jpg",
                    VideoUrl = null,
                    IsFeatured = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-45)
                },
                new PencilDriveImpactStory
                {
                    Id = Guid.NewGuid(),
                    PencilDriveId = pencilDrive.Id,
                    SchoolName = "Mountain Ridge High School",
                    TeacherName = "Ms. Garcia",
                    State = "CO",
                    City = "Denver",
                    PencilsReceived = 3200,
                    StudentsImpacted = 400,
                    StoryText = "Our rural school serves students from a wide geographic area, and many families drive long distances just to get to school. The pencil drive helped eliminate one barrier to education by ensuring our students had the basic supplies they needed. We distributed pencils not just to students, but also sent extras home so they'd have supplies for homework and projects. The TEACH token community's support means everything to our students.",
                    ImageUrl = "/images/stories/mountain-ridge-high-story.jpg",
                    VideoUrl = null,
                    IsFeatured = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                }
            };

            context.PencilDriveImpactStories.AddRange(impactStories);
        }

        /// <summary>
        /// Seed additional sample data for development/demo purposes
        /// </summary>
        public static async Task SeedDevelopmentDataAsync(TeachCrowdSaleDbContext context)
        {
            await SeedSampleWaitlistEntries(context);
            await SeedSampleAnalytics(context);

            await context.SaveChangesAsync();
        }

        private static async Task SeedSampleWaitlistEntries(TeachCrowdSaleDbContext context)
        {
            if (await context.PlatformWaitlists.AnyAsync()) return;

            var waitlistEntries = new List<PlatformWaitlist>
            {
                new PlatformWaitlist
                {
                    Id = Guid.NewGuid(),
                    Email = "sarah.teacher@example.com",
                    UserType = "Educator",
                    SchoolDistrict = "Mountain View School District",
                    TeachingSubject = "STEM",
                    InterestedInTEACHTokens = true,
                    SubscribeToUpdates = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    UpdatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new PlatformWaitlist
                {
                    Id = Guid.NewGuid(),
                    Email = "john.donor@example.com",
                    UserType = "Donor",
                    SchoolDistrict = null,
                    TeachingSubject = null,
                    InterestedInTEACHTokens = true,
                    SubscribeToUpdates = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-8),
                    UpdatedAt = DateTime.UtcNow.AddDays(-8)
                },
                new PlatformWaitlist
                {
                    Id = Guid.NewGuid(),
                    Email = "maria.rodriguez@lincoln.edu",
                    UserType = "Educator",
                    SchoolDistrict = "Houston ISD",
                    TeachingSubject = "Literacy",
                    InterestedInTEACHTokens = false,
                    SubscribeToUpdates = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new PlatformWaitlist
                {
                    Id = Guid.NewGuid(),
                    Email = "corporate@educationsupply.com",
                    UserType = "Partner",
                    SchoolDistrict = null,
                    TeachingSubject = null,
                    InterestedInTEACHTokens = true,
                    SubscribeToUpdates = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    UpdatedAt = DateTime.UtcNow.AddDays(-3)
                }
            };

            context.PlatformWaitlists.AddRange(waitlistEntries);
        }

        private static async Task SeedSampleAnalytics(TeachCrowdSaleDbContext context)
        {
            if (await context.PlatformAnalytics.AnyAsync()) return;

            var analyticsEntries = new List<PlatformAnalytics>();
            var random = new Random();
            var pages = new[] { "/", "/demo", "/vision", "/roadmap" };
            var actions = new[] { "page_view", "waitlist_signup", "demo_interaction", "project_click" };

            // Generate sample analytics for the past 30 days
            for (int i = 0; i < 500; i++)
            {
                var entry = new PlatformAnalytics
                {
                    Id = Guid.NewGuid(),
                    PageUrl = pages[random.Next(pages.Length)],
                    SessionId = $"sess_{Guid.NewGuid().ToString("N")[..8]}",
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
                    ReferrerUrl = i % 4 == 0 ? "https://teachtoken.io" : null,
                    UserIP = $"192.168.{random.Next(1, 255)}.{random.Next(1, 255)}",
                    TimeOnPage = random.Next(30, 600), // 30 seconds to 10 minutes
                    ConversionAction = i % 10 == 0 ? actions[random.Next(actions.Length)] : null,
                    ConversionData = i % 10 == 0 ? "{\"demo\": true}" : null,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 30)).AddHours(-random.Next(0, 24))
                };

                analyticsEntries.Add(entry);
            }

            context.PlatformAnalytics.AddRange(analyticsEntries);
        }
    }
}