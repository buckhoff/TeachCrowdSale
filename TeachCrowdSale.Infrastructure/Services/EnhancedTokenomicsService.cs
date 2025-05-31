using System.Numerics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Interfaces.Repositories;
using TeachCrowdSale.Core.Models.Burning;
using TeachCrowdSale.Core.Models.Governance;
using TeachCrowdSale.Core.Models.Tokenomics;
using TeachCrowdSale.Core.Models.Treasury;
using TeachCrowdSale.Core.Models.Utility;
using TeachCrowdSale.Core.Models.Vesting;

namespace TeachCrowdSale.Infrastructure.Services
{
    public class EnhancedTokenomicsService : ITokenomicsService
    {
        private readonly ILogger<EnhancedTokenomicsService> _logger;
        private readonly IBlockchainService _blockchainService;
        private readonly ITokenContractService _tokenService;
        private readonly IMemoryCache _cache;

        // Repository dependencies
        private readonly ITokenMetricsRepository _metricsRepository;
        private readonly ISupplyRepository _supplyRepository;
        private readonly IVestingRepository _vestingRepository;
        private readonly ITreasuryRepository _treasuryRepository;
        private readonly IBurnRepository _burnRepository;
        private readonly IGovernanceRepository _governanceRepository;

        // Cache keys
        private const string CACHE_KEY_TOKENOMICS_DATA = "tokenomics_comprehensive_data";
        private const string CACHE_KEY_LIVE_METRICS = "tokenomics_live_metrics";
        private const string CACHE_KEY_SUPPLY_BREAKDOWN = "tokenomics_supply_breakdown";
        private const string CACHE_KEY_VESTING_SCHEDULE = "tokenomics_vesting_schedule";
        private const string CACHE_KEY_BURN_MECHANICS = "tokenomics_burn_mechanics";
        private const string CACHE_KEY_TREASURY_ANALYTICS = "tokenomics_treasury_analytics";
        private const string CACHE_KEY_UTILITY_FEATURES = "tokenomics_utility_features";
        private const string CACHE_KEY_GOVERNANCE_INFO = "tokenomics_governance_info";

        // Cache durations
        private readonly TimeSpan _liveCacheDuration = TimeSpan.FromMinutes(1);
        private readonly TimeSpan _shortCacheDuration = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _mediumCacheDuration = TimeSpan.FromMinutes(15);
        private readonly TimeSpan _longCacheDuration = TimeSpan.FromMinutes(30);

