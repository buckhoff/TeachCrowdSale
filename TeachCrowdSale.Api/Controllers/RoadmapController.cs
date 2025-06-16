using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TeachCrowdSale.Core.Models.Request;
using TeachCrowdSale.Core.Models.Response;
using TeachCrowdSale.Infrastructure.Services;
using TeachCrowdSale.Core.Data.Entities.Enums;
using TeachCrowdSale.Core.Data.Enum;
using TeachCrowdSale.Core.Interfaces;

namespace TeachCrowdSale.API.Controllers
{
    /// <summary>
    /// API Controller for Roadmap data - Uses Request/Response models only
    /// This controller should NOT be used by web controllers directly
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class RoadmapController : ControllerBase
    {
        private readonly IRoadmapService _roadmapService;
        private readonly ILogger<RoadmapController> _logger;
        private readonly IMemoryCache _cache;

        public RoadmapController(
            IRoadmapService roadmapService,
            ILogger<RoadmapController> logger,
            IMemoryCache cache)
        {
            _roadmapService = roadmapService;
            _logger = logger;
            _cache = cache;
        }

        #region GET Endpoints

        /// <summary>
        /// Get complete roadmap data for dashboard
        /// </summary>
        [HttpGet("data")]
        [ResponseCache(Duration = 900)] // 15 minutes
        public async Task<ActionResult<RoadmapResponse>> GetRoadmapData()
        {
            try
            {
                _logger.LogInformation("Fetching complete roadmap data");

                var progressSummary = await _roadmapService.GetProgressSummaryAsync();
                var activeMilestones = await _roadmapService.GetMilestonesByStatusAsync(MilestoneStatus.InProgress);
                var upcomingMilestones = await _roadmapService.GetMilestonesByStatusAsync(MilestoneStatus.Planning);
                var completedMilestones = await _roadmapService.GetMilestonesByStatusAsync(MilestoneStatus.Completed);
                var recentReleases = await _roadmapService.GetRecentReleasesAsync(5);
                var githubStats = await _roadmapService.GetGitHubStatsAsync();
                var devStats = await _roadmapService.GetDevelopmentStatsAsync();

                var response = new RoadmapResponse
                {
                    ProgressSummary = MapToProgressSummaryResponse(progressSummary),
                    ActiveMilestones = activeMilestones.Select(MapToMilestoneResponse).ToList(),
                    UpcomingMilestones = upcomingMilestones.Select(MapToMilestoneResponse).ToList(),
                    CompletedMilestones = completedMilestones.Select(MapToMilestoneResponse).ToList(),
                    RecentReleases = recentReleases.Select(MapToReleaseResponse).ToList(),
                    GitHubStats = MapToGitHubStatsResponse(githubStats),
                    DevelopmentStats = MapToDevelopmentStatsResponse(devStats),
                    LastUpdated = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching roadmap data");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Internal server error while fetching roadmap data",
                    Details = ex.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get milestone by ID with full details
        /// </summary>
        [HttpGet("milestones/{id:int}")]
        [ResponseCache(Duration = 300)] // 5 minutes
        public async Task<ActionResult<MilestoneResponse>> GetMilestone(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid milestone ID",
                        ValidationErrors = new List<string> { "Milestone ID must be greater than 0" }
                    });
                }

                var milestone = await _roadmapService.GetMilestoneWithDetailsAsync(id);
                if (milestone == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Message = $"Milestone with ID {id} not found"
                    });
                }

