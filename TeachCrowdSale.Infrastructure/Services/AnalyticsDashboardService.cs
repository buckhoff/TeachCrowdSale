using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models.Response;
using TeachCrowdSale.Core.Models.Treasury;

namespace TeachCrowdSale.Infrastructure.Services
{ 
    public class AnalyticsDashboardService : IAnalyticsDashboardService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AnalyticsDashboardService> _logger;

        // Cache keys and durations
        private const string CACHE_KEY_DASHBOARD = "analytics_dashboard";
        private const string CACHE_KEY_TOKEN_ANALYTICS = "analytics_token";
        private const string CACHE_KEY_PRESALE_ANALYTICS = "analytics_presale";
        private const string CACHE_KEY_PLATFORM_ANALYTICS = "analytics_platform";
        private const string CACHE_KEY_TREASURY_ANALYTICS = "analytics_treasury";
        private const string CACHE_KEY_SUMMARY = "analytics_summary";
        private const string CACHE_KEY_TIER_PERFORMANCE = "analytics_tier_performance";

        private readonly TimeSpan _shortCacheDuration = TimeSpan.FromMinutes(2);
        private readonly TimeSpan _mediumCacheDuration = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _longCacheDuration = TimeSpan.FromMinutes(15);

        // Retry configuration
        private const int MAX_RETRY_ATTEMPTS = 3;
        private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(1);

        public AnalyticsDashboardService(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            ILogger<AnalyticsDashboardService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("TeachAPI");
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AnalyticsDashboardResponse> GetDashboardDataAsync()
        {
            return await GetWithFallbackAsync(
                CACHE_KEY_DASHBOARD,
                "/api/analytics/dashboard",
                GetFallbackDashboardData,
                _shortCacheDuration);
        }

        public async Task<TokenAnalyticsResponse> GetTokenAnalyticsAsync()
        {
            return await GetWithFallbackAsync(
                CACHE_KEY_TOKEN_ANALYTICS,
                "/api/analytics/token",
                GetFallbackTokenAnalytics,
                _mediumCacheDuration);
        }

        public async Task<PresaleAnalyticsResponse> GetPresaleAnalyticsAsync()
        {
            return await GetWithFallbackAsync(
                CACHE_KEY_PRESALE_ANALYTICS,
                "/api/analytics/presale",
                GetFallbackPresaleAnalytics,
                _mediumCacheDuration);
        }

        public async Task<PlatformAnalyticsResponse> GetPlatformAnalyticsAsync()
        {
            return await GetWithFallbackAsync(
                CACHE_KEY_PLATFORM_ANALYTICS,
                "/api/analytics/platform",
                GetFallbackPlatformAnalytics,
                _longCacheDuration);
        }

        public async Task<TreasuryAnalyticsResponse> GetTreasuryAnalyticsAsync()
        {
            return await GetWithFallbackAsync(
                CACHE_KEY_TREASURY_ANALYTICS,
                "/api/analytics/treasury",
                GetFallbackTreasuryAnalytics,
                _longCacheDuration);
        }

        public async Task<List<TimeSeriesDataPointResponse>> GetPriceHistoryAsync(DateTime? startDate = null, DateTime? endDate = null, string interval = "1d")
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            var cacheKey = $"price_history_{start:yyyyMMdd}_{end:yyyyMMdd}_{interval}";

            var endpoint = $"/api/analytics/price-history?startDate={start:yyyy-MM-dd}&endDate={end:yyyy-MM-dd}&interval={interval}";

            return await GetWithFallbackAsync(
                cacheKey,
                endpoint,
                () => GetFallbackPriceHistory(start, end),
                _mediumCacheDuration);
        }

        public async Task<List<TimeSeriesDataPointResponse>> GetVolumeHistoryAsync(DateTime? startDate = null, DateTime? endDate = null, string interval = "1d")
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            var cacheKey = $"volume_history_{start:yyyyMMdd}_{end:yyyyMMdd}_{interval}";

            var endpoint = $"/api/analytics/volume-history?startDate={start:yyyy-MM-dd}&endDate={end:yyyy-MM-dd}&interval={interval}";