        public EnhancedTokenomicsService(
            ILogger<EnhancedTokenomicsService> logger,
            IBlockchainService blockchainService,
            ITokenContractService tokenService,
            IMemoryCache cache,
            ITokenMetricsRepository metricsRepository,
            ISupplyRepository supplyRepository,
            IVestingRepository vestingRepository,
            ITreasuryRepository treasuryRepository,
            IBurnRepository burnRepository,
            IGovernanceRepository governanceRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _blockchainService = blockchainService ?? throw new ArgumentNullException(nameof(blockchainService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _metricsRepository = metricsRepository ?? throw new ArgumentNullException(nameof(metricsRepository));
            _supplyRepository = supplyRepository ?? throw new ArgumentNullException(nameof(supplyRepository));
            _vestingRepository = vestingRepository ?? throw new ArgumentNullException(nameof(vestingRepository));
            _treasuryRepository = treasuryRepository ?? throw new ArgumentNullException(nameof(treasuryRepository));
            _burnRepository = burnRepository ?? throw new ArgumentNullException(nameof(burnRepository));
            _governanceRepository = governanceRepository ?? throw new ArgumentNullException(nameof(governanceRepository));
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

                // Fetch all components in parallel
                var liveMetricsTask = GetLiveTokenMetricsAsync();
                var supplyBreakdownTask = GetSupplyBreakdownAsync();
                var vestingScheduleTask = GetVestingScheduleAsync();
                var burnMechanicsTask = GetBurnMechanicsAsync();
                var treasuryAnalyticsTask = GetTreasuryAnalyticsAsync();
                var utilityFeaturesTask = GetUtilityFeaturesAsync();
                var governanceInfoTask = GetGovernanceInfoAsync();

                await Task.WhenAll(
                    liveMetricsTask,
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

                // Try to get from database first
                var dbMetrics = await _metricsRepository.GetLatestMetricsAsync();
                if (dbMetrics != null && (DateTime.UtcNow - dbMetrics.Timestamp).TotalMinutes < 5)
                {
                    var metrics = MapToLiveTokenMetricsModel(dbMetrics);
                    _cache.Set(CACHE_KEY_LIVE_METRICS, metrics, _liveCacheDuration);
                    return metrics;
                }

                // Fetch fresh data from blockchain and DEX
                var freshMetrics = await FetchLiveMetricsFromSources();

                // Save to database
                if (freshMetrics != null)
                {
                    var snapshot = MapToTokenMetricsSnapshot(freshMetrics);
                    await _metricsRepository.SaveMetricsSnapshotAsync(snapshot);
                }

                var result = freshMetrics ?? GetFallbackLiveMetrics();
                _cache.Set(CACHE_KEY_LIVE_METRICS, result, _liveCacheDuration);
                return result;
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

                // Get from database first
                var dbSupply = await _supplyRepository.GetLatestSupplySnapshotAsync();
                var dbAllocations = await _supplyRepository.GetTokenAllocationsAsync();

                if (dbSupply != null && dbAllocations.Any() && (DateTime.UtcNow - dbSupply.Timestamp).TotalMinutes < 30)
                {
                    var supply = MapToSupplyBreakdownModel(dbSupply, dbAllocations);
                    _cache.Set(CACHE_KEY_SUPPLY_BREAKDOWN, supply, _longCacheDuration);
                    return supply;
                }

                // Fetch fresh data from blockchain
                var freshSupply = await FetchSupplyDataFromBlockchain();

                // Save to database if fresh data available
                if (freshSupply != null)
                {
                    var snapshot = MapToSupplySnapshot(freshSupply);
                    await _supplyRepository.SaveSupplySnapshotAsync(snapshot);
                }

                // If no allocations in DB, create default ones
                if (!dbAllocations.Any())
                {
                    await CreateDefaultTokenAllocations();
                    dbAllocations = await _supplyRepository.GetTokenAllocationsAsync();
                }

                var result = freshSupply ?? MapToSupplyBreakdownModel(dbSupply, dbAllocations) ?? GetFallbackSupplyBreakdown();
                _cache.Set(CACHE_KEY_SUPPLY_BREAKDOWN, result, _longCacheDuration);
                return result;
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

                // Get from database
                var dbCategories = await _vestingRepository.GetVestingCategoriesAsync();
                var dbMilestones = await _vestingRepository.GetVestingMilestonesAsync();

                if (dbCategories.Any())
                {
                    var vesting = MapToVestingScheduleModel(dbCategories, dbMilestones);
                    _cache.Set(CACHE_KEY_VESTING_SCHEDULE, vesting, _longCacheDuration);
                    return vesting;
                }

                // Create default vesting schedule if none exists
                await CreateDefaultVestingSchedule();
                dbCategories = await _vestingRepository.GetVestingCategoriesAsync();
                dbMilestones = await _vestingRepository.GetVestingMilestonesAsync();

                var result = MapToVestingScheduleModel(dbCategories, dbMilestones);
                _cache.Set(CACHE_KEY_VESTING_SCHEDULE, result, _longCacheDuration);
                return result;
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

                // Get from database
                var dbSnapshot = await _burnRepository.GetLatestBurnSnapshotAsync();
                var dbMechanisms = await _burnRepository.GetBurnMechanismsAsync();
                var dbEvents = await _burnRepository.GetBurnEventsAsync(DateTime.UtcNow.AddDays(-30));

                // Try to get fresh burn data from blockchain
                var freshBurnData = await FetchBurnDataFromBlockchain();

                if (freshBurnData != null)
                {
                    var snapshot = MapToBurnSnapshot(freshBurnData);
                    await _burnRepository.SaveBurnSnapshotAsync(snapshot);
                    dbSnapshot = snapshot;
                }

                // Create default mechanisms if none exist
                if (!dbMechanisms.Any())
                {
                    await CreateDefaultBurnMechanisms();
                    dbMechanisms = await _burnRepository.GetBurnMechanismsAsync();
                }

                var result = MapToBurnMechanicsModel(dbSnapshot, dbMechanisms, dbEvents);
                _cache.Set(CACHE_KEY_BURN_MECHANICS, result, _mediumCacheDuration);
                return result;
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

                // Get from database
                var dbSnapshot = await _treasuryRepository.GetLatestTreasurySnapshotAsync();
                var dbAllocations = await _treasuryRepository.GetTreasuryAllocationsAsync();
                var dbTransactions = await _treasuryRepository.GetTreasuryTransactionsAsync(DateTime.UtcNow.AddDays(-30));

                // Try to fetch fresh treasury data from blockchain
                var freshTreasuryData = await FetchTreasuryDataFromBlockchain();

                if (freshTreasu
                    private BurnMechanicsModel MapToBurnMechanicsModel(BurnSnapshot? snapshot, List<BurnMechanism> mechanisms, List<BurnEvent> events)
                    {
                        var statistics = snapshot != null ? new BurnStatisticsModel
                        {
                            TotalBurned = snapshot.TotalBurned,
                            BurnedPercentage = snapshot.BurnedPercentage,
                            BurnedLast30Days = snapshot.BurnedLast30Days,
                            BurnRate = snapshot.BurnRate,
                            EstimatedAnnualBurn = snapshot.EstimatedAnnualBurn,
                            LastBurnDate = snapshot.LastBurnDate
                        } : new BurnStatisticsModel();

                        var mappedMechanisms = mechanisms.Select(m => new BurnMechanismModel
                        {
                            Name = m.Name,
                            Description = m.Description,
                            TriggerPercentage = m.TriggerPercentage,
                            Frequency = m.Frequency,
                            IsActive = m.IsActive,
                            HistoricalBurns = m.HistoricalBurns,
                            Icon = m.Icon
                        }).ToList();

                        var recentBurns = events.TakeLast(10).Select(e => new BurnEventModel
                        {
                            Date = e.Date,
                            Amount = e.Amount,
                            Mechanism = e.Mechanism,
                            TransactionHash = e.TransactionHash,
                            UsdValue = e.UsdValue
                        }).ToList();

                        return new BurnMechanicsModel
                        {
                            Statistics = statistics,
                            Mechanisms = mappedMechanisms,
                            RecentBurns = recentBurns,
                            Projections = GenerateBurnProjections()
                        };
                    }

        private BurnSnapshot MapToBurnSnapshot(BurnMechanicsModel model)
        {
            return new BurnSnapshot
            {
                TotalBurned = model.Statistics.TotalBurned,
                BurnedPercentage = model.Statistics.BurnedPercentage,
                BurnedLast30Days = model.Statistics.BurnedLast30Days,
                BurnRate = model.Statistics.BurnRate,
                EstimatedAnnualBurn = model.Statistics.EstimatedAnnualBurn,
                LastBurnDate = model.Statistics.LastBurnDate
            };
        }

        private TreasuryAnalyticsModel MapToTreasuryAnalyticsModel(TreasurySnapshot? snapshot, List<TreasuryAllocation> allocations, List<TreasuryTransaction> transactions)
        {
            var overview = snapshot != null ? new TreasuryOverviewModel
            {
                TotalValue = snapshot.TotalValue,
                OperationalRunwayYears = snapshot.OperationalRunwayYears,
                MonthlyBurnRate = snapshot.MonthlyBurnRate,
                SafetyFundValue = snapshot.SafetyFundValue,
                StabilityFundValue = snapshot.StabilityFundValue,
                LastUpdate = snapshot.Timestamp
            } : GetFallbackTreasuryOverview();

            var mappedAllocations = allocations.Select(a => new TreasuryAllocationModel
            {
                Category = a.Category,
                Value = a.Value,
                Percentage = a.Percentage,
                Purpose = a.Purpose,
                MonthlyUtilization = a.MonthlyUtilization,
                ProjectedDuration = a.ProjectedDuration,
                Color = a.Color
            }).ToList();

            var performance = new TreasuryPerformanceModel
            {
                EfficiencyRating = CalculateEfficiencyRating(transactions),
                CostPerUser = CalculateCostPerUser(transactions),
                RevenueGrowthRate = CalculateRevenueGrowthRate(transactions),
                SustainabilityScore = CalculateSustainabilityScore(overview),
                Trends = GenerateTreasuryTrends(transactions)
            };

            var scenarios = GenerateTreasuryScenarios(overview);

            return new TreasuryAnalyticsModel
            {
                Overview = overview,
                Allocations = mappedAllocations,
                Performance = performance,
                Scenarios = scenarios
            };
        }

        private TreasurySnapshot MapToTreasurySnapshot(TreasuryAnalyticsModel model)
        {
            return new TreasurySnapshot
            {
                TotalValue = model.Overview.TotalValue,
                OperationalRunwayYears = model.Overview.OperationalRunwayYears,
                MonthlyBurnRate = model.Overview.MonthlyBurnRate,
                SafetyFundValue = model.Overview.SafetyFundValue,
                StabilityFundValue = model.Overview.StabilityFundValue,
                Source = "BLOCKCHAIN"
            };
        }

        private UtilityMetricsSnapshot MapToUtilityMetricsSnapshot(UtilityFeaturesModel model)
        {
            return new UtilityMetricsSnapshot
            {
                TotalUtilityVolume = model.Metrics.TotalUtilityVolume,
                ActiveUtilities = model.Metrics.ActiveUtilities,
                MonthlyGrowthRate = model.Metrics.MonthlyGrowthRate,
                UniqueUsers = model.Metrics.UniqueUsers,
                AverageTransactionValue = model.Metrics.AverageTransactionValue,
                Source = "PLATFORM"
            };
        }

        private GovernanceInfoModel MapToGovernanceInfoModel(List<GovernanceProposal> activeProposals, List<GovernanceProposal> recentProposals, GovernanceInfoModel? blockchainData)
        {
            var overview = blockchainData?.Overview ?? new GovernanceOverviewModel
            {
                TotalVotingPower = 0,
                ActiveProposals = activeProposals.Count,
                PassedProposals = recentProposals.Count(p => p.Status == "PASSED"),
                TotalProposals = recentProposals.Count,
                ParticipationRate = 0m,
                MinimumProposalThreshold = 1_000_000,
                VotingPeriodDays = 7
            };

            var mappedActiveProposals = activeProposals.Select(p => new ProposalModel
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Status = p.Status,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                VotesFor = p.VotesFor,
                VotesAgainst = p.VotesAgainst,
                ParticipationRate = p.ParticipationRate,
                Category = p.Category,
                ProposerAddress = p.ProposerAddress
            }).ToList();

            var mappedRecentProposals = recentProposals.Take(10).Select(p => new ProposalModel
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Status = p.Status,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                VotesFor = p.VotesFor,
                VotesAgainst = p.VotesAgainst,
                ParticipationRate = p.ParticipationRate,
                Category = p.Category,
                ProposerAddress = p.ProposerAddress
            }).ToList();

            var features = GetGovernanceFeatures();
            var stats = CalculateGovernanceStats(recentProposals);

            return new GovernanceInfoModel
            {
                Overview = overview,
                ActiveProposals = mappedActiveProposals,
                RecentProposals = mappedRecentProposals,
                Features = features,
                Stats = stats
            };
        }

#endregion

        #region Helper Methods

        private decimal ConvertFromWei(BigInteger wei, int decimals)
        {
            return (decimal)wei / (decimal)Math.Pow(10, decimals);
        }

        private async Task<List<BurnEvent>> GetRecentBurnEventsFromBlockchain()
        {
            try
            {
                // This would parse blockchain events for burn transactions
                // For now, return empty list as burn mechanisms aren't active yet
                return new List<BurnEvent>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching burn events from blockchain");
                return new List<BurnEvent>();
            }
        }

        private decimal CalculateBurnRate(List<BurnEvent> burnEvents)
        {
            if (!burnEvents.Any()) return 0;

            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var recentBurns = burnEvents.Where(b => b.Date >= thirtyDaysAgo).ToList();

            if (!recentBurns.Any()) return 0;

            var totalBurned = recentBurns.Sum(b => b.Amount);
            var days = (DateTime.UtcNow - recentBurns.Min(b => b.Date)).TotalDays;

            return days > 0 ? (decimal)(totalBurned / (decimal)days) : 0;
        }

        private decimal EstimateAnnualBurn(List<BurnEvent> burnEvents)
        {
            var dailyRate = CalculateBurnRate(burnEvents);
            return dailyRate * 365;
        }

        private async Task<decimal> CalculateMonthlyBurnRate()
        {
            try
            {
                // This would calculate actual operational burn rate
                // For now, return estimated value
                return 695_000m; // $695k monthly operational costs
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calculating monthly burn rate");
                return 695_000m;
            }
        }

        private BurnProjectionModel GenerateBurnProjections()
        {
            var projectionData = new List<BurnProjectionDataPoint>();
            var currentDate = DateTime.UtcNow;
            var monthlyBurn = 2_083_333L; // ~25M tokens per year / 12
            var cumulativeBurn = 0L;

            for (int month = 0; month < 60; month++) // 5 years
            {
                var date = currentDate.AddMonths(month);
                cumulativeBurn += monthlyBurn;

                projectionData.Add(new BurnProjectionDataPoint
                {
                    Date = date,
                    ProjectedBurn = monthlyBurn,
                    RemainingSupply = 5_000_000_000L - cumulativeBurn
                });
            }

            return new BurnProjectionModel
            {
                ProjectedAnnualBurn = 25_000_000L,
                ProjectedTotalBurn5Years = 125_000_000L,
                EstimatedSupplyReduction = 2.5m, // 2.5% over 5 years
                ProjectionData = projectionData
            };
        }

        private TreasuryOverviewModel GetFallbackTreasuryOverview()
        {
            return new TreasuryOverviewModel
            {
                TotalValue = 87_500_000m,
                OperationalRunwayYears = 10.5m,
                MonthlyBurnRate = 695_000m,
                SafetyFundValue = 8_750_000m,
                StabilityFundValue = 4_375_000m,
                LastUpdate = DateTime.UtcNow
            };
        }

        private decimal CalculateEfficiencyRating(List<TreasuryTransaction> transactions)
        {
            // Simple efficiency calculation based on transaction patterns
            return 85.5m; // Mock value
        }

        private decimal CalculateCostPerUser(List<TreasuryTransaction> transactions)
        {
            var totalCosts = transactions.Where(t => t.TransactionType == "OUTFLOW").Sum(t => t.Amount);
            var estimatedUsers = 50_000m; // Mock active user count
            return totalCosts / estimatedUsers;
        }

        private decimal CalculateRevenueGrowthRate(List<TreasuryTransaction> transactions)
        {
            // Calculate month-over-month revenue growth
            return 12.5m; // Mock growth rate
        }

        private decimal CalculateSustainabilityScore(TreasuryOverviewModel overview)
        {
            // Score based on runway years, fund diversification, etc.
            var runwayScore = Math.Min(overview.OperationalRunwayYears * 10, 100);
            return runwayScore;
        }

        private List<TreasuryMetricTrend> GenerateTreasuryTrends(List<TreasuryTransaction> transactions)
        {
            // Generate trend data for the last 12 months
            var trends = new List<TreasuryMetricTrend>();
            var currentDate = DateTime.UtcNow;

            for (int month = 11; month >= 0; month--)
            {
                var date = currentDate.AddMonths(-month);
                trends.Add(new TreasuryMetricTrend
                {
                    Date = date,
                    Value = 87_500_000m - (month * 695_000m), // Decreasing treasury value
                    Metric = "TotalValue"
                });
            }

            return trends;
        }

        private List<TreasuryScenarioModel> GenerateTreasuryScenarios(TreasuryOverviewModel overview)
        {
            return new List<TreasuryScenarioModel>
            {
                new TreasuryScenarioModel
                {
                    Name = "Bull Market",
                    Description = "Strong adoption and revenue growth",
                    Probability = 30m,
                    ImpactOnRunway = 50m,
                    ProjectedRunway = overview.OperationalRunwayYears * 1.5m,
                    Severity = "Positive",
                    Mitigations = new List<string> { "Scale operations", "Increase hiring", "Expand partnerships" }
                },
                new TreasuryScenarioModel
                {
                    Name = "Bear Market",
                    Description = "Reduced activity and slower growth",
                    Probability = 40m,
                    ImpactOnRunway = -20m,
                    ProjectedRunway = overview.OperationalRunwayYears * 0.8m,
                    Severity = "Moderate",
                    Mitigations = new List<string> { "Reduce spending", "Focus on core features", "Extend runway" }
                },
                new TreasuryScenarioModel
                {
                    Name = "Black Swan",
                    Description = "Severe market downturn or regulatory issues",
                    Probability = 10m,
                    ImpactOnRunway = -50m,
                    ProjectedRunway = overview.OperationalRunwayYears * 0.5m,
                    Severity = "Severe",
                    Mitigations = new List<string> { "Emergency measures", "Access reserve funds", "Community support" }
                }
            };
        }

        private List<GovernanceFeatureModel> GetGovernanceFeatures()
        {
            return new List<GovernanceFeatureModel>
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
            };
        }

        private GovernanceStatsModel CalculateGovernanceStats(List<GovernanceProposal> proposals)
        {
            return new GovernanceStatsModel
            {
                TotalVoters = proposals.SelectMany(p => new[] { p.VotesFor, p.VotesAgainst }).Sum(),
                AverageParticipation = proposals.Any() ? proposals.Average(p => p.ParticipationRate) : 0,
                TotalVotesCast = proposals.Sum(p => p.VotesFor + p.VotesAgainst),
                ProposalSuccessRate = proposals.Any() ?
                    (decimal)proposals.Count(p => p.Status == "PASSED") / proposals.Count * 100 : 0,
                DaysToNextProposal = 0,
                LargestVotingPower = proposals.Any() ?
                    proposals.Max(p => Math.Max(p.VotesFor, p.VotesAgainst)) : 0
            };
        }

        #endregion

        #region Fallback Data

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
                MarketCap = 325_000_000m,
                Volume24h = 2_500_000m,
                PriceChange24h = 0.003m,
                PriceChangePercent24h = 4.8m,
                High24h = 0.068m,
                Low24h = 0.061m,
                TotalSupply = 5_000_000_000,
                CirculatingSupply = 1_000_000_000,
                HoldersCount = 3247,
                TotalValueLocked = 15_000_000m
            };
        }