                var response = MapToMilestoneResponse(milestone);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching milestone {MilestoneId}", id);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Internal server error while fetching milestone",
                    Details = ex.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get milestones with optional filtering
        /// </summary>
        [HttpGet("milestones")]
        [ResponseCache(Duration = 600)] // 10 minutes
        public async Task<ActionResult<PaginatedResponse<MilestoneResponse>>> GetMilestones(
            [FromQuery] string? status = null,
            [FromQuery] string? category = null,
            [FromQuery] string? priority = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                _logger.LogInformation("Fetching milestones - Status: {Status}, Category: {Category}, Priority: {Priority}, Page: {Page}",
                    status, category, priority, page);

                var milestones = await _roadmapService.GetFilteredMilestonesAsync(status, category, priority);
                var totalCount = milestones.Count();

                // Apply pagination
                var pagedMilestones = milestones
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(MapToMilestoneResponse)
                    .ToList();

                var response = new PaginatedResponse<MilestoneResponse>
                {
                    Data = pagedMilestones,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    HasPreviousPage = page > 1,
                    HasNextPage = page < Math.Ceiling((double)totalCount / pageSize)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching filtered milestones");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Internal server error while fetching milestones",
                    Details = ex.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get progress summary
        /// </summary>
        [HttpGet("progress")]
        [ResponseCache(Duration = 900)] // 15 minutes
        public async Task<ActionResult<ProgressSummaryResponse>> GetProgressSummary()
        {
            try
            {
                var progressSummary = await _roadmapService.GetProgressSummaryAsync();
                var response = MapToProgressSummaryResponse(progressSummary);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching progress summary");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Internal server error while fetching progress summary",
                    Details = ex.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get GitHub statistics
        /// </summary>
        [HttpGet("github-stats")]
        [ResponseCache(Duration = 1800)] // 30 minutes
        public async Task<ActionResult<GitHubStatsResponse>> GetGitHubStats()
        {
            try
            {
                var githubStats = await _roadmapService.GetGitHubStatsAsync();
                var response = MapToGitHubStatsResponse(githubStats);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching GitHub stats");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Internal server error while fetching GitHub statistics",
                    Details = ex.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get development statistics
        /// </summary>
        [HttpGet("development-stats")]
        [ResponseCache(Duration = 1800)] // 30 minutes
        public async Task<ActionResult<DevelopmentStatsResponse>> GetDevelopmentStats()
        {
            try
            {
                var devStats = await _roadmapService.GetDevelopmentStatsAsync();
                var response = MapToDevelopmentStatsResponse(devStats);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching development stats");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Internal server error while fetching development statistics",
                    Details = ex.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get recent releases
        /// </summary>
        [HttpGet("releases")]
        [ResponseCache(Duration = 3600)] // 1 hour
        public async Task<ActionResult<IEnumerable<ReleaseResponse>>> GetRecentReleases([FromQuery] int limit = 10)
        {
            try
            {
                if (limit < 1 || limit > 50) limit = 10;

                var releases = await _roadmapService.GetRecentReleasesAsync(limit);
                var response = releases.Select(MapToReleaseResponse);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recent releases");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Internal server error while fetching releases",
                    Details = ex.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get tasks for a specific milestone
        /// </summary>
        [HttpGet("milestones/{milestoneId:int}/tasks")]
        [ResponseCache(Duration = 300)] // 5 minutes
        public async Task<ActionResult<IEnumerable<TaskResponse>>> GetMilestoneTasks(int milestoneId)
        {
            try
            {
                if (milestoneId <= 0)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid milestone ID",
                        ValidationErrors = new List<string> { "Milestone ID must be greater than 0" }
                    });
                }

                var tasks = await _roadmapService.GetTasksByMilestoneAsync(milestoneId);
                var response = tasks.Select(MapToTaskResponse);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tasks for milestone {MilestoneId}", milestoneId);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Internal server error while fetching milestone tasks",
                    Details = ex.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get updates for a specific milestone
        /// </summary>
        [HttpGet("milestones/{milestoneId:int}/updates")]
        [ResponseCache(Duration = 300)] // 5 minutes
        public async Task<ActionResult<IEnumerable<UpdateResponse>>> GetMilestoneUpdates(int milestoneId)
        {
            try
            {
                if (milestoneId <= 0)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid milestone ID",
                        ValidationErrors = new List<string> { "Milestone ID must be greater than 0" }
                    });
                }

                var updates = await _roadmapService.GetUpdatesByMilestoneAsync(milestoneId);
                var response = updates.Select(MapToUpdateResponse);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching updates for milestone {MilestoneId}", milestoneId);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Internal server error while fetching milestone updates",
                    Details = ex.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        #endregion

        #region POST Endpoints (for future admin functionality)

        /// <summary>
        /// Create a new milestone
        /// </summary>
        [HttpPost("milestones")]
        public async Task<ActionResult<SuccessResponse<MilestoneResponse>>> CreateMilestone([FromBody] MilestoneRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid milestone data",
                        ValidationErrors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var milestone = await _roadmapService.CreateMilestoneAsync(request);
                var response = MapToMilestoneResponse(milestone);

                return CreatedAtAction(
                    nameof(GetMilestone),
                    new { id = milestone.Id },
                    new SuccessResponse<MilestoneResponse>
                    {
                        Message = "Milestone created successfully",
                        Data = response
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating milestone");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Internal server error while creating milestone",
                    Details = ex.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Create a new task for a milestone
        /// </summary>
        [HttpPost("milestones/{milestoneId:int}/tasks")]
        public async Task<ActionResult<SuccessResponse<TaskResponse>>> CreateTask(int milestoneId, [FromBody] TaskRequest request)
        {
            try
            {
                if (milestoneId <= 0)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid milestone ID",
                        ValidationErrors = new List<string> { "Milestone ID must be greater than 0" }
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid task data",
                        ValidationErrors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                // Ensure the milestone ID in request matches the route parameter
                request.MilestoneId = milestoneId;

                var task = await _roadmapService.CreateTaskAsync(request);
                var response = MapToTaskResponse(task);

                return CreatedAtAction(
                    nameof(GetMilestoneTasks),
                    new { milestoneId = milestoneId },
                    new SuccessResponse<TaskResponse>
                    {
                        Message = "Task created successfully",
                        Data = response
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task for milestone {MilestoneId}", milestoneId);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Internal server error while creating task",
                    Details = ex.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Create a new update for a milestone
        /// </summary>
        [HttpPost("milestones/{milestoneId:int}/updates")]
        public async Task<ActionResult<SuccessResponse<UpdateResponse>>> CreateUpdate(int milestoneId, [FromBody] UpdateRequest request)
        {
            try
            {
                if (milestoneId <= 0)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid milestone ID",
                        ValidationErrors = new List<string> { "Milestone ID must be greater than 0" }
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid update data",
                        ValidationErrors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                // Ensure the milestone ID in request matches the route parameter
                request.MilestoneId = milestoneId;

                var update = await _roadmapService.CreateUpdateAsync(request);
                var response = MapToUpdateResponse(update);

                return CreatedAtAction(
                    nameof(GetMilestoneUpdates),
                    new { milestoneId = milestoneId },
                    new SuccessResponse<UpdateResponse>
                    {
                        Message = "Update created successfully",
                        Data = response
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating update for milestone {MilestoneId}", milestoneId);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Internal server error while creating update",
                    Details = ex.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        #endregion

        #region PUT Endpoints (for future admin functionality)

        /// <summary>
        /// Update an existing milestone
        /// </summary>
        [HttpPut("milestones/{id:int}")]
        public async Task<ActionResult<SuccessResponse<MilestoneResponse>>> UpdateMilestone(int id, [FromBody] MilestoneRequest request)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid milestone ID",
                        ValidationErrors = new List<string> { "Milestone ID must be greater than 0" }
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid milestone data",
                        ValidationErrors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var milestone = await _roadmapService.UpdateMilestoneAsync(id, request);
                if (milestone == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Message = $"Milestone with ID {id} not found"
                    });
                }

                var response = MapToMilestoneResponse(milestone);

                return Ok(new SuccessResponse<MilestoneResponse>
                {
                    Message = "Milestone updated successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating milestone {MilestoneId}", id);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Internal server error while updating milestone",
                    Details = ex.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        #endregion

        #region Entity to Response Mapping Methods

        private MilestoneResponse MapToMilestoneResponse(dynamic milestone)
        {
            return new MilestoneResponse
            {
                Id = milestone.Id,
                Title = milestone.Title,
                Description = milestone.Description,
                Category = milestone.Category,
                Status = milestone.Status,
                Priority = milestone.Priority,
                ProgressPercentage = milestone.ProgressPercentage,
                StartDate = milestone.StartDate,
                EstimatedCompletionDate = milestone.EstimatedCompletionDate,
                ActualCompletionDate = milestone.ActualCompletionDate,
                DurationEstimate = milestone.DurationEstimate,
                CompletedTasksCount = milestone.CompletedTasksCount,
                TotalTasksCount = milestone.TotalTasksCount,
                CreatedAt = milestone.CreatedAt,
                UpdatedAt = milestone.UpdatedAt,
                Tasks = milestone.Tasks?.Select(MapToTaskResponse).ToList(),
                Dependencies = milestone.Dependencies?.Select(MapToDependencyResponse).ToList(),
                Updates = milestone.Updates?.Select(MapToUpdateResponse).ToList()
            };
        }

        private TaskResponse MapToTaskResponse(dynamic task)
        {
            return new TaskResponse
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                ProgressPercentage = task.ProgressPercentage,
                StartDate = task.StartDate,
                DueDate = task.DueDate,
                CompletionDate = task.CompletionDate,
                MilestoneId = task.MilestoneId,
                Assignee = task.Assignee,
                EstimatedHours = task.EstimatedHours,
                ActualHours = task.ActualHours,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                MilestoneTitle = task.Milestone?.Title ?? ""
            };
        }

        private UpdateResponse MapToUpdateResponse(dynamic update)
        {
            return new UpdateResponse
            {
                Id = update.Id,
                Title = update.Title,
                Content = update.Content,
                UpdateType = update.UpdateType,
                MilestoneId = update.MilestoneId,
                Author = update.Author,
                Tags = update.Tags,
                Attachments = update.Attachments,
                CreatedAt = update.CreatedAt,
                UpdatedAt = update.UpdatedAt,
                MilestoneTitle = update.Milestone?.Title ?? ""
            };
        }

        private ReleaseResponse MapToReleaseResponse(dynamic release)
        {
            return new ReleaseResponse
            {
                Id = release.Id,
                Version = release.Version,
                Title = release.Title,
                Description = release.Description,
                ReleaseDate = release.ReleaseDate,
                ReleaseType = release.ReleaseType,
                IsPreRelease = release.IsPreRelease,
                IsDraft = release.IsDraft,
                TagName = release.TagName,
                GitHubUrl = release.GitHubUrl,
                DownloadUrl = release.DownloadUrl,
                ReleaseNotes = release.ReleaseNotes,
                Assets = release.Assets,
                CreatedAt = release.CreatedAt,
                UpdatedAt = release.UpdatedAt
            };
        }

        private DependencyResponse MapToDependencyResponse(dynamic dependency)
        {
            return new DependencyResponse
            {
                Id = dependency.Id,
                MilestoneId = dependency.MilestoneId,
                DependentMilestoneId = dependency.DependentMilestoneId,
                DependencyType = dependency.DependencyType,
                Description = dependency.Description,
                IsActive = dependency.IsActive,
                CreatedAt = dependency.CreatedAt,
                UpdatedAt = dependency.UpdatedAt,
                MilestoneTitle = dependency.Milestone?.Title ?? "",
                DependentMilestoneTitle = dependency.DependentMilestone?.Title ?? ""
            };
        }

        private ProgressSummaryResponse MapToProgressSummaryResponse(dynamic progressSummary)
        {
            return new ProgressSummaryResponse
            {
                OverallProgress = progressSummary.OverallProgress,
                TotalMilestones = progressSummary.TotalMilestones,
                CompletedMilestones = progressSummary.CompletedMilestones,
                InProgressMilestones = progressSummary.InProgressMilestones,
                UpcomingMilestones = progressSummary.UpcomingMilestones,
                TotalTasks = progressSummary.TotalTasks,
                CompletedTasks = progressSummary.CompletedTasks,
                ActiveTasks = progressSummary.ActiveTasks,
                OverdueTasks = progressSummary.OverdueTasks,
                AverageCompletionTime = progressSummary.AverageCompletionTime,
                CurrentPhase = progressSummary.CurrentPhase,
                NextMilestone = progressSummary.NextMilestone,
                EstimatedProjectCompletion = progressSummary.EstimatedProjectCompletion
            };
        }

        private GitHubStatsResponse MapToGitHubStatsResponse(dynamic githubStats)
        {
            return new GitHubStatsResponse
            {
                TotalCommits = githubStats.TotalCommits,
                CommitsThisMonth = githubStats.CommitsThisMonth,
                CommitsThisWeek = githubStats.CommitsThisWeek,
                TotalContributors = githubStats.TotalContributors,
                ActiveContributors = githubStats.ActiveContributors,
                TotalPullRequests = githubStats.TotalPullRequests,
                OpenPullRequests = githubStats.OpenPullRequests,
                MergedPullRequests = githubStats.MergedPullRequests,
                TotalIssues = githubStats.TotalIssues,
                OpenIssues = githubStats.OpenIssues,
                ClosedIssues = githubStats.ClosedIssues,
                CodeFrequency = githubStats.CodeFrequency,
                LastCommitDate = githubStats.LastCommitDate,
                LastCommitMessage = githubStats.LastCommitMessage,
                LastCommitAuthor = githubStats.LastCommitAuthor,
                RepositoryUrl = githubStats.RepositoryUrl,
                DefaultBranch = githubStats.DefaultBranch,
                LinesOfCode = githubStats.LinesOfCode,
                StarCount = githubStats.StarCount,
                ForkCount = githubStats.ForkCount,
                WatcherCount = githubStats.WatcherCount
            };
        }

        private DevelopmentStatsResponse MapToDevelopmentStatsResponse(dynamic devStats)
        {
            return new DevelopmentStatsResponse
            {
                TotalLinesOfCode = devStats.TotalLinesOfCode,
                FilesChanged = devStats.FilesChanged,
                CommitsThisWeek = devStats.CommitsThisWeek,
                CommitsThisMonth = devStats.CommitsThisMonth,
                ActiveBranches = devStats.ActiveBranches,
                CodeCoverage = devStats.CodeCoverage,
                TestsCount = devStats.TestsCount,
                PassingTests = devStats.PassingTests,
                FailingTests = devStats.FailingTests,
                BuildStatus = devStats.BuildStatus,
                LastBuildDate = devStats.LastBuildDate,
                AverageCommitSize = devStats.AverageCommitSize,
                TopContributors = devStats.TopContributors,
                MostActiveRepository = devStats.MostActiveRepository,
                TotalRepositories = devStats.TotalRepositories,
                OpenPullRequests = devStats.OpenPullRequests,
                CodeReviewsCompleted = devStats.CodeReviewsCompleted,
                DeploymentFrequency = devStats.DeploymentFrequency,
                LastDeploymentDate = devStats.LastDeploymentDate,
                TechnicalDebtRatio = devStats.TechnicalDebtRatio
            };
        }

        #endregion
    }
}