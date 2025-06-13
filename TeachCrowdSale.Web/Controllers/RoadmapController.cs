// TeachCrowdSale.Web/Controllers/RoadmapController.cs
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TeachCrowdSale.Core.Models;
using TeachCrowdSale.Core.Models.Response;
using TeachCrowdSale.Core.Interfaces.Services;

namespace TeachCrowdSale.Web.Controllers
{
    /// <summary>
    /// Web controller for roadmap and development dashboard operations
    /// </summary>
    [Route("roadmap")]
    public class RoadmapController : Controller
    {
        private readonly IRoadmapDashboardService _roadmapService;
        private readonly ILogger<RoadmapController> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public RoadmapController(
            IRoadmapDashboardService roadmapService,
            ILogger<RoadmapController> logger)
        {
            _roadmapService = roadmapService ?? throw new ArgumentNullException(nameof(roadmapService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        #region Main Views

        /// <summary>
        /// Main roadmap dashboard page
        /// </summary>
        [HttpGet("")]
        [HttpGet("index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading roadmap dashboard page");

                // Get comprehensive roadmap data
                var roadmapData = await _roadmapService.GetRoadmapPageDataAsync();
                var developmentStats = await _roadmapService.GetDevelopmentStatsAsync();
                var gitHubStats = await _roadmapService.GetGitHubStatsAsync();
                var filterOptions = await _roadmapService.GetFilterOptionsAsync();

                // Create page model
                var pageModel = new RoadmapPageModel
                {
                    PageTitle = "Platform Roadmap & Development - TeachToken",
                    PageDescription = "Track TeachToken development progress, milestones, and upcoming features. See our commitment to transparent development and education innovation.",
                    PageKeywords = "roadmap, development, milestones, progress, TeachToken, blockchain, education, platform",
                    ActiveMilestones = roadmapData.CurrentMilestones,
                    UpcomingMilestones = roadmapData.UpcomingMilestones,
                    CompletedMilestones = roadmapData.CompletedMilestones,
                    DevelopmentStats = developmentStats,
                    RecentUpdates = roadmapData.RecentUpdates,
                    Releases = roadmapData.Releases,
                    GitHubStats = gitHubStats,
                    FilterOptions = filterOptions,
                    LoadedAt = DateTime.UtcNow
                };

                // Create overview model
                pageModel.Overview = new RoadmapOverviewModel
                {
                    TotalMilestones = roadmapData.CurrentMilestones.Count + roadmapData.UpcomingMilestones.Count + roadmapData.CompletedMilestones.Count,
                    CompletedMilestones = roadmapData.CompletedMilestones.Count,
                    InProgressMilestones = roadmapData.CurrentMilestones.Count(m => m.Status == "In Progress"),
                    UpcomingMilestones = roadmapData.UpcomingMilestones.Count,
                    OnHoldMilestones = roadmapData.CurrentMilestones.Count(m => m.Status == "On Hold"),
                    OverallProgress = CalculateOverallProgress(roadmapData.CurrentMilestones.Concat(roadmapData.CompletedMilestones).ToList()),
                    EstimatedCompletionDate = GetEstimatedCompletionDate(roadmapData.UpcomingMilestones),
                    LastUpdateDate = roadmapData.RecentUpdates.FirstOrDefault()?.UpdateDate ?? DateTime.UtcNow.AddDays(-1),
                    IsOnTrack = CheckIfOnTrack(roadmapData.CurrentMilestones),
                    ProjectHealthStatus = GetProjectHealthStatus(roadmapData.CurrentMilestones, developmentStats),
                    ProjectHealthClass = GetProjectHealthClass(roadmapData.CurrentMilestones, developmentStats)
                };

                // Set calculated display properties
                pageModel.Overview.ProgressDisplayText = $"{pageModel.Overview.OverallProgress:F1}% Complete";
                pageModel.Overview.CompletionTimeframe = GetCompletionTimeframe(pageModel.Overview.EstimatedCompletionDate);
                pageModel.Overview.LastUpdateTimeAgo = GetTimeAgo(pageModel.Overview.LastUpdateDate);

                // Pass data to view
                ViewData["Title"] = pageModel.PageTitle;
                ViewData["Description"] = pageModel.PageDescription;
                ViewData["Keywords"] = pageModel.PageKeywords;

                ViewBag.RoadmapData = pageModel;
                ViewBag.JsonData = JsonSerializer.Serialize(pageModel, _jsonOptions);

                return View(pageModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading roadmap dashboard");

                // Return fallback view with error state
                var errorModel = new RoadmapPageModel
                {
                    HasErrors = true,
                    ErrorMessages = new List<string> { "Unable to load latest roadmap data. Please try again later." }
                };

                ViewData["Title"] = "Roadmap - TeachToken";
                ViewBag.RoadmapData = errorModel;

                return View(errorModel);
            }
        }

        #endregion

        #region AJAX Endpoints

        /// <summary>
        /// Get milestones with optional filtering via AJAX
        /// </summary>
        [HttpGet("GetMilestones")]
        [ResponseCache(Duration = 300)] // 5 minutes
        public async Task<IActionResult> GetMilestones(string? status = null, string? category = null, string? priority = null)
        {
            try
            {
                _logger.LogDebug("Fetching milestones with filters - Status: {Status}, Category: {Category}, Priority: {Priority}",
                    status, category, priority);

                var milestones = await _roadmapService.GetMilestonesAsync(status, category);

                // Apply priority filter if specified
                if (!string.IsNullOrWhiteSpace(priority))
                {
                    milestones = milestones.Where(m => m.Priority.Equals(priority, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // Convert to card models for grid display
                var milestoneCards = milestones.Select(ConvertToMilestoneCard).ToList();

                return Json(milestoneCards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching filtered milestones");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error fetching milestones",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get milestone details by ID via AJAX
        /// </summary>
        [HttpGet("GetMilestoneDetails/{id:int}")]
        [ResponseCache(Duration = 180)] // 3 minutes
        public async Task<IActionResult> GetMilestoneDetails([FromRoute] int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ErrorResponse { Message = "Invalid milestone ID" });
                }

                var milestone = await _roadmapService.GetMilestoneDetailsAsync(id);

                if (milestone == null)
                {
                    return NotFound(new ErrorResponse { Message = "Milestone not found" });
                }

                // Get milestone tasks and dependencies
                var tasks = await _roadmapService.GetMilestoneTasksAsync(id);
                var dependencies = await _roadmapService.GetMilestoneDependenciesAsync(id);

                var detailsModel = new
                {
                    milestone = milestone,
                    tasks = tasks,
                    dependencies = dependencies,
                    tasksCompleted = tasks.Count(t => t.Status == "Completed"),
                    totalTasks = tasks.Count,
                    hasBlockedTasks = tasks.Any(t => t.Status == "Blocked"),
                    recentActivity = tasks.Where(t => t.ActualCompletionDate >= DateTime.UtcNow.AddDays(-7)).Count()
                };

                return Json(detailsModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching milestone details for ID: {MilestoneId}", id);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error fetching milestone details",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get development progress data for charts via AJAX
        /// </summary>
        [HttpGet("GetProgressData")]
        [ResponseCache(Duration = 600)] // 10 minutes
        public async Task<IActionResult> GetProgressData()
        {
            try
            {
                var progressData = await _roadmapService.GetProgressHistoryAsync(0); // 0 for overall progress
                var timelineData = await _roadmapService.GetTimelineDataAsync();

                var chartData = new
                {
                    progressHistory = progressData,
                    timeline = timelineData,
                    generatedAt = DateTime.UtcNow
                };

                return Json(chartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching progress chart data");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error fetching progress data",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Search milestones by term via AJAX
        /// </summary>
        [HttpGet("Search")]
        [ResponseCache(Duration = 120)] // 2 minutes
        public async Task<IActionResult> Search(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                {
                    return BadRequest(new ErrorResponse { Message = "Search term is required" });
                }

                if (term.Length < 2)
                {
                    return BadRequest(new ErrorResponse { Message = "Search term must be at least 2 characters" });
                }

                var searchResults = await _roadmapService.SearchMilestonesAsync(term);
                var milestoneCards = searchResults.Select(ConvertToMilestoneCard).ToList();

                return Json(new
                {
                    term = term,
                    results = milestoneCards,
                    count = milestoneCards.Count,
                    searchedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching milestones for term: {SearchTerm}", term);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error performing search",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get development statistics via AJAX
        /// </summary>
        [HttpGet("GetDevelopmentStats")]
        [ResponseCache(Duration = 900)] // 15 minutes
        public async Task<IActionResult> GetDevelopmentStats()
        {
            try
            {
                var devStats = await _roadmapService.GetDevelopmentStatsAsync();
                var gitHubStats = await _roadmapService.GetGitHubStatsAsync();

                var statsModel = new
                {
                    development = devStats,
                    github = gitHubStats,
                    summary = new
                    {
                        totalCommits = devStats.TotalCommits,
                        activeContributors = devStats.ActiveContributors,
                        codeQuality = devStats.CodeQualityGrade,
                        activityLevel = devStats.ActivityLevel,
                        lastUpdate = Math.Max(
                            ((DateTimeOffset)devStats.LastCommit).ToUnixTimeSeconds(),
                            ((DateTimeOffset)gitHubStats.LastCommitDate).ToUnixTimeSeconds()
                        )
                    }
                };

                return Json(statsModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching development statistics");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error fetching development statistics",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get recent development updates via AJAX
        /// </summary>
        [HttpGet("GetRecentUpdates")]
        [ResponseCache(Duration = 300)] // 5 minutes
        public async Task<IActionResult> GetRecentUpdates(int count = 10)
        {
            try
            {
                if (count < 1 || count > 50)
                {
                    count = 10; // Default to reasonable limit
                }

                var updates = await _roadmapService.GetRecentUpdatesAsync(count);

                return Json(new
                {
                    updates = updates,
                    count = updates.Count,
                    lastUpdate = updates.FirstOrDefault()?.UpdateDate ?? DateTime.MinValue
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recent updates");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error fetching recent updates",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Export roadmap data via AJAX
        /// </summary>
        [HttpPost("Export")]
        public async Task<IActionResult> Export([FromBody] RoadmapExportRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid export request",
                        ValidationErrors = ModelState.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                        )
                    });
                }

                var milestones = await _roadmapService.GetMilestonesAsync(
                    request.FilterStatus,
                    request.FilterCategory
                );

                var exportData = new RoadmapExportModel
                {
                    ExportType = request.ExportType,
                    Milestones = milestones,
                    ExportDate = DateTime.UtcNow,
                    ExportedBy = "Web User", // Could be enhanced with user context
                    FileName = GenerateExportFileName(request.ExportType)
                };

                // Return appropriate format based on request
                return request.ExportType.ToLower() switch
                {
                    "csv" => GenerateCsvExport(exportData),
                    "json" => Json(exportData),
                    _ => BadRequest(new ErrorResponse { Message = "Unsupported export format" })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting roadmap data");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error exporting data",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Service health check via AJAX
        /// </summary>
        [HttpGet("HealthCheck")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                var isHealthy = await _roadmapService.CheckServiceHealthAsync();

                return Json(new
                {
                    status = isHealthy ? "healthy" : "degraded",
                    timestamp = DateTime.UtcNow,
                    service = "roadmap-dashboard"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking roadmap service health");
                return Json(new
                {
                    status = "unhealthy",
                    timestamp = DateTime.UtcNow,
                    service = "roadmap-dashboard",
                    error = "Service check failed"
                });
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Convert milestone to card model for grid display
        /// </summary>
        private MilestoneCardModel ConvertToMilestoneCard(MilestoneDisplayModel milestone)
        {
            return new MilestoneCardModel
            {
                Id = milestone.Id,
                Title = milestone.Title,
                Description = milestone.Description,
                ShortDescription = TruncateText(milestone.Description, 120),
                Category = milestone.Category,
                CategoryClass = GetCategoryClass(milestone.Category),
                Status = milestone.Status,
                StatusClass = milestone.StatusClass,
                StatusIcon = GetStatusIcon(milestone.Status),
                Priority = milestone.Priority,
                PriorityClass = milestone.PriorityClass,
                ProgressPercentage = milestone.ProgressPercentage,
                StartDate = milestone.StartDate,
                EstimatedCompletionDate = milestone.EstimatedCompletionDate,
                ActualCompletionDate = milestone.ActualCompletionDate,
                ProgressBarClass = GetProgressBarClass(milestone.ProgressPercentage, milestone.Status),
                TimelineText = GetTimelineText(milestone.StartDate, milestone.EstimatedCompletionDate, milestone.ActualCompletionDate),
                DurationText = milestone.DurationEstimate,
                ShowProgressBar = milestone.Status != "Completed",
                ShowDueDate = milestone.EstimatedCompletionDate.HasValue,
                IsOverdue = milestone.IsOverdue,
                IsBlocked = milestone.IsBlocked,
                IsNew = milestone.StartDate >= DateTime.UtcNow.AddDays(-14),
                HasRecentActivity = milestone.RecentUpdates.Any(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-7)),
                CompletedTasksCount = milestone.CompletedTasksCount,
                TotalTasksCount = milestone.TotalTasksCount,
                TasksSummary = $"{milestone.CompletedTasksCount}/{milestone.TotalTasksCount} tasks",
                GitHubIssueUrl = milestone.GitHubIssueUrl,
                DocumentationUrl = milestone.DocumentationUrl,
                HasExternalLinks = !string.IsNullOrWhiteSpace(milestone.GitHubIssueUrl) || !string.IsNullOrWhiteSpace(milestone.DocumentationUrl)
            };
        }

        /// <summary>
        /// Calculate overall progress across all milestones
        /// </summary>
        private decimal CalculateOverallProgress(List<MilestoneDisplayModel> milestones)
        {
            if (!milestones.Any()) return 0;

            var totalWeight = milestones.Count;
            var completedWeight = milestones.Where(m => m.Status == "Completed").Count();
            var inProgressWeight = milestones.Where(m => m.Status == "In Progress")
                .Sum(m => m.ProgressPercentage / 100m);

            return ((completedWeight + inProgressWeight) / totalWeight) * 100;
        }

        /// <summary>
        /// Get estimated completion date from upcoming milestones
        /// </summary>
        private DateTime? GetEstimatedCompletionDate(List<MilestoneDisplayModel> upcomingMilestones)
        {
            return upcomingMilestones
                .Where(m => m.EstimatedCompletionDate.HasValue)
                .Select(m => m.EstimatedCompletionDate!.Value)
                .DefaultIfEmpty()
                .Max();
        }

        /// <summary>
        /// Check if project is on track based on milestone status
        /// </summary>
        private bool CheckIfOnTrack(List<MilestoneDisplayModel> currentMilestones)
        {
            var overdueMilestones = currentMilestones.Count(m => m.IsOverdue);
            var blockedMilestones = currentMilestones.Count(m => m.IsBlocked);

            return overdueMilestones <= 1 && blockedMilestones == 0;
        }

        /// <summary>
        /// Get project health status
        /// </summary>
        private string GetProjectHealthStatus(List<MilestoneDisplayModel> milestones, DevelopmentStatsModel stats)
        {
            var overdue = milestones.Count(m => m.IsOverdue);
            var blocked = milestones.Count(m => m.IsBlocked);
            var recentActivity = stats.LastCommit >= DateTime.UtcNow.AddDays(-3);

            if (blocked > 0 || overdue > 2 || !recentActivity) return "At Risk";
            if (overdue > 0 || stats.OpenIssues > 20) return "Needs Attention";
            return "Healthy";
        }

        /// <summary>
        /// Get CSS class for project health status
        /// </summary>
        private string GetProjectHealthClass(List<MilestoneDisplayModel> milestones, DevelopmentStatsModel stats)
        {
            return GetProjectHealthStatus(milestones, stats).ToLower() switch
            {
                "healthy" => "health-good",
                "needs attention" => "health-warning",
                "at risk" => "health-danger",
                _ => "health-unknown"
            };
        }

        /// <summary>
        /// Get completion timeframe display text
        /// </summary>
        private string GetCompletionTimeframe(DateTime? completionDate)
        {
            if (!completionDate.HasValue) return "TBD";

            var timespan = completionDate.Value - DateTime.UtcNow;

            if (timespan.TotalDays < 0) return "Overdue";
            if (timespan.TotalDays < 30) return $"{(int)timespan.TotalDays} days";
            if (timespan.TotalDays < 365) return $"{(int)(timespan.TotalDays / 30)} months";

            return $"{(int)(timespan.TotalDays / 365)} years";
        }

        /// <summary>
        /// Get time ago display text
        /// </summary>
        private string GetTimeAgo(DateTime date)
        {
            var timespan = DateTime.UtcNow - date;

            if (timespan.TotalDays >= 1) return $"{(int)timespan.TotalDays} days ago";
            if (timespan.TotalHours >= 1) return $"{(int)timespan.TotalHours} hours ago";

            return "Recently";
        }

        /// <summary>
        /// Truncate text for card display
        /// </summary>
        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength - 3) + "...";
        }

        /// <summary>
        /// Get CSS class for category
        /// </summary>
        private string GetCategoryClass(string category)
        {
            return category.ToLower() switch
            {
                "blockchain" => "category-blockchain",
                "platform" => "category-platform",
                "mobile" => "category-mobile",
                "security" => "category-security",
                "integration" => "category-integration",
                _ => "category-default"
            };
        }

        /// <summary>
        /// Get icon for milestone status
        /// </summary>
        private string GetStatusIcon(string status)
        {
            return status.ToLower() switch
            {
                "completed" => "✅",
                "in progress" => "🔄",
                "testing" => "🧪",
                "planning" => "📋",
                "on hold" => "⏸️",
                "blocked" => "🚫",
                _ => "📌"
            };
        }

        /// <summary>
        /// Get progress bar CSS class
        /// </summary>
        private string GetProgressBarClass(decimal progress, string status)
        {
            if (status.ToLower() == "completed") return "progress-completed";
            if (progress >= 80) return "progress-high";
            if (progress >= 50) return "progress-medium";
            if (progress >= 25) return "progress-low";
            return "progress-minimal";
        }

        /// <summary>
        /// Get timeline display text
        /// </summary>
        private string GetTimelineText(DateTime? start, DateTime? estimated, DateTime? actual)
        {
            if (actual.HasValue) return $"Completed {GetTimeAgo(actual.Value)}";
            if (estimated.HasValue) return $"Due {GetCompletionTimeframe(estimated.Value)}";
            if (start.HasValue) return $"Started {GetTimeAgo(start.Value)}";
            return "Not scheduled";
        }

        /// <summary>
        /// Generate CSV export response
        /// </summary>
        private IActionResult GenerateCsvExport(RoadmapExportModel exportData)
        {
            var csv = new System.Text.StringBuilder();

            // CSV headers
            csv.AppendLine("ID,Title,Description,Category,Status,Priority,Progress,Start Date,Estimated Completion,Actual Completion");

            // CSV data
            foreach (var milestone in exportData.Milestones)
            {
                csv.AppendLine($"{milestone.Id}," +
                    $"\"{milestone.Title}\"," +
                    $"\"{milestone.Description}\"," +
                    $"{milestone.Category}," +
                    $"{milestone.Status}," +
                    $"{milestone.Priority}," +
                    $"{milestone.ProgressPercentage}%," +
                    $"{milestone.StartDate:yyyy-MM-dd}," +
                    $"{milestone.EstimatedCompletionDate:yyyy-MM-dd}," +
                    $"{milestone.ActualCompletionDate:yyyy-MM-dd}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", exportData.FileName);
        }

        /// <summary>
        /// Generate export filename
        /// </summary>
        private string GenerateExportFileName(string exportType)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            return $"teachtoken_roadmap_{timestamp}.{exportType.ToLower()}";
        }

        #endregion
    }

    /// <summary>
    /// Export request model
    /// </summary>
    public class RoadmapExportRequest
    {
        public string ExportType { get; set; } = "csv";
        public string? FilterStatus { get; set; }
        public string? FilterCategory { get; set; }
        public string? FilterPriority { get; set; }
    }
}