        private SupplyBreakdownModel GetFallbackSupplyBreakdown()
        {
            var allocations = new List<SupplyAllocationModel>
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
            };

            var metrics = new SupplyMetricsModel
            {
                MaxSupply = 5_000_000_000,
                CurrentSupply = 5_000_000_000,
                CirculatingSupply = 1_000_000_000,
                LockedSupply = 4_000_000_000,
                BurnedSupply = 0,
                CirculatingPercent = 20.0m,
                LockedPercent = 80.0m,
                BurnedPercent = 0.0m
            };

            return new SupplyBreakdownModel
            {
                Allocations = allocations,
                Metrics = metrics
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

            var summary = new VestingSummaryModel
            {
                TotalVestedTokens = 4_000_000_000,
                CurrentlyUnlocked = 1_000_000_000,
                RemainingLocked = 3_000_000_000,
                NextUnlockDate = DateTime.UtcNow.AddDays(90),
                NextUnlockAmount = 250_000_000,
                AverageVestingPeriod = 18m
            };

            return new VestingScheduleModel
            {
                Categories = categories,
                Summary = summary
            };
        }

        private BurnMechanicsModel GetFallbackBurnMechanics()
        {
            var statistics = new BurnStatisticsModel
            {
                TotalBurned = 0,
                BurnedPercentage = 0m,
                BurnedLast30Days = 0,
                BurnRate = 0.5m,
                EstimatedAnnualBurn = 25_000_000m,
                LastBurnDate = DateTime.MinValue
            };

            var mechanisms = new List<BurnMechanismModel>
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
            };

