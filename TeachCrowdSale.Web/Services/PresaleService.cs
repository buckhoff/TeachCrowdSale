using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using TeachCrowdSale.Web.Models;

namespace TeachTokenCrowdsale.Web.Services
{
    public class PresaleService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PresaleService> _logger;
        private readonly IConfiguration _configuration;

        public PresaleService(
            HttpClient httpClient,
            IMemoryCache cache,
            ILogger<PresaleService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<List<PresaleTierModel>> GetAllTiersAsync()
        {
            // Try to get from cache first
            if (_cache.TryGetValue("presale_tiers", out List<PresaleTierModel> tiers))
            {
                return tiers;
            }

            try
            {
                // In a real application, this would make an API call
                // var response = await _httpClient.GetFromJsonAsync<List<PresaleTierModel>>("/api/presale/tiers");

                // For demo purposes, we'll use mock data
                var mockTiers = new List<PresaleTierModel>
                {
                    new PresaleTierModel
                    {
                        Id = 1,
                        Name = "Seed Round",
                        Price = 0.035m,
                        TotalAllocation = 75_000_000,
                        Sold = 75_000_000, // 100% sold
                        MinPurchase = 100,
                        MaxPurchase = 50_000,
                        VestingTGE = 10,
                        VestingMonths = 18,
                        IsActive = false,
                        EndTime = DateTime.UtcNow.AddDays(-30) // Already ended
                    },
                    new PresaleTierModel
                    {
                        Id = 2,
                        Name = "Community Round",
                        Price = 0.045m,
                        TotalAllocation = 100_000_000,
                        Sold = 72_500_000, // 72.5% sold
                        MinPurchase = 100,
                        MaxPurchase = 25_000,
                        VestingTGE = 15,
                        VestingMonths = 15,
                        IsActive = true,
                        EndTime = DateTime.UtcNow.AddDays(14) // Ends in 2 weeks
                    },
                    new PresaleTierModel
                    {
                        Id = 3,
                        Name = "Growth Round",
                        Price = 0.055m,
                        TotalAllocation = 100_000_000,
                        Sold = 0, // Not started yet
                        MinPurchase = 100,
                        MaxPurchase = 10_000,
                        VestingTGE = 20,
                        VestingMonths = 12,
                        IsActive = false,
                        EndTime = DateTime.UtcNow.AddDays(45) // Starts in future
                    },
                    new PresaleTierModel
                    {
                        Id = 4,
                        Name = "Expansion Round",
                        Price = 0.070m,
                        TotalAllocation = 75_000_000,
                        Sold = 0, // Not started yet
                        MinPurchase = 100,
                        MaxPurchase = 5_000,
                        VestingTGE = 20,
                        VestingMonths = 9,
                        IsActive = false,
                        EndTime = DateTime.UtcNow.AddDays(75) // Starts in future
                    },
                    new PresaleTierModel
                    {
                        Id = 5,
                        Name = "Adoption Round",
                        Price = 0.085m,
                        TotalAllocation = 50_000_000,
                        Sold = 0, // Not started yet
                        MinPurchase = 50,
                        MaxPurchase = 5_000,
                        VestingTGE = 25,
                        VestingMonths = 6,
                        IsActive = false,
                        EndTime = DateTime.UtcNow.AddDays(105) // Starts in future
                    },
                    new PresaleTierModel
                    {
                        Id = 6,
                        Name = "Launch Round",
                        Price = 0.100m,
                        TotalAllocation = 50_000_000,
                        Sold = 0, // Not started yet
                        MinPurchase = 20,
                        MaxPurchase = 5_000,
                        VestingTGE = 30,
                        VestingMonths = 4,
                        IsActive = false,
                        EndTime = DateTime.UtcNow.AddDays(135) // Starts in future
                    },
                    new PresaleTierModel
                    {
                        Id = 7,
                        Name = "Final Round",
                        Price = 0.120m,
                        TotalAllocation = 50_000_000,
                        Sold = 0, // Not started yet
                        MinPurchase = 20,
                        MaxPurchase = 5_000,
                        VestingTGE = 40,
                        VestingMonths = 3,
                        IsActive = false,
                        EndTime = DateTime.UtcNow.AddDays(165) // Starts in future
                    }
                };

                // Cache for 5 minutes
                _cache.Set("presale_tiers", mockTiers, TimeSpan.FromMinutes(5));

                return mockTiers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching presale tiers");
                return new List<PresaleTierModel>();
            }
        }

        public async Task<PresaleTierModel> GetCurrentTierAsync()
        {
            try
            {
                var tiers = await GetAllTiersAsync();
                return tiers.FirstOrDefault(t => t.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current tier");
                return null;
            }
        }

        public async Task<PresaleStatsModel> GetPresaleStatsAsync()
        {
            // Try to get from cache first
            if (_cache.TryGetValue("presale_stats", out PresaleStatsModel stats))
            {
                return stats;
            }

            try
            {
                // In a real application, this would make an API call
                // var response = await _httpClient.GetFromJsonAsync<PresaleStatsModel>("/api/presale/status");

                // For demo purposes, we'll use mock data
                var mockStats = new PresaleStatsModel
                {
                    TotalRaised = 3_450_000, // $3.45M raised
                    FundingGoal = 5_000_000, // $5M goal
                    ParticipantsCount = 1_842, // 1,842 participants
                    TokensSold = 75_000_000 + 72_500_000, // Tier 1 + part of Tier 2
                    TokensRemaining = 500_000_000 - (75_000_000 + 72_500_000), // Total allocation - sold
                    PresaleStartTime = DateTime.UtcNow.AddDays(-60),
                    PresaleEndTime = DateTime.UtcNow.AddDays(170),
                    IsActive = true
                };

                // Cache for 5 minutes
                _cache.Set("presale_stats", mockStats, TimeSpan.FromMinutes(5));

                return mockStats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching presale stats");
                return new PresaleStatsModel
                {
                    TotalRaised = 0,
                    FundingGoal = 5_000_000,
                    ParticipantsCount = 0,
                    TokensSold = 0,
                    TokensRemaining = 500_000_000,
                    PresaleStartTime = DateTime.UtcNow,
                    PresaleEndTime = DateTime.UtcNow.AddDays(180),
                    IsActive = false
                };
            }
        }

        public async Task<UserPurchaseModel> GetUserPurchaseAsync(string address)
        {
            try
            {
                // In a real application, this would make an API call
                // var response = await _httpClient.GetFromJsonAsync<UserPurchaseModel>($"/api/presale/user/{address}");

                // For demo purposes, we'll return mock data
                // Normally this would only be returned if the address has made purchases
                return new UserPurchaseModel
                {
                    Address = address,
                    TotalTokens = 25_000,
                    UsdAmount = 1_125, // $1,125 spent at $0.045 per token
                    ClaimableTokens = 3_750, // 15% of 25,000 (TGE portion)
                    TierPurchases = new List<decimal> { 0, 1_125, 0, 0, 0, 0, 0 } // Purchased in Tier 2
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user purchase for address {Address}", address);
                return null;
            }
        }

        public async Task<VestingMilestoneModel> GetNextVestingMilestoneAsync(string address)
        {
            try
            {
                // In a real application, this would make an API call
                // var response = await _httpClient.GetFromJsonAsync<VestingMilestoneModel>($"/api/presale/next-vesting/{address}");

                // For demo purposes, we'll return mock data
                return new VestingMilestoneModel
                {
                    Timestamp = DateTime.UtcNow.AddMonths(1),
                    Amount = 1_428.57m, // Monthly vesting amount (25,000 - 3,750 TGE) / 15 months
                    FormattedDate = DateTime.UtcNow.AddMonths(1).ToString("MMMM dd, yyyy")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching next vesting milestone for address {Address}", address);
                return null;
            }
        }

        public async Task<bool> PurchaseTokensAsync(int tierId, decimal amount)
        {
            try
            {
                // In a real application, this would make an API call
                // var response = await _httpClient.PostAsJsonAsync("/api/presale/purchase", new { TierId = tierId, Amount = amount });
                // return response.IsSuccessStatusCode;

                // For demo purposes, we'll return success
                await Task.Delay(2000); // Simulate API call delay
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error purchasing tokens for tier {TierId} with amount {Amount}", tierId, amount);
                return false;
            }
        }

        public async Task<bool> ClaimTokensAsync()
        {
            try
            {
                // In a real application, this would make an API call
                // var response = await _httpClient.PostAsync("/api/presale/claim", null);
                // return response.IsSuccessStatusCode;

                // For demo purposes, we'll return success
                await Task.Delay(2000); // Simulate API call delay
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error claiming tokens");
                return false;
            }
        }
    }

    
}