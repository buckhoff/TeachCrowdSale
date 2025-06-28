using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Models;
using TeachCrowdSale.Core.Models.Request;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Core.Interfaces.Services
{
    /// <summary>
    /// Service interface for PencilImpact platform dashboard operations
    /// </summary>
    public interface IPlatformDashboardService
    {
        /// <summary>
        /// Get comprehensive dashboard data for home page
        /// </summary>
        Task<PlatformHomeModel> GetHomePageDataAsync();

        /// <summary>
        /// Get featured demo projects for showcase
        /// </summary>
        Task<List<DemoProjectModel>> GetFeaturedProjectsAsync();

        /// <summary>
        /// Get current pencil drive information
        /// </summary>
        Task<PencilDriveModel> GetCurrentPencilDriveAsync();

        /// <summary>
        /// Submit waitlist signup
        /// </summary>
        Task<WaitlistSignupResponse> SubmitWaitlistSignupAsync(WaitlistSignupRequest request);

        /// <summary>
        /// Get waitlist statistics
        /// </summary>
        Task<WaitlistStats> GetWaitlistStatsAsync();

        /// <summary>
        /// Track platform analytics event
        /// </summary>
        Task TrackAnalyticsEventAsync(string pageUrl, string? sessionId, string? action, object? data);
    }
}
