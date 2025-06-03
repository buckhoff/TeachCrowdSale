using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Interfaces.Services
{
    /// <summary>
    /// Interface for staking contract interactions
    /// </summary>
    public interface IStakingContractService
    {
        Task<decimal> GetPoolTotalStakedAsync(int poolId);
        Task<decimal> GetUserStakedAmountAsync(string walletAddress, int poolId);
        Task<decimal> GetPendingRewardsAsync(string walletAddress, int stakeId);
        Task<bool> IsPoolActiveAsync(int poolId);
        Task<decimal> GetPoolAPYAsync(int poolId);
        Task<DateTime> GetStakeUnlockTimeAsync(int stakeId);

        // Transaction operations
        Task<string> StakeTokensTransactionAsync(string walletAddress, int poolId, decimal amount);
        Task<string> UnstakeTokensTransactionAsync(string walletAddress, int stakeId);
        Task<string> ClaimRewardsTransactionAsync(string walletAddress, int stakeId);
        Task<string> CompoundRewardsTransactionAsync(string walletAddress, int stakeId);
    }
}
