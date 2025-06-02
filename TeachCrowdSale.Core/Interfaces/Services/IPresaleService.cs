using TeachCrowdSale.Core.Models;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Core.Interfaces.Services;

public interface IPresaleService
{
    Task<SaleTier> GetCurrentTierAsync();
    Task<List<SaleTier>> GetAllTiersAsync();
    Task<DateTime> GetTierEndTimeAsync(int tierId);
    Task<PresaleStats> GetPresaleStatsAsync();
    Task<UserBalanceModel> GetUserPurchaseAsync(string address);
    Task<decimal> GetClaimableTokensAsync(string address);
    Task<VestingMilestoneResponse> GetNextVestingMilestoneAsync(string address);
    Task<bool> PurchaseTokensAsync(string address, int tierId, decimal amount);
    Task<bool> ClaimTokensAsync(string address);
}