            return new BurnMechanicsModel
            {
                Statistics = statistics,
                Mechanisms = mechanisms,
                RecentBurns = new List<BurnEventModel>(),
                Projections = GenerateBurnProjections()
            };
        }

        private TreasuryAnalyticsModel GetFallbackTreasuryAnalytics()
        {
            var overview = GetFallbackTreasuryOverview();

            var allocations = new List<TreasuryAllocationModel>
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
            };

            var performance = new TreasuryPerformanceModel
            {
                EfficiencyRating = 85.5m,
                CostPerUser = 13.9m,
                RevenueGrowthRate = 12.5m,
                SustainabilityScore = 95.2m,
                Trends = GenerateTreasuryTrends(new List<TreasuryTransaction>())
            };

            var scenarios = GenerateTreasuryScenarios(overview);

            return new TreasuryAnalyticsModel
            {
                Overview = overview,
                Allocations = allocations,
                Performance = performance,
                Scenarios = scenarios
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
                },
                Roadmap = new List<UtilityRoadmapModel>
                {
                    new UtilityRoadmapModel
                    {
                        Feature = "Staking Platform",
                        Description = "Launch single-sided and LP token staking",
                        EstimatedLaunch = DateTime.UtcNow.AddDays(120),
                        Status = "In Development",
                        Priority = "High",
                        Benefits = new List<string> { "Token utility", "Rewards generation", "Supply reduction" }
                    },
                    new UtilityRoadmapModel
                    {
                        Feature = "Governance System",
                        Description = "Decentralized governance with proposal and voting",
                        EstimatedLaunch = DateTime.UtcNow.AddDays(180),
                        Status = "Planning",
                        Priority = "High",
                        Benefits = new List<string> { "Community control", "Democratic decisions", "Protocol evolution" }
                    },
                    new UtilityRoadmapModel
                    {
                        Feature = "Education Marketplace",
                        Description = "Direct teacher funding and resource marketplace",
                        EstimatedLaunch = DateTime.UtcNow.AddDays(240),
                        Status = "Design",
                        Priority = "Medium",
                        Benefits = new List<string> { "Real utility", "Direct impact", "Platform adoption" }
                    }
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
                ActiveProposals = new List<ProposalModel>(),
                RecentProposals = new List<ProposalModel>(),
                Features = GetGovernanceFeatures(),
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

        #region Helper Classes

        private class DexAggregatorData
        {
            public decimal Volume24h { get; set; }
            public decimal PriceChange24h { get; set; }
            public decimal PriceChangePercent24h { get; set; }
            public decimal High24h { get; set; }
            public decimal Low24h { get; set; }
        }

        #endregion
    }
}
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
                                using System.Numerics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TeachCrowdSale.Core.Data.Entities.Tokenomics;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Interfaces.Repositories;
