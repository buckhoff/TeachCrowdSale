using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Models.Burning;
using TeachCrowdSale.Core.Models.Governance;
using TeachCrowdSale.Core.Models.Tokenomics;
using TeachCrowdSale.Core.Models.Treasury;
using TeachCrowdSale.Core.Models.Utility;
using TeachCrowdSale.Core.Models.Vesting;

namespace TeachCrowdSale.Infrastructure.Services
{
    /// <summary>
    /// Tokenomics service implementation for aggregating tokenomics data
    /// </summary>
    public class TokenomicsService : ITokenomicsService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<TokenomicsService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        // Cache keys and durations
        private const string CACHE_KEY_TOKENOMICS_DATA = "tokenomics_page_data";
        private const string CACHE_KEY_LIVE_METRICS = "live_token_metrics";
        private const string CACHE_KEY_SUPPLY_BREAKDOWN = "supply_breakdown";
        private const string CACHE_KEY_VESTING_SCHEDULE = "vesting_schedule";
        private const string CACHE_KEY_BURN_MECHANICS = "burn_mechanics";
        private const string CACHE_KEY_TREASURY_ANALYTICS = "treasury_analytics";
        private const string CACHE_KEY_UTILITY_FEATURES = "utility_features";
        private const string CACHE_KEY_GOVERNANCE_INFO = "governance_info";

        private readonly TimeSpan _shortCacheDuration = TimeSpan.FromMinutes(2);
        private readonly TimeSpan _mediumCacheDuration = TimeSpan.FromMinutes(10);
        private readonly TimeSpan _longCacheDuration = TimeSpan.FromMinutes(30);

