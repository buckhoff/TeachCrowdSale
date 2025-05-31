using TeachCrowdSale.Core.Models.Burning;
using TeachCrowdSale.Core.Models.Governance;
using TeachCrowdSale.Core.Models.Tokenomics;
using TeachCrowdSale.Core.Models.Treasury;
using TeachCrowdSale.Core.Models.Utility;
using TeachCrowdSale.Core.Models.Vesting;

namespace TeachCrowdSale.Core.Interfaces
{
    /// <summary>
    /// Service interface for tokenomics data operations
    /// </summary>
    public interface ITokenomicsService
    {
        /// <summary>
        /// Get comprehensive tokenomics page data
        /// </summary>
        Task<TokenomicsDisplayModel> GetTokenomicsDataAsync();

        /// <summary>
        /// Get live token metrics for real-time updates
        /// </summary>
        Task<LiveTokenMetricsModel> GetLiveTokenMetricsAsync();

        /// <summary>
        /// Get detailed supply breakdown information
        /// </summary>
        Task<SupplyBreakdownModel> GetSupplyBreakdownAsync();

        /// <summary>
        /// Get vesting schedule data for all categories
        /// </summary>
        Task<VestingScheduleModel> GetVestingScheduleAsync();

        /// <summary>
        /// Get burn mechanics and statistics
        /// </summary>
        Task<BurnMechanicsModel> GetBurnMechanicsAsync();

        /// <summary>
        /// Get treasury analytics and runway information
        /// </summary>
        Task<TreasuryAnalyticsModel> GetTreasuryAnalyticsAsync();

        /// <summary>
        /// Get utility features and usage metrics
        /// </summary>
        Task<UtilityFeaturesModel> GetUtilityFeaturesAsync();

        /// <summary>
        /// Get governance information and proposals
        /// </summary>
        Task<GovernanceInfoModel> GetGovernanceInfoAsync();

        /// <summary>
        /// Get fallback data when API calls fail
        /// </summary>
        TokenomicsDisplayModel GetFallbackTokenomicsData();

        /// <summary>
        /// Check API health status
        /// </summary>
        Task<bool> CheckApiHealthAsync();
    }
}