            return await GetWithFallbackAsync(
                cacheKey,
                endpoint,
                () => GetFallbackVolumeHistory(start, end),
                _mediumCacheDuration);
        }

        public async Task<object> GetAnalyticsSummaryAsync()
        {
            return await GetWithFallbackAsync(
                CACHE_KEY_SUMMARY,
                "/api/analytics/summary",
                GetFallbackSummary,
                _shortCacheDuration);
        }

        #region Private Helper Methods

        private async Task<T> GetWithFallbackAsync<T>(
            string cacheKey,
            string endpoint,
            Func<T> fallbackData,
            TimeSpan cacheDuration) where T : class
        {
            // Try cache first
            if (_cache.TryGetValue(cacheKey, out T? cachedData) && cachedData != null)
            {
                return cachedData;
            }

            // Try API with retry logic
            for (int attempt = 1; attempt <= MAX_RETRY_ATTEMPTS; attempt++)
            {
                try
                {
                    var response = await _httpClient.GetAsync(endpoint);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var data = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (data != null)
                        {
                            // Cache successful response
                            _cache.Set(cacheKey, data, cacheDuration);

                            if (attempt > 1)
                            {
                                _logger.LogInformation("Successfully retrieved {Endpoint} after {Attempt} attempts", endpoint, attempt);
                            }

                            return data;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("API request failed for {Endpoint} with status {StatusCode} on attempt {Attempt}",
                            endpoint, response.StatusCode, attempt);
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning(ex, "HTTP request exception for {Endpoint} on attempt {Attempt}", endpoint, attempt);
                }
                catch (TaskCanceledException ex)
                {
                    _logger.LogWarning(ex, "Request timeout for {Endpoint} on attempt {Attempt}", endpoint, attempt);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error for {Endpoint} on attempt {Attempt}", endpoint, attempt);
                }

                // Wait before retry (except on last attempt)
                if (attempt < MAX_RETRY_ATTEMPTS)
                {
                    await Task.Delay(_retryDelay * attempt); // Exponential backoff
                }
            }

            // All attempts failed, use fallback
            _logger.LogWarning("All API attempts failed for {Endpoint}, using fallback data", endpoint);

            var fallback = fallbackData();

            // Cache fallback data with shorter duration
            _cache.Set(cacheKey, fallback, TimeSpan.FromMinutes(1));

            return fallback;
        }
        public async Task<List<TierPerformanceResponse>?> GetTierPerformanceAsync()
        {
            var cacheKey = CACHE_KEY_TIER_PERFORMANCE;

            if (_cache.TryGetValue(cacheKey, out List<TierPerformanceResponse>? cachedData) && cachedData != null)
            {
                return cachedData;
            }

            try
            {
                var response = await _httpClient.GetAsync("/api/analytics/tiers");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<List<TierPerformanceResponse>>(content, GetJsonOptions());

                    if (data != null)
                    {
                        _cache.Set(cacheKey, data, _mediumCacheDuration);
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tier performance from API");
            }

            return GetFallbackTierPerformance();
        }

        #endregion

        #region Fallback Data Methods

        private AnalyticsDashboardResponse GetFallbackDashboardData()
        {
            return new AnalyticsDashboardResponse
            {
                TokenAnalytics = GetFallbackTokenAnalytics(),
                PresaleAnalytics = GetFallbackPresaleAnalytics(),
                PlatformAnalytics = GetFallbackPlatformAnalytics(),
                TreasuryAnalytics = GetFallbackTreasuryAnalytics(),
                PriceHistory = GetFallbackPriceHistory(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow),
                VolumeHistory = GetFallbackVolumeHistory(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow),
                TierPerformance = GetFallbackTierPerformance(),
                LoadedAt = DateTime.UtcNow
            };
        }

        private TokenAnalyticsResponse GetFallbackTokenAnalytics()
        {
            return new TokenAnalyticsResponse
            {
                CurrentPrice = 0.065m,
                MarketCap = 325_000_000m,
                Volume24h = 2_500_000m,
                PriceChange24h = 4.8m,
                PriceChange7d = 12.3m,
                PriceChange30d = 28.7m,
                HoldersCount = 3247,
                HoldersChange24h = 45m,
                TotalSupply = 5_000_000_000m,
                CirculatingSupply = 1_000_000_000m,
                BurnedTokens = 0m,
                StakedTokens = 0m,
                AllTimeHigh = 0.065m,
                AllTimeHighDate = DateTime.UtcNow,
                AllTimeLow = 0.04m,
                AllTimeLowDate = DateTime.UtcNow.AddDays(-30),
                LiquidityTokens = 50_000_000m,
                TreasuryTokens = 3_950_000_000m
            };
        }

        private PresaleAnalyticsResponse GetFallbackPresaleAnalytics()
        {
            return new PresaleAnalyticsResponse
            {
                TotalRaised = 12_500_000m,
                FundingGoal = 87_500_000m,
                TokensSold = 192_307_692m,
                TokensRemaining = 1_057_692_308m,
                ParticipantsCount = 2847,
                RaisedChange24h = 125_000m,
                RaisedChange7d = 875_000m,
                NewParticipants24h = 35,
                NewParticipants7d = 245,
                CurrentTier = new CurrentTierAnalyticsResponse
                {
                    TierId = 2,
                    TierName = "Community Round",
                    Price = 0.06m,
                    Allocation = 375_000_000m,
                    Sold = 169_000_000m,
                    Remaining = 206_000_000m,
                    SoldToday = 125_000m,
                    SoldThisWeek = 875_000m,
                    SalesVelocity = 1_250_000m,
                    EstimatedDaysToSellOut = 165,
                    ConversionRate = 2.8m,
                    PageViews24h = 1250,
                    UniquePurchasers24h = 35
                },
                PresaleStartDate = DateTime.UtcNow.AddDays(-60),
                PresaleEndDate = DateTime.UtcNow.AddDays(120),
                WeeklyRunRate = 875_000m,
                MonthlyProjection = 3_762_500m
            };
        }

        private PlatformAnalyticsResponse GetFallbackPlatformAnalytics()
        {
            return new PlatformAnalyticsResponse
            {
                TotalValueLocked = 0m,
                StakedTokens = 0m,
                RewardsDistributed = 0m,
                ActiveStakers = 0,
                TotalEducationFunding = 0m,
                EducatorsSupported = 0,
                StudentsImpacted = 0,
                SchoolsPartnered = 0,
                TransactionsToday = 45,
                ActiveUsers24h = 324,
                ActiveUsers7d = 1892,
                PlatformGrowthRate = 0m,
                UserRetentionRate = 0m,
                AverageSessionDuration = 0m
            };
        }

        private TreasuryAnalyticsResponse GetFallbackTreasuryAnalytics()
        {
            return new TreasuryAnalyticsResponse
            {
                TotalTreasuryValue = 87_500_000m,
                StabilityFundBalance = 8_750_000m,
                OperationalFunds = 70_000_000m,
                ReserveFunds = 8_750_000m,
                MonthlyBurnRate = 695_000m,
                OperationalRunwayMonths = 125,
                TotalRunwayMonths = 125,
                Overview = new TreasuryOverviewModel
                {
                    TotalValue = 87_500_000m,
                    OperationalRunwayYears = 10.5m,
                    MonthlyBurnRate = 695_000m,
                    SafetyFundValue = 8_750_000m,
                    StabilityFundValue = 8_750_000m,
                    LastUpdate = DateTime.UtcNow,
                    IsLatest = true
                },
                Allocations = GetFallbackTreasuryAllocations(),
                Performance = new TreasuryPerformanceModel
                {
                    EfficiencyRating = 85.5m,
                    CostPerUser = 12.50m,
                    RevenueGrowthRate = 15.2m,
                    SustainabilityScore = 92.0m
                },
                FundAllocations = GetFallbackFundAllocations(),
                TreasuryGrowthRate = 5.2m,
                FundingEfficiency = 87.3m,
                DiversificationRatio = 75.0m,
                RiskLevel = "Low",
                ProjectedRunwayExtension = 2.5m,
                EstimatedRunwayEndDate = DateTime.UtcNow.AddYears(12)
            };
        }

        private List<TimeSeriesDataPointResponse> GetFallbackPriceHistory(DateTime start, DateTime end)
        {
            var data = new List<TimeSeriesDataPointResponse>();
            var current = start;
            var random = new Random();
            var basePrice = 0.045m;

            while (current <= end)
            {
                var variation = (decimal)(random.NextDouble() * 0.01 - 0.005); // ±0.5%
                var price = Math.Max(0.04m, basePrice + variation);

                data.Add(new TimeSeriesDataPointResponse
                {
                    Timestamp = current,
                    Value = price,
                    Open = Math.Max(0.04m, price - (decimal)(random.NextDouble() * 0.002)),
                    High = price + (decimal)(random.NextDouble() * 0.003),
                    Low = Math.Max(0.04m, price - (decimal)(random.NextDouble() * 0.003)),
                    Close = price,
                    Volume = (decimal)(random.NextDouble() * 1000000 + 500000),
                    Category = "Price"
                });

                current = current.AddDays(1);
                basePrice = price; // Trending price
            }

            return data;
        }

        private List<TimeSeriesDataPointResponse> GetFallbackVolumeHistory(DateTime start, DateTime end)
        {
            var data = new List<TimeSeriesDataPointResponse>();
            var current = start;
            var random = new Random();

            while (current <= end)
            {
                var volume = (decimal)(random.NextDouble() * 2000000 + 1000000);

                data.Add(new TimeSeriesDataPointResponse
                {
                    Timestamp = current,
                    Value = volume,
                    Volume = volume,
                    Category = "Volume"
                });

                current = current.AddDays(1);
            }

            return data;
        }

        private List<TierPerformanceResponse> GetFallbackTierPerformance()
        {
            return new List<TierPerformanceResponse>
            {
                new TierPerformanceResponse
                {
                    TierId = 1,
                    TierName = "Seed Round",
                    Price = 0.04m,
                    Allocation = 250_000_000m,
                    Sold = 250_000_000m,
                    IsActive = false,
                    IsSoldOut = true,
                    Status = "Completed",
                    RevenueGenerated = 10_000_000m,
                    SalesVelocity = 0m,
                    SoldToday = 0m,
                    SoldThisWeek = 0m,
                    ParticipantsCount = 1247,
                    AverageInvestment = 8_018m
                },
                new TierPerformanceResponse
                {
                    TierId = 2,
                    TierName = "Community Round",
                    Price = 0.06m,
                    Allocation = 375_000_000m,
                    Sold = 169_000_000m,
                    IsActive = true,
                    IsSoldOut = false,
                    Status = "Active",
                    RevenueGenerated = 10_140_000m,
                    SalesVelocity = 1_250_000m,
                    SoldToday = 125_000m,
                    SoldThisWeek = 875_000m,
                    ParticipantsCount = 1600,
                    AverageInvestment = 6_337m
                }
            };
        }

        private List<TreasuryAllocationModel> GetFallbackTreasuryAllocations()
        {
            return new List<TreasuryAllocationModel>
            {
                new TreasuryAllocationModel { Category = "Development", Value = 26_250_000m, Percentage = 30m, Purpose = "Platform development and maintenance", Color = "#4F46E5" },
                new TreasuryAllocationModel { Category = "Marketing", Value = 17_500_000m, Percentage = 20m, Purpose = "User acquisition and brand building", Color = "#059669" },
                new TreasuryAllocationModel { Category = "Operations", Value = 13_125_000m, Percentage = 15m, Purpose = "Day-to-day operational expenses", Color = "#DC2626" },
                new TreasuryAllocationModel { Category = "Legal & Compliance", Value = 8_750_000m, Percentage = 10m, Purpose = "Legal framework and regulatory compliance", Color = "#7C2D12" },
                new TreasuryAllocationModel { Category = "Partnerships", Value = 8_750_000m, Percentage = 10m, Purpose = "Strategic partnerships and integrations", Color = "#BE185D" },
                new TreasuryAllocationModel { Category = "Reserve Fund", Value = 13_125_000m, Percentage = 15m, Purpose = "Emergency and strategic reserves", Color = "#374151" }
            };
        }

        private List<FundAllocationResponse> GetFallbackFundAllocations()
        {
            return new List<FundAllocationResponse>
            {
                new FundAllocationResponse { Category = "Development", Amount = 26_250_000m, Percentage = 30m, Status = "Allocated", LastUpdated = DateTime.UtcNow },
                new FundAllocationResponse { Category = "Marketing", Amount = 17_500_000m, Percentage = 20m, Status = "Allocated", LastUpdated = DateTime.UtcNow },
                new FundAllocationResponse { Category = "Operations", Amount = 13_125_000m, Percentage = 15m, Status = "Active", LastUpdated = DateTime.UtcNow },
                new FundAllocationResponse { Category = "Reserve", Amount = 13_125_000m, Percentage = 15m, Status = "Reserved", LastUpdated = DateTime.UtcNow }
            };
        }

        private object GetFallbackSummary()
        {
            return new
            {
                tokenPrice = 0.065m,
                priceChange24h = 4.8m,
                marketCap = 325_000_000m,
                volume24h = 2_500_000m,
                totalRaised = 12_500_000m,
                fundingProgress = 14.3m,
                participants = 2847,
                currentTier = new
                {
                    name = "Community Round",
                    price = 0.06m,
                    progress = 45.1m
                },
                lastUpdated = DateTime.UtcNow
            };
        }

        #endregion

        #region Helper Methods

        private static JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        #endregion
    }
}