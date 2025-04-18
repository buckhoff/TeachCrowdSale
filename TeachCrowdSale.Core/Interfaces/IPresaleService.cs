using TeachCrowdSale.Core.Models;

namespace TeachCrowdSale.Core.Interfaces;

public interface IPresaleService
{
    Task<SaleTier> GetCurrentTierAsync();
    Task<List<SaleTier>> GetAllTiersAsync();
    Task<DateTime> GetTierEndTimeAsync(int tierId);
    Task<PresaleStats> GetPresaleStatsAsync();
    Task<UserPurchase> GetUserPurchaseAsync(string address);
    Task<decimal> GetClaimableTokensAsync(string address);
    Task<VestingMilestone> GetNextVestingMilestoneAsync(string address);
    Task<bool> PurchaseTokensAsync(string address, int tierId, decimal amount);
    Task<bool> ClaimTokensAsync(string address);
}