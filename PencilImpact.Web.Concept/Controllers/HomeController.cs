// PencilImpact.Web/Controllers/HomeController.cs
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models;
using TeachCrowdSale.Core.Models.Request;

namespace PencilImpact.Web.Concept.Controllers
{
    /// <summary>
    /// Home controller for PencilImpact platform
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IPlatformDashboardService _platformService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IPlatformDashboardService platformService,
            ILogger<HomeController> logger)
        {
            _platformService = platformService ?? throw new ArgumentNullException(nameof(platformService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Main landing page for PencilImpact
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var homeData = await _platformService.GetHomePageDataAsync();

                ViewData["Title"] = homeData.PageTitle;
                ViewData["Description"] = homeData.MetaDescription;

                // Track page view
                var sessionId = HttpContext.Session.Id;
                await _platformService.TrackAnalyticsEventAsync("/", sessionId, "page_view", new
                {
                    userAgent = Request.Headers.UserAgent.ToString(),
                    referrer = Request.Headers.Referer.ToString()
                });

                return View(homeData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page");

                // Return fallback data
                var fallbackData = new PlatformHomeModel();
                ViewData["Title"] = fallbackData.PageTitle;
                ViewData["Description"] = fallbackData.MetaDescription;

                return View(fallbackData);
            }
        }

        /// <summary>
        /// Platform vision and roadmap page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Vision()
        {
            try
            {
                var homeData = await _platformService.GetHomePageDataAsync();

                ViewData["Title"] = "Platform Vision - PencilImpact";
                ViewData["Description"] = "Discover how PencilImpact will revolutionize education funding with transparent, community-driven support powered by TEACH tokens.";

                var visionData = new PlatformVisionDetailModel
                {
                    VisionStatement = homeData.Vision.VisionStatement,
                    Metrics = homeData.Vision.Metrics,
                    Features = homeData.Vision.Features,
                    TokenIntegration = homeData.TokenIntegration,
                    Timeline = GetDevelopmentTimeline(),
                    Roadmap = GetPlatformRoadmap()
                };

                // Track page view
                var sessionId = HttpContext.Session.Id;
                await _platformService.TrackAnalyticsEventAsync("/vision", sessionId, "page_view", null);

                return View(visionData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vision page");
                return View(new PlatformVisionDetailModel());
            }
        }

        /// <summary>
        /// Interactive platform demo
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Demo()
        {
            try
            {
                var featuredProjects = await _platformService.GetFeaturedProjectsAsync();
                var pencilDrive = await _platformService.GetCurrentPencilDriveAsync();

                ViewData["Title"] = "Platform Demo - PencilImpact";
                ViewData["Description"] = "Experience how PencilImpact will revolutionize education funding with this interactive demonstration.";

                var demoData = new PlatformDemoModel
                {
                    FeaturedProjects = featuredProjects,
                    PencilDrive = pencilDrive,
                    DemoFlow = GetDemoFlowSteps(),
                    InteractiveFeatures = GetInteractiveFeatures()
                };

                // Serialize data for JavaScript
                ViewBag.JsonData = JsonSerializer.Serialize(new
                {
                    demoProjects = featuredProjects,
                    pencilDrive = pencilDrive,
                    demoFlow = demoData.DemoFlow,
                    features = demoData.InteractiveFeatures
                });

                // Track page view
                var sessionId = HttpContext.Session.Id;
                await _platformService.TrackAnalyticsEventAsync("/demo", sessionId, "page_view", null);

                return View(demoData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading demo page");
                return View(new PlatformDemoModel());
            }
        }

        /// <summary>
        /// Development roadmap page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Roadmap()
        {
            try
            {
                ViewData["Title"] = "Development Roadmap - PencilImpact";
                ViewData["Description"] = "Follow our journey from concept to full platform launch. See milestones, timelines, and how TEACH token holders can participate.";

                var roadmapData = new PlatformRoadmapModel
                {
                    CurrentPhase = "Concept Site",
                    Phases = GetDevelopmentPhases(),
                    Milestones = GetKeyMilestones(),
                    TokenIntegration = GetTokenIntegrationRoadmap()
                };

                // Track page view
                var sessionId = HttpContext.Session.Id;
                await _platformService.TrackAnalyticsEventAsync("/roadmap", sessionId, "page_view", null);

                return View(roadmapData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading roadmap page");
                return View(new PlatformRoadmapModel());
            }
        }

        /// <summary>
        /// AJAX endpoint for waitlist signup
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SubmitWaitlist([FromBody] WaitlistSignupRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Please check your information and try again.", errors = ModelState });
            }

            try
            {
                // Add session tracking
                request.SessionId = HttpContext.Session.Id;
                request.ReferrerUrl = Request.Headers.Referer.ToString();

                var result = await _platformService.SubmitWaitlistSignupAsync(request);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        waitlistId = result.WaitlistId
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = result.Message,
                        errorCode = result.ErrorCode
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting waitlist signup");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred. Please try again later."
                });
            }
        }

        /// <summary>
        /// AJAX endpoint for analytics tracking
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> TrackAnalytics([FromBody] AnalyticsTrackingRequest request)
        {
            try
            {
                await _platformService.TrackAnalyticsEventAsync(
                    request.PageUrl,
                    HttpContext.Session.Id,
                    request.Action,
                    request.Data
                );

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking analytics");
                return Ok(new { success = false }); // Don't break user experience for analytics failures
            }
        }

        #region Private Helper Methods

        private static List<DemoFlowStep> GetDemoFlowSteps()
        {
            return new List<DemoFlowStep>
            {
                new()
                {
                    Title = "Browse Projects",
                    Description = "Discover education projects from verified teachers across the country",
                    Icon = "🔍",
                    Features = new List<string> { "Filter by subject, location, urgency", "See real impact metrics", "Read teacher stories" }
                },
                new()
                {
                    Title = "Choose Impact",
                    Description = "Select projects that align with your values and interests",
                    Icon = "❤️",
                    Features = new List<string> { "Smart matching algorithm", "Impact calculators", "Community recommendations" }
                },
                new()
                {
                    Title = "Donate with TEACH",
                    Description = "Use TEACH tokens for enhanced benefits and reduced fees",
                    Icon = "🪙",
                    Features = new List<string> { "Lower platform fees", "Governance voting rights", "Staking rewards" }
                },
                new()
                {
                    Title = "Track Impact",
                    Description = "See real-time updates on how your donation helps students",
                    Icon = "📊",
                    Features = new List<string> { "Photo updates from teachers", "Student achievement metrics", "Long-term impact tracking" }
                }
            };
        }

        private static List<InteractiveFeature> GetInteractiveFeatures()
        {
            return new List<InteractiveFeature>
            {
                new() { Name = "Project Browser", Description = "Browse and filter demo projects", IsInteractive = true },
                new() { Name = "Donation Calculator", Description = "See impact of different donation amounts", IsInteractive = true },
                new() { Name = "TEACH Token Benefits", Description = "Compare benefits of token vs. traditional donations", IsInteractive = true },
                new() { Name = "Pencil Drive Tracker", Description = "Live progress tracking for community drives", IsInteractive = true }
            };
        }

        private static List<DevelopmentPhase> GetDevelopmentPhases()
        {
            return new List<DevelopmentPhase>
            {
                new()
                {
                    Name = "Concept Site",
                    Description = "Professional platform vision and community building",
                    Timeline = "June - August 2025",
                    Status = "In Progress",
                    Deliverables = new List<string> { "Landing page", "Waitlist system", "Demo showcase", "Token integration preview" }
                },
                new()
                {
                    Name = "MVP Platform",
                    Description = "Functional education funding platform with basic features",
                    Timeline = "September 2025 - June 2026",
                    Status = "Planned",
                    Deliverables = new List<string> { "School verification", "Project creation", "Payment processing", "Impact tracking" }
                },
                new()
                {
                    Name = "Full Platform",
                    Description = "AI-powered comprehensive ecosystem with advanced features",
                    Timeline = "July 2026 - December 2027",
                    Status = "Planned",
                    Deliverables = new List<string> { "AI matching", "Advanced analytics", "Mobile app", "International expansion" }
                }
            };
        }

        private static List<RoadmapMilestone> GetKeyMilestones()
        {
            return new List<RoadmapMilestone>
            {
                new() { Title = "Concept Site Launch", Date = "July 2025", Status = "In Progress", Description = "Public launch of platform vision" },
                new() { Title = "1,000 Educator Signups", Date = "August 2025", Status = "Target", Description = "Community milestone for MVP validation" },
                new() { Title = "TEACH Token TGE", Date = "October 2025", Status = "Planned", Description = "Token generation event and public sale" },
                new() { Title = "MVP Beta Launch", Date = "January 2026", Status = "Planned", Description = "Limited beta with partner schools" },
                new() { Title = "First Pencil Drive", Date = "March 2026", Status = "Planned", Description = "Community-wide pencil distribution event" },
                new() { Title = "MVP Public Launch", Date = "June 2026", Status = "Planned", Description = "Full public platform launch" }
            };
        }

        private static List<string> GetDevelopmentTimeline()
        {
            return new List<string>
            {
                "Q3 2025: Concept Site & Community Building",
                "Q4 2025: TEACH Token Launch & Partnership Development",
                "Q1 2026: MVP Development & Beta Testing",
                "Q2 2026: MVP Launch & First Pencil Drive",
                "Q3-Q4 2026: Platform Optimization & Feature Enhancement",
                "2027+: Full Platform with AI & International Expansion"
            };
        }

        private static List<string> GetPlatformRoadmap()
        {
            return new List<string>
            {
                "Phase 1: Vision & Validation (Current)",
                "Phase 2: Basic Platform & Token Integration",
                "Phase 3: AI Enhancement & Scaling",
                "Phase 4: Global Education Impact"
            };
        }

        private static List<TokenIntegrationPhase> GetTokenIntegrationRoadmap()
        {
            return new List<TokenIntegrationPhase>
            {
                new() { Phase = "Launch", Features = new List<string> { "Payment processing", "Basic staking", "Governance voting" }, Timeline = "Q4 2025" },
                new() { Phase = "Enhancement", Features = new List<string> { "Advanced staking", "NFT rewards", "DAO governance" }, Timeline = "Q2 2026" },
                new() { Phase = "Ecosystem", Features = new List<string> { "DeFi integration", "Cross-platform utility", "International support" }, Timeline = "2027+" }
            };
        }

        #endregion
    }

    #region Supporting Models for Views

    public class PlatformVisionDetailModel
    {
        public string VisionStatement { get; set; } = string.Empty;
        public List<VisionMetric> Metrics { get; set; } = new();
        public List<PlatformFeature> Features { get; set; } = new();
        public TokenIntegrationModel TokenIntegration { get; set; } = new();
        public List<string> Timeline { get; set; } = new();
        public List<string> Roadmap { get; set; } = new();
    }

    public class PlatformDemoModel
    {
        public List<DemoProjectModel> FeaturedProjects { get; set; } = new();
        public PencilDriveModel PencilDrive { get; set; } = new();
        public List<DemoFlowStep> DemoFlow { get; set; } = new();
        public List<InteractiveFeature> InteractiveFeatures { get; set; } = new();
    }

    public class DemoFlowStep
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public List<string> Features { get; set; } = new();
    }

    public class InteractiveFeature
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsInteractive { get; set; }
    }

    public class PlatformRoadmapModel
    {
        public string CurrentPhase { get; set; } = string.Empty;
        public List<DevelopmentPhase> Phases { get; set; } = new();
        public List<RoadmapMilestone> Milestones { get; set; } = new();
        public List<TokenIntegrationPhase> TokenIntegration { get; set; } = new();
    }

    public class DevelopmentPhase
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Timeline { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<string> Deliverables { get; set; } = new();
    }

    public class RoadmapMilestone
    {
        public string Title { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class TokenIntegrationPhase
    {
        public string Phase { get; set; } = string.Empty;
        public List<string> Features { get; set; } = new();
        public string Timeline { get; set; } = string.Empty;
    }

    public class AnalyticsTrackingRequest
    {
        public string PageUrl { get; set; } = string.Empty;
        public string? Action { get; set; }
        public object? Data { get; set; }
    }

    #endregion
}