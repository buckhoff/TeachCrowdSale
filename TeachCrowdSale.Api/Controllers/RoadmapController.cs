// TeachCrowdSale.Api/Controllers/RoadmapApiController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Models;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Api.Controllers
{
    [EnableRateLimiting("Roadmap")]
    [ApiController]
    [Route("api/roadmap")]
    public class RoadmapController : ControllerBase
    {
        private readonly IRoadmapService _roadmapService;
        private readonly ILogger<RoadmapController> _logger;

        public RoadmapController(
            IRoadmapService roadmapService,
            ILogger<RoadmapController> logger)
        {
            _roadmapService = roadmapService ?? throw new ArgumentNullException(nameof(roadmapService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get comprehensive roadmap page data
        /// </summary>
        [HttpGet("data")]
        [ResponseCache(Duration = 300)] // 5 minutes
        public async Task<ActionResult<RoadmapDataModel>> GetRoadmapData()
        {
            try
            {
                var roadmapData = await _roadmapService.GetRoadmapDataAsync();
                return Ok(roadmapData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comprehensive roadmap data");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving roadmap data",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get all milestones with optional filtering
        /// </summary>
        [HttpGet("milestones")]
        [ResponseCache(Duration = 600)] // 10 minutes
        public async Task<ActionResult<List<MilestoneDisplayModel>>> GetMilestones(
            [FromQuery] string? status = null,
            [FromQuery] string? category = null)
        {
            try
            {
                var milestones = await _roadmapService.GetMilestonesAsync(status, category);
                return Ok(milestones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving milestones with status: {Status}, category: {Category}", status, category);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving milestones",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get milestone by ID with full details
        /// </summary>
        [HttpGet("milestones/{id:int}")]
        [ResponseCache(Duration = 300)] // 5 minutes
        public async Task<ActionResult<MilestoneDisplayModel>> GetMilestone([FromRoute] int id)
        {
            try
            {
                var milestone = await _roadmapService.GetMilestoneAsync(id);

                if (milestone == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Message = $"Milestone with ID {id} not found",
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                return Ok(milestone);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving milestone {MilestoneId}", id);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving milestone details",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get tasks for a specific milestone
        /// </summary>
        [HttpGet("milestones/{id:int}/tasks")]
        [ResponseCache(Duration = 300)] // 5 minutes
        public async Task<ActionResult<List<TaskDisplayModel>>> GetMilestoneTasks([FromRoute] int id)
        {
            try
            {
                var tasks = await _roadmapService.GetMilestoneTasksAsync(id);
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks for milestone {MilestoneId}", id);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving milestone tasks",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get dependencies for a specific milestone
        /// </summary>
        [HttpGet("milestones/{id:int}/dependencies")]
        [ResponseCache(Duration = 600)] // 10 minutes
        public async Task<ActionResult<List<DependencyDisplayModel>>> GetMilestoneDependencies([FromRoute] int id)
        {
            try
            {
                var dependencies = await _roadmapService.GetMilestoneDependenciesAsync(id);
                return Ok(dependencies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dependencies for milestone {MilestoneId}", id);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving milestone dependencies",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get milestone progress history for charts
        /// </summary>
        [HttpGet("milestones/{id:int}/progress-history")]
        [ResponseCache(Duration = 1800)] // 30 minutes
        public async Task<ActionResult<object>> GetMilestoneProgressHistory([FromRoute] int id)
        {
            try
            {
                var progressHistory = await _roadmapService.GetProgressHistoryAsync(id);
                return Ok(progressHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving progress history for milestone {MilestoneId}", id);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving progress history",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Search milestones by term
        /// </summary>
        [HttpGet("milestones/search")]
        [ResponseCache(Duration = 300)] // 5 minutes
        public async Task<ActionResult<List<MilestoneDisplayModel>>> SearchMilestones([FromQuery] string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Search term must be at least 2 characters long",
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                var milestones = await _roadmapService.SearchMilestonesAsync(q);
                return Ok(milestones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching milestones with term: {SearchTerm}", q);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error searching milestones",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get development statistics
        /// </summary>
        [HttpGet("stats")]
        [ResponseCache(Duration = 900)] // 15 minutes
        public async Task<ActionResult<GitHubDevelopmentStatsModel>> GetDevelopmentStats()
        {
            try
            {
                var stats = await _roadmapService.GetDevelopmentStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving development statistics");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving development statistics",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get recent development updates
        /// </summary>
        [HttpGet("updates")]
        [ResponseCache(Duration = 300)] // 5 minutes
        public async Task<ActionResult<List<UpdateDisplayModel>>> GetRecentUpdates([FromQuery] int count = 10)
        {
            try
            {
                if (count < 1 || count > 50)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Count must be between 1 and 50",
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                var updates = await _roadmapService.GetRecentUpdatesAsync(count);
                return Ok(updates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent updates");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving recent updates",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get releases information
        /// </summary>
        [HttpGet("releases")]
        [ResponseCache(Duration = 1800)] // 30 minutes
        public async Task<ActionResult<List<ReleaseDisplayModel>>> GetReleases()
        {
            try
            {
                var releases = await _roadmapService.GetReleasesAsync();
                return Ok(releases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving releases");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving releases",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get GitHub statistics
        /// </summary>
        [HttpGet("github-stats")]
        [ResponseCache(Duration = 900)] // 15 minutes
        public async Task<ActionResult<GitHubStatsModel>> GetGitHubStats()
        {
            try
            {
                var stats = await _roadmapService.GetGitHubStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving GitHub statistics");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving GitHub statistics",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get roadmap filter options
        /// </summary>
        [HttpGet("filters")]
        [ResponseCache(Duration = 3600)] // 1 hour
        public async Task<ActionResult<RoadmapFilterModel>> GetFilterOptions()
        {
            try
            {
                var filterOptions = await _roadmapService.GetFilterOptionsAsync();
                return Ok(filterOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving filter options");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving filter options",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get development timeline data for charts
        /// </summary>
        [HttpGet("timeline")]
        [ResponseCache(Duration = 1800)] // 30 minutes
        public async Task<ActionResult<object>> GetTimelineData()
        {
            try
            {
                var timelineData = await _roadmapService.GetTimelineDataAsync();
                return Ok(timelineData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving timeline data");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Error retrieving timeline data",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }
    }
}