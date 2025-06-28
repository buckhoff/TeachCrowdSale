using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models;
using TeachCrowdSale.Core.Models.Request;
using TeachCrowdSale.Core.Models.Response;
using TeachCrowdSale.Infrastructure.Data.Context;

namespace TeachCrowdSale.Infrastructure.Services
{
    /// <summary>
    /// Platform dashboard service implementation with mock data and API integration
    /// </summary>
    public class PlatformDashboardService : IPlatformDashboardService
    {
        private readonly TeachCrowdSaleDbContext _context;
        private readonly IHomeService _homeService; // Existing service for token data
        private readonly IMemoryCache _cache;
        private readonly ILogger<PlatformDashboardService> _logger;
        private readonly HttpClient _httpClient;

        // Cache keys and durations
        private const string CACHE_KEY_HOME_DATA = "platform_home_data";
        private const string CACHE_KEY_FEATURED_PROJECTS = "platform_featured_projects";
        private const string CACHE_KEY_PENCIL_DRIVE = "platform_pencil_drive";
        private const string CACHE_KEY_WAITLIST_STATS = "platform_waitlist_stats";

        private readonly TimeSpan _shortCacheDuration = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _mediumCacheDuration = TimeSpan.FromMinutes(15);
        private readonly TimeSpan _longCacheDuration = TimeSpan.FromMinutes(30);

        public PlatformDashboardService(
            TeachCrowdSaleDbContext context,
            IHomeService homeService,
            IMemoryCache cache,
            ILogger<PlatformDashboardService> logger,
            HttpClient httpClient)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _homeService = homeService ?? throw new ArgumentNullException(nameof(homeService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<PlatformHomeModel> GetHomePageDataAsync()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_HOME_DATA, out PlatformHomeModel? cachedData) && cachedData != null)
                {
                    return cachedData;
                }

                _logger.LogInformation("Building platform home page data");

                // Get real token sale data from existing service
                var tokenSaleData = await GetTokenSaleDataAsync();

                // Get platform-specific data
                var featuredProjects = await GetFeaturedProjectsAsync();
                var pencilDrive = await GetCurrentPencilDriveAsync();
                var waitlistStats = await GetWaitlistStatsAsync();

                var homeModel = new PlatformHomeModel
                {
                    Hero = GetHeroSectionData(),
                    TokenIntegration = MapTokenIntegrationData(tokenSaleData),
                    Vision = GetPlatformVisionData(),
                    FeaturedProjects = featuredProjects,
                    PencilDrive = pencilDrive,
                    WaitlistForm = GetWaitlistFormData(),
                    AnalyticsData = JsonSerializer.Serialize(new
                    {
                        tokenSaleActive = tokenSaleData?.IsSaleActive ?? false,
                        pencilDriveActive = pencilDrive?.IsActive ?? true,
                        featuredProjectCount = featuredProjects?.Count ?? 0,
                        waitlistTotal = waitlistStats?.TotalSignups ?? 0
                    })
                };