using TeachCrowdSale.Core.Models.Burning;
using TeachCrowdSale.Core.Models.Governance;
using TeachCrowdSale.Core.Models.Tokenomics;
using TeachCrowdSale.Core.Models.Treasury;
using TeachCrowdSale.Core.Models.Utility;
using TeachCrowdSale.Core.Models.Vesting;

namespace TeachCrowdSale.Infrastructure.Services
{
    public class EnhancedTokenomicsService : ITokenomicsService
    {
        private readonly ILogger<EnhancedTokenomicsService> _logger;
        private readonly IBlockchainService _blockchainService;
        private readonly ITokenContractService _tokenService;
        private readonly IMemoryCache _cache;

        // Repository dependencies
        private readonly ITokenMetricsRepository _metricsRepository;
        private readonly ISupplyRepository _supplyRepository;
        private readonly IVestingRepository _vestingRepository;
        private readonly ITreasuryRepository _treasuryRepository;
        private readonly IBurnRepository _burnRepository;
        private readonly IGovernanceRepository _governanceRepository;

        // Cache keys
        private const string CACHE_KEY_TOKENOMICS_DATA = "tokenomics_comprehensive_data";
        private const string CACHE_KEY_LIVE_METRICS = "tokenomics_live_metrics";
        private const string CACHE_KEY_SUPPLY_BREAKDOWN = "tokenomics_supply_breakdown";
        private const string CACHE_KEY_VESTING_SCHEDULE = "tokenomics_vesting_schedule";
        private const string CACHE_KEY_BURN_MECHANICS = "tokenomics_burn_mechanics";
        private const string CACHE_KEY_TREASURY_ANALYTICS = "tokenomics_treasury_analytics";
        private const string CACHE_KEY_UTILITY_FEATURES = "tokenomics_utility_features";
        private const string CACHE_KEY_GOVERNANCE_INFO = "tokenomics_governance_info";

