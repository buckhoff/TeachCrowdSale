// TeachCrowdSale.Infrastructure/Services/AnalyticsService.cs
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Models.Response;
using TeachCrowdSale.Core.Interfaces.Repositories;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models.Treasury;

namespace TeachCrowdSale.Infrastructure.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IAnalyticsRepository _analyticsRepository;
        private readonly IPresaleService _presaleService;
        private readonly ITokenContractService _tokenService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AnalyticsService> _logger;

        // Cache keys and durations
        private const string CACHE_KEY_DASHBOARD = "analytics_dashboard";
        private const string CACHE_KEY_TOKEN_ANALYTICS = "analytics_token";
        private const string CACHE_KEY_PRESALE_ANALYTICS = "analytics_presale";
        private const string CACHE_KEY_PLATFORM_ANALYTICS = "analytics_platform";
        private const string CACHE_KEY_TREASURY_ANALYTICS = "analytics_treasury";

        private readonly TimeSpan _shortCacheDuration = TimeSpan.FromMinutes(2);
        private readonly TimeSpan _mediumCacheDuration = TimeSpan.FromMinutes(10);
        private readonly TimeSpan _longCacheDuration = TimeSpan.FromMinutes(30);

        public AnalyticsService(
            IAnalyticsRepository analyticsRepository,
            IPresaleService presaleService,
            ITokenContractService tokenService,
            IMemoryCache cache,
            ILogger<AnalyticsService> logger)
        {
            _analyticsRepository = analyticsRepository ?? throw new ArgumentNullException(nameof(analyticsRepository));
            _presaleService = presaleService ?? throw new ArgumentNullException(nameof(presaleService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AnalyticsDashboardResponse> GetAnalyticsDashboardAsync()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_DASHBOARD, out AnalyticsDashboardResponse? cachedDashboard) && cachedDashboard != null)
                {
                    return cachedDashboard;
                }

                var dashboard = new AnalyticsDashboardResponse();

                // Load all analytics data in parallel
                var tokenAnalyticsTask = GetTokenAnalyticsAsync();
                var presaleAnalyticsTask = GetPresaleAnalyticsAsync();
                var platformAnalyticsTask = GetPlatformAnalyticsAsync();
                var treasuryAnalyticsTask = GetTreasuryAnalyticsAsync();
                var tierPerformanceTask = GetTierPerformanceAsync();

                // Get historical data for charts
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddDays(-30); // Last 30 days
                var priceHistoryTask = GetPriceHistoryAsync(startDate, endDate);
                var volumeHistoryTask = GetVolumeHistoryAsync(startDate, endDate);

                await Task.WhenAll(
                    tokenAnalyticsTask,
                    presaleAnalyticsTask,
                    platformAnalyticsTask,
                    treasuryAnalyticsTask,
                    tierPerformanceTask,
                    priceHistoryTask,
                    volumeHistoryTask
                );

                dashboard.TokenAnalytics = await tokenAnalyticsTask;
                dashboard.PresaleAnalytics = await presaleAnalyticsTask;
                dashboard.PlatformAnalytics = await platformAnalyticsTask;
                dashboard.TreasuryAnalytics = await treasuryAnalyticsTask;
                dashboard.TierPerformance = await tierPerformanceTask;
                dashboard.PriceHistory = await priceHistoryTask;
                dashboard.VolumeHistory = await volumeHistoryTask;

                _cache.Set(CACHE_KEY_DASHBOARD, dashboard, _shortCacheDuration);
                return dashboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving analytics dashboard");
                throw;
            }
        }

        public async Task<TokenAnalyticsResponse> GetTokenAnalyticsAsync()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_TOKEN_ANALYTICS, out TokenAnalyticsResponse? cached) && cached != null)
                {
                    return cached;
                }

                var tokenAnalytics = new TokenAnalyticsResponse();

                // Get current token data
                var currentPrice = await GetTokenPriceSafely();
                var totalSupply = await GetTotalSupplySafely();
                var circulatingSupply = await GetCirculatingSupplySafely();
                var holdersCount = await GetHoldersCountSafely();
                var marketCap = await GetMarketCapSafely();
                var volume24h = await GetVolume24hSafely();
                var burnedTokens = await GetBurnedTokensSafely();
                var stakedTokens = await GetStakedTokensSafely();

                // Calculate price changes
                var priceChange24h = await CalculatePriceChangeAsync(TimeSpan.FromDays(1));
                var priceChange7d = await CalculatePriceChangeAsync(TimeSpan.FromDays(7));
                var priceChange30d = await CalculatePriceChangeAsync(TimeSpan.FromDays(30));

                // Get historical price data for ATH/ATL
                var priceHistory = await GetPriceHistoryAsync(DateTime.UtcNow.AddYears(-1), DateTime.UtcNow);
                var (allTimeHigh, athDate, allTimeLow, atlDate) = CalculateAllTimeHighLow(priceHistory);

                tokenAnalytics.CurrentPrice = currentPrice;
                tokenAnalytics.MarketCap = marketCap;
                tokenAnalytics.Volume24h = volume24h;
                tokenAnalytics.PriceChange24h = priceChange24h;
                tokenAnalytics.PriceChange7d = priceChange7d;
                tokenAnalytics.PriceChange30d = priceChange30d;
                tokenAnalytics.HoldersCount = holdersCount;
                tokenAnalytics.HoldersChange24h = await CalculateHoldersChangeAsync();
                tokenAnalytics.TotalSupply = totalSupply;
                tokenAnalytics.CirculatingSupply = circulatingSupply;
                tokenAnalytics.BurnedTokens = burnedTokens;
                tokenAnalytics.StakedTokens = stakedTokens;
                tokenAnalytics.AllTimeHigh = allTimeHigh;
                tokenAnalytics.AllTimeHighDate = athDate;
                tokenAnalytics.AllTimeLow = allTimeLow;
                tokenAnalytics.AllTimeLowDate = atlDate;

                _cache.Set(CACHE_KEY_TOKEN_ANALYTICS, tokenAnalytics, _mediumCacheDuration);
                return tokenAnalytics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving token analytics");
                throw;
            }
        }

        public async Task<PresaleAnalyticsResponse> GetPresaleAnalyticsAsync()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_PRESALE_ANALYTICS, out PresaleAnalyticsResponse? cached) && cached != null)
                {
                    return cached;
                }

                var presaleAnalytics = new PresaleAnalyticsResponse();

                // Get current presale stats
                var presaleStats = await _presaleService.GetPresaleStatsAsync();
                var currentTier = await _presaleService.GetCurrentTierAsync();

                presaleAnalytics.TotalRaised = presaleStats.TotalRaised;
                presaleAnalytics.FundingGoal = presaleStats.FundingGoal;
                presaleAnalytics.TokensSold = presaleStats.TokensSold;
                presaleAnalytics.TokensRemaining = presaleStats.TokensRemaining;
                presaleAnalytics.ParticipantsCount = presaleStats.ParticipantsCount;

                // Calculate growth metrics
                presaleAnalytics.RaisedChange24h = await CalculateRaisedChangeAsync(TimeSpan.FromDays(1));
                presaleAnalytics.RaisedChange7d = await CalculateRaisedChangeAsync(TimeSpan.FromDays(7));
                presaleAnalytics.NewParticipants24h = await CalculateNewParticipantsAsync(TimeSpan.FromDays(1));
                presaleAnalytics.NewParticipants7d = await CalculateNewParticipantsAsync(TimeSpan.FromDays(7));

                // Current tier analytics
                if (currentTier != null)
                {
                    presaleAnalytics.CurrentTier = await GetCurrentTierAnalyticsAsync(currentTier);
                }

                // Timeline metrics
                presaleAnalytics.PresaleStartDate = presaleStats.PresaleStartTime;
                presaleAnalytics.PresaleEndDate = presaleStats.PresaleEndTime != DateTime.MinValue ? presaleStats.PresaleEndTime : null;

                // Performance calculations
                var dailyAnalytics = await _analyticsRepository.GetDailyAnalyticsRangeAsync(
                    DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

                if (dailyAnalytics.Any())
                {
                    presaleAnalytics.WeeklyRunRate = dailyAnalytics.Sum(d => d.DailyUsdRaised);
                    presaleAnalytics.MonthlyProjection = presaleAnalytics.WeeklyRunRate * 4.3m; // ~4.3 weeks per month
                }

                _cache.Set(CACHE_KEY_PRESALE_ANALYTICS, presaleAnalytics, _mediumCacheDuration);
                return presaleAnalytics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving presale analytics");
                throw;
            }
        }

        public async Task<PlatformAnalyticsResponse> GetPlatformAnalyticsAsync()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_PLATFORM_ANALYTICS, out PlatformAnalyticsResponse? cached) && cached != null)
                {
                    return cached;
                }

                // Platform analytics (most will be zero until platform launches)
                var platformAnalytics = new PlatformAnalyticsResponse
                {
                    // These will be populated from blockchain data when platform is live
                    TotalValueLocked = 0,
                    StakedTokens = await GetStakedTokensSafely(),
                    RewardsDistributed = 0,
                    ActiveStakers = 0,
                    TotalEducationFunding = 0,
                    EducatorsSupported = 0,
                    StudentsImpacted = 0,
                    SchoolsPartnered = 0,
                    TransactionsToday = await GetTransactionCountAsync(TimeSpan.FromDays(1)),
                    ActiveUsers24h = 0, // Will come from platform analytics
                    ActiveUsers7d = 0,
                    PlatformGrowthRate = 0,
                    UserRetentionRate = 0,
                    AverageSessionDuration = 0
                };

                _cache.Set(CACHE_KEY_PLATFORM_ANALYTICS, platformAnalytics, _longCacheDuration);
                return platformAnalytics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving platform analytics");
                throw;
            }
        }

        public async Task<TreasuryAnalyticsResponse> GetTreasuryAnalyticsAsync()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_TREASURY_ANALYTICS, out TreasuryAnalyticsResponse? cached) && cached != null)
                {
                    return cached;
                }

                var treasuryAnalytics = new TreasuryAnalyticsResponse();

                // Get latest snapshot for treasury data
                var latestSnapshot = await _analyticsRepository.GetLatestSnapshotAsync();

                if (latestSnapshot != null)
                {
                    treasuryAnalytics.TotalTreasuryValue = latestSnapshot.TreasuryBalance;
                    treasuryAnalytics.StabilityFundBalance = latestSnapshot.StabilityFundBalance;

                    // Calculate operational metrics
                    treasuryAnalytics.MonthlyBurnRate = await CalculateMonthlyBurnRateAsync();
                    treasuryAnalytics.OperationalRunwayMonths = CalculateOperationalRunway(
                        treasuryAnalytics.TotalTreasuryValue, treasuryAnalytics.MonthlyBurnRate);

                    // Mock fund allocations for demonstration
                    treasuryAnalytics.FundAllocations = GetMockFundAllocations();

                    // Risk assessment
                    treasuryAnalytics.RiskLevel = AssessRiskLevel(treasuryAnalytics.OperationalRunwayMonths);
                }

                _cache.Set(CACHE_KEY_TREASURY_ANALYTICS, treasuryAnalytics, _longCacheDuration);
                return treasuryAnalytics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving treasury analytics");
                throw;
            }
        }

        public async Task<List<TierPerformanceResponse>> GetTierPerformanceAsync()
        {
            try
            {
                var allTiers = await _presaleService.GetAllTiersAsync();
                var tierPerformance = new List<TierPerformanceResponse>();

                foreach (var tier in allTiers)
                {
                    var performance = new TierPerformanceResponse
                    {
                        TierId = tier.Id,
                        TierName = GetTierName(tier.Id),
                        Price = tier.Price,
                        Allocation = tier.Allocation,
                        Sold = tier.Sold,
                        IsActive = tier.IsActive,
                        IsSoldOut = tier.Sold >= tier.Allocation,
                        Status = GetTierStatus(tier),
                        RevenueGenerated = tier.Sold * tier.Price,
                        SalesVelocity = await CalculateTierSalesVelocityAsync(tier.Id)
                    };

                    // Get tier-specific analytics
                    var tierSnapshots = await _analyticsRepository.GetTierSnapshotsAsync(
                        DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, tier.Id);

                    if (tierSnapshots.Any())
                    {
                        performance.SoldToday = await CalculateTierSoldTodayAsync(tier.Id);
                        performance.SoldThisWeek = await CalculateTierSoldThisWeekAsync(tier.Id);
                    }

                    tierPerformance.Add(performance);
                }

                return tierPerformance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tier performance");
                throw;
            }
        }

        public async Task<List<TimeSeriesDataPointResponse>> GetPriceHistoryAsync(DateTime startDate, DateTime endDate, string interval = "1d")
        {
            try
            {
                var snapshots = await _analyticsRepository.GetSnapshotsAsync(startDate, endDate);
                var priceHistory = new List<TimeSeriesDataPointResponse>();

                // Group by interval
                var groupedData = interval switch
                {
                    "1h" => GroupSnapshotsByHour(snapshots),
                    "4h" => GroupSnapshotsByHours(snapshots, 4),
                    "1d" => GroupSnapshotsByDay(snapshots),
                    "1w" => GroupSnapshotsByWeek(snapshots),
                    _ => GroupSnapshotsByDay(snapshots)
                };

                foreach (var group in groupedData)
                {
                    if (group.Any())
                    {
                        var dataPoint = new TimeSeriesDataPointResponse
                        {
                            Timestamp = group.Key,
                            Value = group.Average(s => s.TokenPrice),
                            Open = group.First().TokenPrice,
                            High = group.Max(s => s.TokenPrice),
                            Low = group.Min(s => s.TokenPrice),
                            Close = group.Last().TokenPrice,
                            Volume = group.Average(s => s.Volume24h)
                        };

                        price