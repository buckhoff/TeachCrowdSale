using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TeachCrowdSale.Core.Helper;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models;

namespace TeachCrowdSale.Web.Controllers
{
    /// <summary>
    /// Controller for the Platform Roadmap & Development section
    /// Follows the established TeachToken architecture patterns
    /// </summary>
    public class RoadmapController : Controller
    {
        private readonly IRoadmapDashboardService _roadmapService;
        private readonly ILogger<RoadmapController> _logger;
        private readonly IMemoryCache _cache;

        public RoadmapController(
            IRoadmapDashboardService roadmapService,
            ILogger<RoadmapController> logger,
            IMemoryCache cache)
        {
            _roadmapService = roadmapService;
            _logger = logger;
            _cache = cache;
        }

        #region Main Views

        /// <summary>
        /// Main roadmap dashboard page
        /// </summary>
        [HttpGet]
        [Route("roadmap")]
        [Route("roadmap/index")]
        [ResponseCache(Duration = 900, Location = ResponseCacheLocation.Client)] // 15 minutes
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading roadmap dashboard page");

                var roadmapData = await _roadmapService.GetRoadmapPageDataAsync();

                // Set page metadata
                ViewData["Title"] = "Platform Roadmap & Development - TeachToken";
                ViewData["Description"] = "Track TeachToken's development progress, upcoming milestones, GitHub activity, and platform roadmap. See real-time updates on our educational blockchain platform development.";
                ViewData["Keywords"] = "TeachToken roadmap, development progress, blockchain milestones, GitHub activity, platform updates, educational technology";

                // Add structured data for better SEO
                ViewBag.StructuredData = GetStructuredData(roadmapData);

                return View(roadmapData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading roadmap dashboard");

                // Create fallback model for error scenarios
                var errorModel = new RoadmapPageModel
                {
                    ProgressSummary = new ProgressSummaryModel
                    {
                        OverallProgress = 0,
                        CurrentPhase = "Unable to load data",
                        NextMilestone = "Please try again later."
                    },
                    LastUpdated = DateTime.UtcNow
                };

                ViewData["Title"] = "Roadmap - TeachToken";
                ViewData["Description"] = "TeachToken development roadmap and progress tracking.";
                ViewBag.ErrorMessage = "Unable to load roadmap data. Please try again later.";

                return View(errorModel);
            }
        }

        #endregion

        #region AJAX Endpoints