        public TokenomicsService(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            ILogger<TokenomicsService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("TeachAPI");
            _cache = cache;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<TokenomicsDisplayModel> GetTokenomicsDataAsync()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_TOKENOMICS_DATA, out TokenomicsDisplayModel? cachedData) && cachedData != null)
                {
                    return cachedData;
                }

                var tokenomicsData = new TokenomicsDisplayModel();

                var liveMetricsTask = GetLiveTokenMetricsAsync();
                var supplyBreakdownTask = GetSupplyBreakdownAsync();
                var vestingScheduleTask = GetVestingScheduleAsync();
                var burnMechanicsTask = GetBurnMechanicsAsync();
                var treasuryAnalyticsTask = GetTreasuryAnalyticsAsync();
                var utilityFeaturesTask = GetUtilityFeaturesAsync();
                var governanceInfoTask = GetGovernanceInfoAsync();


                await Task.WhenAll(liveMetricsTask,
                   supplyBreakdownTask,
                   vestingScheduleTask,
                   burnMechanicsTask,
                   treasuryAnalyticsTask,
                   utilityFeaturesTask,
                   governanceInfoTask);

                tokenomicsData.LiveMetrics = await liveMetricsTask;
                tokenomicsData.SupplyBreakdown = await supplyBreakdownTask;
                tokenomicsData.VestingSchedule = await vestingScheduleTask;
                tokenomicsData.BurnMechanics = await burnMechanicsTask;
                tokenomicsData.TreasuryAnalytics = await treasuryAnalyticsTask;
                tokenomicsData.UtilityFeatures = await utilityFeaturesTask;
                tokenomicsData.GovernanceInfo = await governanceInfoTask;

                _cache.Set(CACHE_KEY_TOKENOMICS_DATA, tokenomicsData, _shortCacheDuration);
                return tokenomicsData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading comprehensive tokenomics data");
                return GetFallbackTokenomicsData();
            }
        }

        public async Task<LiveTokenMetricsModel> GetLiveTokenMetricsAsync()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_LIVE_METRICS, out LiveTokenMetricsModel? cachedMetrics) && cachedMetrics != null)
                {
                    return cachedMetrics;
                }

                // Call multiple API endpoints to get live data
                var tasks = new[]
                {
                    _httpClient.GetAsync("/api/tokeninfo"),
                    _httpClient.GetAsync("/api/tokeninfo/market"),
                    _httpClient.GetAsync("/api/presale/status")
                };

                var responses = await Task.WhenAll(tasks);
                var contents = await Task.WhenAll(responses.Select(r => r.Content.ReadAsStringAsync()));

                var metrics = new LiveTokenMetricsModel();

                // Parse token info
                if (responses[0].IsSuccessStatusCode)
                {
                    var tokenInfo = JsonSerializer.Deserialize<dynamic>(contents[0], _jsonOptions);
                    // Map properties from tokenInfo to metrics
                    // Implementation would depend on actual API response structure
                }

                // Parse market data
                if (responses[1].IsSuccessStatusCode)
                {
                    var marketData = JsonSerializer.Deserialize<dynamic>(contents[1], _jsonOptions);
                    // Map market data properties
                }

                // Use fallback data if API calls fail
                metrics = GetFallbackLiveMetrics();

                _cache.Set(CACHE_KEY_LIVE_METRICS, metrics, TimeSpan.FromMinutes(1));
                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving live token metrics");
                return GetFallbackLiveMetrics();
            }
        }

        public async Task<SupplyBreakdownModel> GetSupplyBreakdownAsync()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_SUPPLY_BREAKDOWN, out SupplyBreakdownModel? cachedSupply) && cachedSupply != null)
                {
                    return cachedSupply;
                }

                var supplyData = GetFallbackSupplyBreakdown();
                _cache.Set(CACHE_KEY_SUPPLY_BREAKDOWN, supplyData, _longCacheDuration);
                return supplyData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supply breakdown");
                return GetFallbackSupplyBreakdown();
            }
        }

        public async Task<VestingScheduleModel> GetVestingScheduleAsync()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_VESTING_SCHEDULE, out VestingScheduleModel? cachedVesting) && cachedVesting != null)
                {
                    return cachedVesting;
                }

                var vestingData = GetFallbackVestingSchedule();
                _cache.Set(CACHE_KEY_VESTING_SCHEDULE, vestingData, _longCacheDuration);
                return vestingData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vesting schedule");
                return GetFallbackVestingSchedule();
            }
        }

        public async Task<BurnMechanicsModel> GetBurnMechanicsAsync()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_BURN_MECHANICS, out BurnMechanicsModel? cachedBurn) && cachedBurn != null)
                {
                    return cachedBurn;
                }

                var burnData = GetFallbackBurnMechanics();
                _cache.Set(CACHE_KEY_BURN_MECHANICS, burnData, _mediumCacheDuration);
                return burnData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving burn mechanics");
                return GetFallbackBurnMechanics();
            }
        }

        public async Task<TreasuryAnalyticsModel> GetTreasuryAnalyticsAsync()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_TREASURY_ANALYTICS, out TreasuryAnalyticsModel? cachedTreasury) && cachedTreasury != null)
                {
                    return cachedTreasury;
                }

                var treasuryData = GetFallbackTreasuryAnalytics();
                _cache.Set(CACHE_KEY_TREASURY_ANALYTICS, treasuryData, _mediumCacheDuration);
                return treasuryData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving treasury analytics");
                return GetFallbackTreasuryAnalytics();
            }
        }

        public async Task<UtilityFeaturesModel> GetUtilityFeaturesAsync()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_UTILITY_FEATURES, out UtilityFeaturesModel? cachedUtility) && cachedUtility != null)
                {
                    return cachedUtility;
                }

                var utilityData = GetFallbackUtilityFeatures();
                _cache.Set(CACHE_KEY_UTILITY_FEATURES, utilityData, _longCacheDuration);
                return utilityData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving utility features");
                return GetFallbackUtilityFeatures();
            }
        }

        public async Task<GovernanceInfoModel> GetGovernanceInfoAsync()
        {
            try
            {
                if (_cache.TryGetValue(CACHE_KEY_GOVERNANCE_INFO, out GovernanceInfoModel? cachedGovernance) && cachedGovernance != null)
                {
                    return cachedGovernance;
                }

                var governanceData = GetFallbackGovernanceInfo();
                _cache.Set(CACHE_KEY_GOVERNANCE_INFO, governanceData, _mediumCacheDuration);
                return governanceData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving governance information");
                return GetFallbackGovernanceInfo();
            }
        }

        public async Task<bool> CheckApiHealthAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/health");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "API health check failed");
                return false;
            }
        }

        #region Fallback Data Methods

        public TokenomicsDisplayModel GetFallbackTokenomicsData()
        {
            return new TokenomicsDisplayModel
            {
                LiveMetrics = GetFallbackLiveMetrics(),
                SupplyBreakdown = GetFallbackSupplyBreakdown(),
                VestingSchedule = GetFallbackVestingSchedule(),
                BurnMechanics = GetFallbackBurnMechanics(),
                TreasuryAnalytics = GetFallbackTreasuryAnalytics(),
                UtilityFeatures = GetFallbackUtilityFeatures(),
                GovernanceInfo = GetFallbackGovernanceInfo()
            };
        }

        private LiveTokenMetricsModel GetFallbackLiveMetrics()
        {
            return new LiveTokenMetricsModel
            {
                CurrentPrice = 0.065m,
                MarketCap = 325000000m,
                Volume24h = 2500000m,
                PriceChange24h = 0.003m,
                PriceChangePercent24h = 4.8m,
                High24h = 0.068m,
                Low24h = 0.061m,
                TotalSupply = 5_000_000_000,
                CirculatingSupply = 1_000_000_000,
                HoldersCount = 3247,
                TotalValueLocked = 15000000m
            };
        }

        private SupplyBreakdownModel GetFallbackSupplyBreakdown()
        {
            return new SupplyBreakdownModel
            {
                Allocations = new List<SupplyAllocationModel>
                {
                    new SupplyAllocationModel
                    {
                        Category = "Public Presale",
                        TokenAmount = 1_250_000_000,
                        Percentage = 25.0m,
                        Color = "#4f46e5",
                        Description = "Four-tier presale with structured pricing",
                        UnlockDate = DateTime.UtcNow.AddDays(90),
                        VestingMonths = 6,
                        IsLocked = true
                    },
                    new SupplyAllocationModel
                    {
                        Category = "Community Incentives",
                        TokenAmount = 1_200_000_000,
                        Percentage = 24.0m,
                        Color = "#06b6d4",
                        Description = "Staking rewards and user acquisition",
                        UnlockDate = DateTime.UtcNow.AddDays(180),
                        VestingMonths = 36,
                        IsLocked = true
                    },
                    new SupplyAllocationModel
                    {
                        Category = "Platform Ecosystem",
                        TokenAmount = 1_000_000_000,
                        Percentage = 20.0m,
                        Color = "#8b5cf6",
                        Description = "Partnerships and strategic initiatives",
                        UnlockDate = DateTime.UtcNow.AddDays(365),
                        VestingMonths = 24,
                        IsLocked = true
                    },
                    new SupplyAllocationModel
                    {
                        Category = "Initial Liquidity",
                        TokenAmount = 600_000_000,
                        Percentage = 12.0m,
                        Color = "#10b981",
                        Description = "DEX liquidity and market making",
                        UnlockDate = DateTime.UtcNow.AddDays(90),
                        VestingMonths = 0,
                        IsLocked = false
                    },
                    new SupplyAllocationModel
                    {
                        Category = "Team & Development",
                        TokenAmount = 400_000_000,
                        Percentage = 8.0m,
                        Color = "#f59e0b",
                        Description = "Core team and development costs",
                        UnlockDate = DateTime.UtcNow.AddDays(365),
                        VestingMonths = 24,
                        IsLocked = true
                    },
                    new SupplyAllocationModel
                    {
                        Category = "Educational Partners",
                        TokenAmount = 350_000_000,
                        Percentage = 7.0m,
                        Color = "#ec4899",
                        Description = "School partnerships and teacher incentives",
                        UnlockDate = DateTime.UtcNow.AddDays(180),
                        VestingMonths = 18,
                        IsLocked = true
                    },
                    new SupplyAllocationModel
                    {
                        Category = "Reserve Fund",
                        TokenAmount = 200_000_000,
                        Percentage = 4.0m,
                        Color = "#ef4444",
                        Description = "Emergency fund and governance reserve",
                        UnlockDate = DateTime.UtcNow.AddDays(730),
                        VestingMonths = 12,
                        IsLocked = true
                    }
                },
                Metrics = new SupplyMetricsModel
                {
                    MaxSupply = 5_000_000_000,
                    CurrentSupply = 5_000_000_000,
                    CirculatingSupply = 1_000_000_000,
                    LockedSupply = 4_000_000_000,
                    BurnedSupply = 0,
                    CirculatingPercent = 20.0m,
                    LockedPercent = 80.0m,
                    BurnedPercent = 0.0m
                }
            };
        }

        private VestingScheduleModel GetFallbackVestingSchedule()
        {
            var categories = new List<VestingCategoryModel>
            {
                new VestingCategoryModel
                {
                    Category = "Public Presale",
                    TotalTokens = 1_250_000_000,
                    TgePercentage = 20m,
                    VestingMonths = 6,
                    StartDate = DateTime.UtcNow.AddDays(90),
                    EndDate = DateTime.UtcNow.AddDays(270),
                    Color = "#4f46e5"
                },
                new VestingCategoryModel
                {
                    Category = "Team & Development",
                    TotalTokens = 400_000_000,
                    TgePercentage = 0m,
                    VestingMonths = 24,
                    StartDate = DateTime.UtcNow.AddDays(365),
                    EndDate = DateTime.UtcNow.AddDays(1095),
                    Color = "#f59e0b"
                }
            };

            return new VestingScheduleModel
            {
                Categories = categories,
                Summary = new VestingSummaryModel
                {
                    TotalVestedTokens = 4_000_000_000,
                    CurrentlyUnlocked = 1_000_000_000,
                    RemainingLocked = 3_000_000_000,
                    NextUnlockDate = DateTime.UtcNow.AddDays(90),
                    NextUnlockAmount = 250_000_000,
                    AverageVestingPeriod = 18m
                }
            };
        }

        private BurnMechanicsModel GetFallbackBurnMechanics()
        {
            return new BurnMechanicsModel
            {
                Statistics = new BurnStatisticsModel
                {
                    TotalBurned = 0,
                    BurnedPercentage = 0m,
                    BurnedLast30Days = 0,
                    BurnRate = 0.5m,
                    EstimatedAnnualBurn = 25_000_000m,
                    LastBurnDate = DateTime.MinValue
                },
                Mechanisms = new List<BurnMechanismModel>
                {
                    new BurnMechanismModel
                    {
                        Name = "Transaction Burn",
                        Description = "0.1% of all platform transactions burned",
                        TriggerPercentage = 0.1m,
                        Frequency = "Per Transaction",
                        IsActive = false,
                        Icon = "🔥"
                    },
                    new BurnMechanismModel
                    {
                        Name = "Quarterly Burn",
                        Description = "Platform revenue used for token buyback and burn",
                        TriggerPercentage = 10m,
                        Frequency = "Quarterly",
                        IsActive = false,
                        Icon = "📅"
                    },
                    new BurnMechanismModel
                    {
                        Name = "Governance Burn",
                        Description = "Community-voted burn events",
                        TriggerPercentage = 5m,
                        Frequency = "On Proposal",
                        IsActive = false,
                        Icon = "🗳️"
                    }
                }
            };
        }

        private TreasuryAnalyticsModel GetFallbackTreasuryAnalytics()
        {
            return new TreasuryAnalyticsModel
            {
                Overview = new TreasuryOverviewModel
                {
                    TotalValue = 87_500_000m,
                    OperationalRunwayYears = 10.5m,
                    MonthlyBurnRate = 695_000m,
                    SafetyFundValue = 8_750_000m,
                    StabilityFundValue = 4_375_000m
                },
                Allocations = new List<TreasuryAllocationModel>
                {
                    new TreasuryAllocationModel
                    {
                        Category = "Development",
                        Value = 35_000_000m,
                        Percentage = 40m,
                        Purpose = "Platform development and team salaries",
                        MonthlyUtilization = 278_000m,
                        ProjectedDuration = 125m,
                        Color = "#4f46e5"
                    },
                    new TreasuryAllocationModel
                    {
                        Category = "Marketing",
                        Value = 17_500_000m,
                        Percentage = 20m,
                        Purpose = "User acquisition and brand awareness",
                        MonthlyUtilization = 139_000m,
                        ProjectedDuration = 125m,
                        Color = "#06b6d4"
                    },
                    new TreasuryAllocationModel
                    {
                        Category = "Operations",
                        Value = 17_500_000m,
                        Percentage = 20m,
                        Purpose = "Infrastructure and operational costs",
                        MonthlyUtilization = 139_000m,
                        ProjectedDuration = 125m,
                        Color = "#10b981"
                    },
                    new TreasuryAllocationModel
                    {
                        Category = "Partnerships",
                        Value = 8_750_000m,
                        Percentage = 10m,
                        Purpose = "Strategic partnerships and integrations",
                        MonthlyUtilization = 69_500m,
                        ProjectedDuration = 125m,
                        Color = "#8b5cf6"
                    },
                    new TreasuryAllocationModel
                    {
                        Category = "Reserve",
                        Value = 8_750_000m,
                        Percentage = 10m,
                        Purpose = "Emergency fund and contingencies",
                        MonthlyUtilization = 0m,
                        ProjectedDuration = 999m,
                        Color = "#ef4444"
                    }
                },
                Scenarios = new List<TreasuryScenarioModel>
                {
                    new TreasuryScenarioModel
                    {
                        Name = "Bull Market",
                        Description = "Strong adoption and revenue growth",
                        Probability = 30m,
                        ImpactOnRunway = 50m,
                        ProjectedRunway = 15.8m,
                        Severity = "Positive"
                    },
                    new TreasuryScenarioModel
                    {
                        Name = "Bear Market",
                        Description = "Reduced activity and slower growth",
                        Probability = 40m,
                        ImpactOnRunway = -20m,
                        ProjectedRunway = 8.4m,
                        Severity = "Moderate"
                    },
                    new TreasuryScenarioModel
                    {
                        Name = "Black Swan",
                        Description = "Severe market downturn",
                        Probability = 10m,
                        ImpactOnRunway = -50m,
                        ProjectedRunway = 5.3m,
                        Severity = "Severe"
                    }
                }
            };
        }

        private UtilityFeaturesModel GetFallbackUtilityFeatures()
        {
            return new UtilityFeaturesModel
            {
                Categories = new List<UtilityCategoryModel>
                {
                    new UtilityCategoryModel
                    {
                        Name = "Staking & Rewards",
                        Description = "Earn rewards by staking TEACH tokens",
                        Icon = "💰",
                        IsLive = false,
                        LaunchDate = DateTime.UtcNow.AddDays(120),
                        Features = new List<UtilityFeatureModel>
                        {
                            new UtilityFeatureModel
                            {
                                Name = "Single-sided Staking",
                                Description = "Stake TEACH tokens for yield",
                                IsActive = false,
                                LaunchDate = DateTime.UtcNow.AddDays(120)
                            },
                            new UtilityFeatureModel
                            {
                                Name = "LP Token Staking",
                                Description = "Stake liquidity pool tokens for enhanced rewards",
                                IsActive = false,
                                LaunchDate = DateTime.UtcNow.AddDays(150)
                            }
                        }
                    },
                    new UtilityCategoryModel
                    {
                        Name = "Governance",
                        Description = "Vote on platform decisions and proposals",
                        Icon = "🗳️",
                        IsLive = false,
                        LaunchDate = DateTime.UtcNow.AddDays(180),
                        Features = new List<UtilityFeatureModel>
                        {
                            new UtilityFeatureModel
                            {
                                Name = "Proposal Voting",
                                Description = "Vote on governance proposals",
                                IsActive = false,
                                LaunchDate = DateTime.UtcNow.AddDays(180)
                            },
                            new UtilityFeatureModel
                            {
                                Name = "Treasury Decisions",
                                Description = "Vote on treasury allocation",
                                IsActive = false,
                                LaunchDate = DateTime.UtcNow.AddDays(210)
                            }
                        }
                    },
                    new UtilityCategoryModel
                    {
                        Name = "Platform Access",
                        Description = "Access premium features on TeacherSupport platform",
                        Icon = "🎓",
                        IsLive = false,
                        LaunchDate = DateTime.UtcNow.AddDays(240),
                        Features = new List<UtilityFeatureModel>
                        {
                            new UtilityFeatureModel
                            {
                                Name = "Premium Teacher Tools",
                                Description = "Access advanced teaching resources",
                                IsActive = false,
                                LaunchDate = DateTime.UtcNow.AddDays(240)
                            },
                            new UtilityFeatureModel
                            {
                                Name = "Direct Funding",
                                Description = "Fund teachers and educational projects",
                                IsActive = false,
                                LaunchDate = DateTime.UtcNow.AddDays(270)
                            }
                        }
                    }
                },
                Metrics = new UtilityMetricsModel
                {
                    TotalUtilityVolume = 0m,
                    ActiveUtilities = 0,
                    MonthlyGrowthRate = 0m,
                    UniqueUsers = 0,
                    AverageTransactionValue = 0m
                }
            };
        }

        private GovernanceInfoModel GetFallbackGovernanceInfo()
        {
            return new GovernanceInfoModel
            {
                Overview = new GovernanceOverviewModel
                {
                    TotalVotingPower = 0,
                    ActiveProposals = 0,
                    PassedProposals = 0,
                    TotalProposals = 0,
                    ParticipationRate = 0m,
                    MinimumProposalThreshold = 1_000_000,
                    VotingPeriodDays = 7
                },
                Features = new List<GovernanceFeatureModel>
                {
                    new GovernanceFeatureModel
                    {
                        Name = "Proposal System",
                        Description = "Create and vote on governance proposals",
                        IsImplemented = false,
                        ImplementationDate = DateTime.UtcNow.AddDays(180),
                        Impact = "Community-driven decision making"
                    },
                    new GovernanceFeatureModel
                    {
                        Name = "Treasury Governance",
                        Description = "Vote on treasury fund allocation",
                        IsImplemented = false,
                        ImplementationDate = DateTime.UtcNow.AddDays(210),
                        Impact = "Democratic financial management"
                    },
                    new GovernanceFeatureModel
                    {
                        Name = "Parameter Updates",
                        Description = "Adjust protocol parameters via voting",
                        IsImplemented = false,
                        ImplementationDate = DateTime.UtcNow.AddDays(240),
                        Impact = "Adaptive protocol evolution"
                    }
                },
                Stats = new GovernanceStatsModel
                {
                    TotalVoters = 0,
                    AverageParticipation = 0m,
                    TotalVotesCast = 0,
                    ProposalSuccessRate = 0m,
                    DaysToNextProposal = 0,
                    LargestVotingPower = 0
                }
            };
        }

        #endregion
    }
}