                _cache.Set(CACHE_KEY_HOME_DATA, homeModel, _shortCacheDuration);
                return homeModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting platform home page data");
                return GetFallbackHomeData();
            }
        }

        public async Task<List<DemoProjectModel>> GetFeaturedProjectsAsync()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_FEATURED_PROJECTS, out List<DemoProjectModel>? cachedProjects) && cachedProjects != null)
                {
                    return cachedProjects;
                }

                // Try to get from database first
                var dbProjects = await _context.DemoProjects
                    .Where(p => p.IsActive && p.IsFeatured)
                    .OrderBy(p => p.CreatedAt)
                    .Take(6)
                    .ToListAsync();

                List<DemoProjectModel> projects;

                if (dbProjects.Any())
                {
                    projects = dbProjects.Select(MapDemoProjectToModel).ToList();
                }
                else
                {
                    // Use mock data if no database projects
                    projects = GetMockFeaturedProjects();
                }

                _cache.Set(CACHE_KEY_FEATURED_PROJECTS, projects, _mediumCacheDuration);
                return projects;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured projects");
                return GetMockFeaturedProjects();
            }
        }

        public async Task<PencilDriveModel> GetCurrentPencilDriveAsync()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_PENCIL_DRIVE, out PencilDriveModel? cachedDrive) && cachedDrive != null)
                {
                    return cachedDrive;
                }

                var currentYear = DateTime.Now.Year;
                var dbDrive = await _context.PencilDrives
                    .Where(d => d.Year == currentYear && d.IsActive)
                    .FirstOrDefaultAsync();

                PencilDriveModel driveModel;

                if (dbDrive != null)
                {
                    driveModel = MapPencilDriveToModel(dbDrive);
                }
                else
                {
                    driveModel = GetMockPencilDrive();
                }

                _cache.Set(CACHE_KEY_PENCIL_DRIVE, driveModel, _mediumCacheDuration);
                return driveModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current pencil drive");
                return GetMockPencilDrive();
            }
        }

        public async Task<WaitlistSignupResponse> SubmitWaitlistSignupAsync(WaitlistSignupRequest request)
        {
            try
            {
                // Check if email already exists
                var existingEntry = await _context.PlatformWaitlists
                    .FirstOrDefaultAsync(w => w.Email.ToLower() == request.Email.ToLower());

                if (existingEntry != null)
                {
                    return new WaitlistSignupResponse
                    {
                        Success = false,
                        Message = "This email is already on our waitlist.",
                        ErrorCode = "EMAIL_EXISTS"
                    };
                }

                // Create new waitlist entry
                var waitlistEntry = new PlatformWaitlist
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email.ToLower().Trim(),
                    UserType = request.UserType,
                    SchoolDistrict = request.SchoolDistrict?.Trim(),
                    TeachingSubject = request.TeachingSubject?.Trim(),
                    InterestedInTEACHTokens = request.InterestedInTEACHTokens,
                    SubscribeToUpdates = request.SubscribeToUpdates,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.PlatformWaitlists.Add(waitlistEntry);
                await _context.SaveChangesAsync();

                // Track analytics event
                await TrackAnalyticsEventAsync("/waitlist", request.SessionId, "waitlist_signup", new
                {
                    userType = request.UserType,
                    interestedInTokens = request.InterestedInTEACHTokens,
                    referrer = request.ReferrerUrl
                });

                // Clear waitlist stats cache
                _cache.Remove(CACHE_KEY_WAITLIST_STATS);
                _cache.Remove(CACHE_KEY_HOME_DATA);

                _logger.LogInformation("Waitlist signup successful for {Email} as {UserType}", request.Email, request.UserType);

                return new WaitlistSignupResponse
                {
                    Success = true,
                    Message = "Thank you for joining our waitlist! We'll keep you updated on our progress.",
                    WaitlistId = waitlistEntry.Id,
                    SignupDate = waitlistEntry.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting waitlist signup for {Email}", request.Email);
                return new WaitlistSignupResponse
                {
                    Success = false,
                    Message = "An error occurred while processing your signup. Please try again.",
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        public async Task<WaitlistStats> GetWaitlistStatsAsync()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_WAITLIST_STATS, out WaitlistStats? cachedStats) && cachedStats != null)
                {
                    return cachedStats;
                }

                var stats = await _context.PlatformWaitlists
                    .GroupBy(w => 1)
                    .Select(g => new WaitlistStats
                    {
                        TotalSignups = g.Count(),
                        EducatorSignups = g.Count(w => w.UserType == "Educator"),
                        DonorSignups = g.Count(w => w.UserType == "Donor"),
                        PartnerSignups = g.Count(w => w.UserType == "Partner"),
                        TEACHTokenInterested = g.Count(w => w.InterestedInTEACHTokens),
                        LastSignupDate = g.Max(w => w.CreatedAt)
                    })
                    .FirstOrDefaultAsync();

                stats ??= new WaitlistStats();

                _cache.Set(CACHE_KEY_WAITLIST_STATS, stats, _longCacheDuration);
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting waitlist statistics");
                return new WaitlistStats();
            }
        }

        public async Task TrackAnalyticsEventAsync(string pageUrl, string? sessionId, string? action, object? data)
        {
            try
            {
                var analyticsEntry = new PlatformAnalytics
                {
                    Id = Guid.NewGuid(),
                    PageUrl = pageUrl,
                    SessionId = sessionId,
                    ConversionAction = action,
                    ConversionData = data != null ? JsonSerializer.Serialize(data) : null,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PlatformAnalytics.Add(analyticsEntry);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking analytics event for {PageUrl}", pageUrl);
                // Don't throw - analytics shouldn't break the application
            }
        }

        #region Private Helper Methods

        private async Task<TokenSaleMetrics?> GetTokenSaleDataAsync()
        {
            try
            {
                // Call existing HomeService to get real token sale data
                var homeData = await _homeService.GetLiveStatsAsync();

                return new TokenSaleMetrics
                {
                    TotalRaised = homeData?.TotalRaised ?? 0,
                    TokenPrice = homeData?.CurrentTierPrice ?? 0.001m,
                    TotalHolders = homeData?.HoldersCount ?? 0,
                    IsSaleActive = homeData?.IsPresaleActive ?? false,
                    SaleProgress = homeData?.CurrentTierProgress ?? 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token sale data from HomeService");
                return new TokenSaleMetrics
                {
                    TotalRaised = 245000,
                    TokenPrice = 0.001m,
                    TotalHolders = 1250,
                    IsSaleActive = true,
                    SaleProgress = 24.5m
                };
            }
        }

        private static HeroSectionModel GetHeroSectionData()
        {
            return new HeroSectionModel
            {
                MainHeadline = "Small Tokens, Big Impacts",
                SubHeadline = "Revolutionizing education funding with transparent, efficient, and community-driven support for teachers and students.",
                CtaText = "Join the Waitlist",
                SecondaryCtaText = "Explore Demo"
            };
        }

        private static TokenIntegrationModel MapTokenIntegrationData(TokenSaleMetrics? tokenData)
        {
            return new TokenIntegrationModel
            {
                CurrentPrice = tokenData?.TokenPrice ?? 0.001m,
                TotalRaised = tokenData?.TotalRaised ?? 245000,
                TotalHolders = tokenData?.TotalHolders ?? 1250,
                IsSaleActive = tokenData?.IsSaleActive ?? true,
                SaleEndDate = DateTime.UtcNow.AddMonths(8) // Concept: 8 months until TGE
            };
        }

        private static PlatformVisionModel GetPlatformVisionData()
        {
            return new PlatformVisionModel
            {
                VisionStatement = "Creating a world where every educator has the resources they need to inspire and educate the next generation."
            };
        }

        private static WaitlistFormModel GetWaitlistFormData()
        {
            return new WaitlistFormModel();
        }

        private static DemoProjectModel MapDemoProjectToModel(DemoProject project)
        {
            var deadline = project.Deadline;
            var daysRemaining = (deadline - DateTime.UtcNow).Days;

            return new DemoProjectModel
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                Category = project.Category,
                FundingGoal = project.FundingGoal,
                CurrentFunding = project.CurrentFunding,
                SchoolName = project.SchoolName,
                TeacherName = project.TeacherName,
                State = project.State,
                City = project.City,
                StudentsImpacted = project.StudentsImpacted,
                GradeLevel = project.GradeLevel,
                ImageUrl = project.ImageUrl,
                IsUrgent = project.IsUrgent,
                IsFeatured = project.IsFeatured,
                DaysRemaining = Math.Max(0, daysRemaining)
            };
        }

        private static PencilDriveModel MapPencilDriveToModel(PencilDrive drive)
        {
            var daysRemaining = (drive.EndDate - DateTime.UtcNow).Days;
            var tokenHolderPencils = Math.Min(500_000, (int)(drive.TokensRaised / 10 * 125)); // $10 TEACH = 125 pencils
            var totalPencils = drive.PartnerPencilsCommitted + drive.PlatformPencilsCommitted + tokenHolderPencils;

            return new PencilDriveModel
            {
                IsActive = drive.IsActive && DateTime.UtcNow <= drive.EndDate,
                Year = drive.Year,
                TokensRaised = drive.TokensRaised,
                PencilsSecured = totalPencils,
                TotalPencilGoal = drive.PencilGoal,
                PartnerName = drive.PartnerName ?? "Major Pencil Manufacturer",
                SchoolsApplied = drive.SchoolsApplied,
                SchoolsApproved = drive.SchoolsApproved,
                DaysRemaining = Math.Max(0, daysRemaining)
            };
        }

        private static List<DemoProjectModel> GetMockFeaturedProjects()
        {
            return new List<DemoProjectModel>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "STEM Lab Equipment for Rural School",
                    Description = "Help us create a modern science laboratory for our students in rural Montana. We need microscopes, chemistry sets, and basic lab equipment to give our 8th graders hands-on learning experiences.",
                    Category = "STEM",
                    FundingGoal = 2500,
                    CurrentFunding = 1890,
                    SchoolName = "Mountain View Middle School",
                    TeacherName = "Sarah Johnson",
                    State = "MT",
                    City = "Billings",
                    StudentsImpacted = 85,
                    GradeLevel = "6-8",
                    IsUrgent = false,
                    IsFeatured = true,
                    DaysRemaining = 23
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Books for Classroom Library",
                    Description = "Our students deserve access to diverse, engaging books. Help us stock our classroom library with 100 new books that reflect our students' experiences and interests.",
                    Category = "Literacy",
                    FundingGoal = 800,
                    CurrentFunding = 320,
                    SchoolName = "Lincoln Elementary",
                    TeacherName = "Maria Rodriguez",
                    State = "TX",
                    City = "Houston",
                    StudentsImpacted = 32,
                    GradeLevel = "3-5",
                    IsUrgent = true,
                    IsFeatured = true,
                    DaysRemaining = 8
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Art Supplies for Creative Expression",
                    Description = "Give our students the tools to express their creativity! We need paints, brushes, canvases, and other art supplies for our elementary art program.",
                    Category = "Arts",
                    FundingGoal = 1200,
                    CurrentFunding = 750,
                    SchoolName = "Riverside Elementary",
                    TeacherName = "David Kim",
                    State = "CA",
                    City = "San Diego",
                    StudentsImpacted = 120,
                    GradeLevel = "K-5",
                    IsUrgent = false,
                    IsFeatured = true,
                    DaysRemaining = 45
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Chromebooks for Digital Learning",
                    Description = "Bridge the digital divide by helping us provide Chromebooks for students who don't have access to technology at home. Each device will serve multiple students throughout the school year.",
                    Category = "Technology",
                    FundingGoal = 3500,
                    CurrentFunding = 1200,
                    SchoolName = "Jefferson High School",
                    TeacherName = "Amanda Chen",
                    State = "OH",
                    City = "Cleveland",
                    StudentsImpacted = 60,
                    GradeLevel = "9-12",
                    IsUrgent = false,
                    IsFeatured = true,
                    DaysRemaining = 67
                }
            };
        }

        private static PencilDriveModel GetMockPencilDrive()
        {
            return new PencilDriveModel
            {
                IsActive = true,
                Year = DateTime.Now.Year,
                TokensRaised = 4250, // Mock: $42,500 in TEACH tokens raised
                PencilsSecured = 1_531_250, // 500K partner + 500K platform + 531,250 from tokens
                TotalPencilGoal = 2_000_000,
                PartnerName = "Ticonderoga Pencil Company",
                SchoolsApplied = 147,
                SchoolsApproved = 89,
                DaysRemaining = 78
            };
        }

        private static PlatformHomeModel GetFallbackHomeData()
        {
            return new PlatformHomeModel
            {
                Hero = GetHeroSectionData(),
                TokenIntegration = new TokenIntegrationModel(),
                Vision = GetPlatformVisionData(),
                FeaturedProjects = GetMockFeaturedProjects(),
                PencilDrive = GetMockPencilDrive(),
                WaitlistForm = GetWaitlistFormData(),
                AnalyticsData = "{\"fallback\": true}"
            };
        }

        #endregion
    }
}