        /// <summary>
        /// Get milestones with optional filtering via AJAX
        /// </summary>
        [HttpGet]
        [Route("roadmap/api/milestones")]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)] // 5 minutes
        public async Task<IActionResult> GetMilestones(
            [FromQuery] string? status = null,
            [FromQuery] string? category = null,
            [FromQuery] string? priority = null)
        {
            try
            {
                _logger.LogDebug("Fetching milestones with filters - Status: {Status}, Category: {Category}, Priority: {Priority}",
                    status, category, priority);

                var milestones = await _roadmapService.GetFilteredMilestonesAsync(status, category);

                // Apply priority filter if specified (done in memory for simplicity)
                if (!string.IsNullOrWhiteSpace(priority))
                {
                    milestones = milestones.Where(m =>
                        string.Equals(m.Priority, priority, StringComparison.OrdinalIgnoreCase));
                }

                // Transform to simplified card models for grid display
                var milestoneCards = milestones.Select(m => new
                {
                    id = m.Id,
                    title = m.Title,
                    description = m.Description.Length > 120 ? m.Description[..120] + "..." : m.Description,
                    category = m.Category,
                    categoryIcon = DisplayHelpers.GetCategoryIcon(m.Category),
                    status = m.Status,
                    statusClass = m.StatusClass,
                    priority = m.Priority,
                    priorityClass = m.PriorityClass,
                    progressPercentage = m.ProgressPercentage,
                    progressText = m.ProgressText,
                    timeRemaining = m.TimeRemaining,
                    estimatedCompletion = m.EstimatedCompletionFormatted,
                    isCompleted = m.IsCompleted,
                    isOverdue = m.IsOverdue
                }).ToList();

                return Json(new { success = true, data = milestoneCards });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching filtered milestones");
                return Json(new
                {
                    success = false,
                    message = "Error loading milestones",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get milestone details by ID via AJAX
        /// </summary>
        [HttpGet]
        [Route("roadmap/api/milestone/{id:int}")]
        [ResponseCache(Duration = 180, Location = ResponseCacheLocation.Client)] // 3 minutes
        public async Task<IActionResult> GetMilestoneDetails([FromRoute] int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid milestone ID" });
                }

                var milestone = await _roadmapService.GetMilestoneDetailsAsync(id);

                if (milestone == null)
                {
                    return NotFound(new { success = false, message = "Milestone not found" });
                }

                // Return detailed milestone data
                var detailModel = new
                {
                    success = true,
                    data = new
                    {
                        milestone.Id,
                        milestone.Title,
                        milestone.Description,
                        milestone.Category,
                        categoryIcon = DisplayHelpers.GetCategoryIcon(milestone.Category),
                        milestone.Status,
                        milestone.StatusClass,
                        milestone.Priority,
                        milestone.PriorityClass,
                        milestone.ProgressPercentage,
                        milestone.StartDate,
                        milestone.EstimatedCompletionDate,
                        milestone.ActualCompletionDate,
                        milestone.DurationEstimate,
                        milestone.TimeRemaining,
                        milestone.CompletedTasksCount,
                        milestone.TotalTasksCount,
                        startDateFormatted = milestone.StartDateFormatted,
                        estimatedCompletionFormatted = milestone.EstimatedCompletionFormatted,
                        actualCompletionFormatted = milestone.ActualCompletionFormatted,
                        progressText = milestone.ProgressText,
                        milestone.IsCompleted,
                        milestone.IsInProgress,
                        milestone.IsOverdue,
                        tasks = milestone.Tasks?.Select(t => new
                        {
                            t.Id,
                            t.Title,
                            t.Status,
                            t.StatusClass,
                            t.Priority,
                            t.PriorityClass,
                            t.ProgressPercentage,
                            t.Assignee,
                            assigneeDisplay = t.AssigneeDisplay,
                            dueDateFormatted = t.DueDateFormatted,
                            t.IsCompleted,
                            t.IsOverdue,
                            timeTrackingText = t.TimeTrackingText
                        }),
                        updates = milestone.Updates?.OrderByDescending(u => u.CreatedAt).Take(5).Select(u => new
                        {
                            u.Id,
                            u.Title,
                            contentPreview = u.ContentPreview,
                            u.UpdateType,
                            updateTypeIcon = DisplayHelpers.GetUpdateTypeIcon(u.UpdateType),
                            u.Author,
                            authorDisplay = u.AuthorDisplay,
                            createdAtFormatted = u.CreatedAtFormatted,
                            u.HasTags,
                            u.HasAttachments
                        }),
                        dependencies = milestone.Dependencies?.Select(d => new
                        {
                            d.Id,
                            d.DependencyType,
                            dependencyTypeIcon = DisplayHelpers.GetDependencyTypeIcon(d.DependencyType),
                            dependencyText = d.DependencyText,
                            d.Description,
                            d.IsActive
                        })
                    }
                };

                return Json(detailModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching milestone details for ID: {MilestoneId}", id);
                return Json(new
                {
                    success = false,
                    message = "Error loading milestone details",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get progress data for charts via AJAX
        /// </summary>
        [HttpGet]
        [Route("roadmap/api/progress")]
        [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Client)] // 10 minutes
        public async Task<IActionResult> GetProgressData()
        {
            try
            {
                var progressSummary = await _roadmapService.GetProgressSummaryAsync();
                var githubStats = await _roadmapService.GetGitHubStatsAsync();
                var devStats = await _roadmapService.GetDevelopmentStatsAsync();

                var progressData = new
                {
                    success = true,
                    data = new
                    {
                        overall = new
                        {
                            progressSummary.OverallProgress,
                            progressSummary.TotalMilestones,
                            progressSummary.CompletedMilestones,
                            progressSummary.InProgressMilestones,
                            progressSummary.UpcomingMilestones,
                            progressSummary.TotalTasks,
                            progressSummary.CompletedTasks,
                            progressSummary.ActiveTasks,
                            progressSummary.OverdueTasks,
                            progressSummary.CurrentPhase,
                            progressSummary.NextMilestone,
                            estimatedCompletionFormatted = progressSummary.EstimatedCompletionFormatted,
                            completionRateText = progressSummary.CompletionRateText,
                            taskProgressText = progressSummary.TaskProgressText,
                            progressSummary.HasOverdueTasks,
                            healthStatus = progressSummary.HealthStatus
                        },
                        github = new
                        {
                            githubStats.TotalCommits,
                            githubStats.CommitsThisMonth,
                            githubStats.CommitsThisWeek,
                            githubStats.TotalContributors,
                            githubStats.ActiveContributors,
                            githubStats.OpenPullRequests,
                            githubStats.MergedPullRequests,
                            githubStats.OpenIssues,
                            githubStats.ClosedIssues,
                            lastCommitFormatted = githubStats.LastCommitFormatted,
                            lastCommitMessageDisplay = githubStats.LastCommitMessageDisplay,
                            lastCommitAuthorDisplay = githubStats.LastCommitAuthorDisplay,
                            activityLevel = githubStats.ActivityLevel,
                            commitFrequencyText = githubStats.CommitFrequencyText,
                            repositoryStatsText = githubStats.RepositoryStatsText,
                            githubStats.HasRecentActivity
                        },
                        development = new
                        {
                            linesOfCodeFormatted = devStats.LinesOfCodeFormatted,
                            devStats.FilesChanged,
                            devStats.CommitsThisWeek,
                            devStats.CommitsThisMonth,
                            devStats.ActiveBranches,
                            codeCoverageFormatted = devStats.CodeCoverageFormatted,
                            testStatusText = devStats.TestStatusText,
                            devStats.BuildStatus,
                            buildStatusClass = devStats.BuildStatusClass,
                            lastBuildFormatted = devStats.LastBuildFormatted,
                            topContributorsText = devStats.TopContributorsText,
                            activitySummary = devStats.ActivitySummary,
                            codeQualityStatus = devStats.CodeQualityStatus,
                            devStats.HasFailingTests,
                            devStats.IsBuildHealthy,
                            technicalDebtFormatted = devStats.TechnicalDebtFormatted,
                            lastDeploymentFormatted = devStats.LastDeploymentFormatted
                        }
                    }
                };

                return Json(progressData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching progress data");
                return Json(new
                {
                    success = false,
                    message = "Error loading progress data",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get recent releases via AJAX
        /// </summary>
        [HttpGet]
        [Route("roadmap/api/releases")]
        [ResponseCache(Duration = 1800, Location = ResponseCacheLocation.Client)] // 30 minutes
        public async Task<IActionResult> GetRecentReleases([FromQuery] int limit = 10)
        {
            try
            {
                var releases = await _roadmapService.GetRecentReleasesAsync(limit);

                var releaseData = releases.Select(r => new
                {
                    r.Id,
                    r.Version,
                    r.Title,
                    descriptionPreview = r.DescriptionPreview,
                    r.ReleaseDate,
                    releaseDateFormatted = r.ReleaseDateFormatted,
                    r.ReleaseType,
                    releaseTypeIcon = DisplayHelpers.GetReleaseTypeIcon(r.ReleaseType),
                    versionDisplay = r.VersionDisplay,
                    statusBadge = r.StatusBadge,
                    r.IsPreRelease,
                    r.IsDraft,
                    r.HasAssets,
                    r.HasGitHubLink,
                    r.HasDownload,
                    r.GitHubUrl,
                    r.DownloadUrl
                }).ToList();

                return Json(new { success = true, data = releaseData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recent releases");
                return Json(new
                {
                    success = false,
                    message = "Error loading releases",
                    error = ex.Message
                });
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Generate structured data for SEO
        /// </summary>
        private object GetStructuredData(RoadmapPageModel model)
        {
            return new
            {
                "@context" = "https://schema.org",
                "@type" = "WebPage",
                "name" = "TeachToken Platform Roadmap",
                "description" = "Development roadmap and progress tracking for the TeachToken educational blockchain platform",
                "url" = "https://teachtoken.io/roadmap",
                "mainEntity" = new
                {
                    "@type" = "SoftwareApplication",
                    "name" = "TeachToken Platform",
                    "applicationCategory" = "Educational Technology",
                    "operatingSystem" = "Web, Mobile",
                    "description" = "Blockchain-based educational platform for decentralized learning and teaching",
                    "softwareVersion" = model.RecentReleases?.FirstOrDefault()?.Version ?? "1.0.0",
                    "dateModified" = model.LastUpdated.ToString("yyyy-MM-dd"),
                    "offers" = new
                    {
                        "@type" = "Offer",
                        "price" = "0",
                        "priceCurrency" = "USD"
                    }
                },
                "breadcrumb" = new
                {
                    "@type" = "BreadcrumbList",
                    "itemListElement" = new[]
                    {
                        new
                        {
                            "@type" = "ListItem",
                            "position" = 1,
                            "name" = "Home",
                            "item" = "https://teachtoken.io"
                        },
                        new
                        {
                            "@type" = "ListItem",
                            "position" = 2,
                            "name" = "Roadmap",
                            "item" = "https://teachtoken.io/roadmap"
                        }
                    }
                }
            };
        }

        #endregion
    }
}