        // Cache durations
        private readonly TimeSpan _liveCacheDuration = TimeSpan.FromMinutes(1);
        private readonly TimeSpan _shortCacheDuration = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _mediumCacheDuration = TimeSpan.FromMinutes(15);
        private readonly TimeSpan _longCacheDuration = TimeSpan.FromMinutes(30);

        public EnhancedTokenomicsService(
            ILogger<EnhancedTokenomicsService> logger,
            IBlockchainService blockchainService,
            ITokenContractService tokenService,
            IMemoryCache cache,
            ITokenMetricsRepository metricsRepository,
            ISupplyRepository supplyRepository,
            IVestingRepository vestingRepository,
            ITreasuryRepository treasuryRepository,
            IBurnRepository burnRepository,
            IGovernanceRepository governanceRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _blockchainService = blockchainService ?? throw new ArgumentNullException(nameof(blockchainService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _metricsRepository = metricsRepository ?? throw new ArgumentNullException(nameof(metricsRepository));
            _supplyRepository = supplyRepository ?? throw new ArgumentNullException(nameof(supplyRepository));
            _vestingRepository = vestingRepository ?? throw new ArgumentNullException(nameof(vestingRepository));
            _treasuryRepository = treasuryRepository ?? throw new ArgumentNullException(nameof(treasuryRepository));
            _burnRepository = burnRepository ?? throw new ArgumentNullException(nameof(burnRepository));
            _governanceRepository = governanceRepository ?? throw new ArgumentNullException(nameof(governanceRepository));
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

                // Fetch all components in parallel
                var liveMetricsTask = GetLiveTokenMetricsAsync();
                var supplyBreakdownTask = GetSupplyBreakdownAsync();
                var vestingScheduleTask = GetVestingScheduleAsync();
                var burnMechanicsTask = GetBurnMechanicsAsync();
                var treasuryAnalyticsTask = GetTreasuryAnalyticsAsync();
                var utilityFeaturesTask = GetUtilityFeaturesAsync();
                var governanceInfoTask = GetGovernanceInfoAsync();

                await Task.WhenAll(
                    liveMetricsTask,
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

                // Try to get from database first
                var dbMetrics = await _metricsRepository.GetLatestMetricsAsync();
                if (dbMetrics != null && (DateTime.UtcNow - dbMetrics.Timestamp).TotalMinutes < 5)
                {
                    var metrics = MapToLiveTokenMetricsModel(dbMetrics);
                    _cache.Set(CACHE_KEY_LIVE_METRICS, metrics, _liveCacheDuration);
                    return metrics;
                }

                // Fetch fresh data from blockchain and DEX
                var freshMetrics = await FetchLiveMetricsFromSources();

                // Save to database
                if (freshMetrics != null)
                {
                    var snapshot = MapToTokenMetricsSnapshot(freshMetrics);
                    await _metricsRepository.SaveMetricsSnapshotAsync(snapshot);
                }

                var result = freshMetrics ?? GetFallbackLiveMetrics();
                _cache.Set(CACHE_KEY_LIVE_METRICS, result, _liveCacheDuration);
                return result;
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

                // Get from database first
                var dbSupply = await _supplyRepository.GetLatestSupplySnapshotAsync();
                var dbAllocations = await _supplyRepository.GetTokenAllocationsAsync();

                if (dbSupply != null && dbAllocations.Any() && (DateTime.UtcNow - dbSupply.Timestamp).TotalMinutes < 30)
                {
                    var supply = MapToSupplyBreakdownModel(dbSupply, dbAllocations);
                    _cache.Set(CACHE_KEY_SUPPLY_BREAKDOWN, supply, _longCacheDuration);
                    return supply;
                }

                // Fetch fresh data from blockchain
                var freshSupply = await FetchSupplyDataFromBlockchain();

                // Save to database if fresh data available
                if (freshSupply != null)
                {
                    var snapshot = MapToSupplySnapshot(freshSupply);
                    await _supplyRepository.SaveSupplySnapshotAsync(snapshot);
                }

                // If no allocations in DB, create default ones
                if (!dbAllocations.Any())
                {
                    await CreateDefaultTokenAllocations();
                    dbAllocations = await _supplyRepository.GetTokenAllocationsAsync();
                }

                var result = freshSupply ?? MapToSupplyBreakdownModel(dbSupply, dbAllocations) ?? GetFallbackSupplyBreakdown();
                _cache.Set(CACHE_KEY_SUPPLY_BREAKDOWN, result, _longCacheDuration);
                return result;
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

                // Get from database
                var dbCategories = await _vestingRepository.GetVestingCategoriesAsync();
                var dbMilestones = await _vestingRepository.GetVestingMilestonesAsync();

                if (dbCategories.Any())
                {
                    var vesting = MapToVestingScheduleModel(dbCategories, dbMilestones);
                    _cache.Set(CACHE_KEY_VESTING_SCHEDULE, vesting, _longCacheDuration);
                    return vesting;
                }

                // Create default vesting schedule if none exists
                await CreateDefaultVestingSchedule();
                dbCategories = await _vestingRepository.GetVestingCategoriesAsync();
                dbMilestones = await _vestingRepository.GetVestingMilestonesAsync();

                var result = MapToVestingScheduleModel(dbCategories, dbMilestones);
                _cache.Set(CACHE_KEY_VESTING_SCHEDULE, result, _longCacheDuration);
                return result;
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

                // Get from database
                var dbSnapshot = await _burnRepository.GetLatestBurnSnapshotAsync();
                var dbMechanisms = await _burnRepository.GetBurnMechanismsAsync();
                var dbEvents = await _burnRepository.GetBurnEventsAsync(DateTime.UtcNow.AddDays(-30));

                // Try to get fresh burn data from blockchain
                var freshBurnData = await FetchBurnDataFromBlockchain();

                if (freshBurnData != null)
                {
                    var snapshot = MapToBurnSnapshot(freshBurnData);
                    await _burnRepository.SaveBurnSnapshotAsync(snapshot);
                    dbSnapshot = snapshot;
                }

                // Create default mechanisms if none exist
                if (!dbMechanisms.Any())
                {
                    await CreateDefaultBurnMechanisms();
                    dbMechanisms = await _burnRepository.GetBurnMechanismsAsync();
                }

                var result = MapToBurnMechanicsModel(dbSnapshot, dbMechanisms, dbEvents);
                _cache.Set(CACHE_KEY_BURN_MECHANICS, result, _mediumCacheDuration);
                return result;
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

                // Get from database
                var dbSnapshot = await _treasuryRepository.GetLatestTreasurySnapshotAsync();
                var dbAllocations = await _treasuryRepository.GetTreasuryAllocationsAsync();
                var dbTransactions = await _treasuryRepository.GetTreasuryTransactionsAsync(DateTime.UtcNow.AddDays(-30));

                // Try to fetch fresh treasury data from blockchain
                var freshTreasuryData = await FetchTreasuryDataFromBlockchain();

                if (freshTreasu