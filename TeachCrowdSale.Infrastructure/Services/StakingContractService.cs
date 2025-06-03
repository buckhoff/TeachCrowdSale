using System.Numerics;
using Microsoft.Extensions.Logging;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Interfaces.Services;

namespace TeachCrowdSale.Infrastructure.Services;

/// <summary>
/// Service for interacting with staking smart contracts
/// </summary>
public class StakingContractService : IStakingContractService
{
    private readonly ILogger<StakingContractService> _logger;
    private readonly IBlockchainService _blockchainService;

    public StakingContractService(
        ILogger<StakingContractService> logger,
        IBlockchainService blockchainService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _blockchainService = blockchainService ?? throw new ArgumentNullException(nameof(blockchainService));
    }

    public async Task<decimal> GetPoolTotalStakedAsync(int poolId)
    {
        try
        {
            var contractAddresses = _blockchainService.GetContractAddresses();

            var totalStaked = await _blockchainService.CallContractFunctionAsync<BigInteger>(
                contractAddresses.StakingAddress,
                "getPoolTotalStaked(uint256)",
                poolId);

            return ConvertFromWei(totalStaked, 18);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pool total staked for pool {PoolId}", poolId);
            throw;
        }
    }

    public async Task<decimal> GetUserStakedAmountAsync(string walletAddress, int poolId)
    {
        try
        {
            var contractAddresses = _blockchainService.GetContractAddresses();

            var stakedAmount = await _blockchainService.CallContractFunctionAsync<BigInteger>(
                contractAddresses.StakingAddress,
                "getUserStakedAmount(address,uint256)",
                walletAddress, poolId);

            return ConvertFromWei(stakedAmount, 18);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user staked amount for {WalletAddress} in pool {PoolId}", walletAddress, poolId);
            throw;
        }
    }

    public async Task<decimal> GetPendingRewardsAsync(string walletAddress, int stakeId)
    {
        try
        {
            var contractAddresses = _blockchainService.GetContractAddresses();

            var pendingRewards = await _blockchainService.CallContractFunctionAsync<BigInteger>(
                contractAddresses.StakingAddress,
                "getPendingRewards(address,uint256)",
                walletAddress, stakeId);

            return ConvertFromWei(pendingRewards, 18);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending rewards for {WalletAddress}, stake {StakeId}", walletAddress, stakeId);
            throw;
        }
    }

    public async Task<bool> IsPoolActiveAsync(int poolId)
    {
        try
        {
            var contractAddresses = _blockchainService.GetContractAddresses();

            return await _blockchainService.CallContractFunctionAsync<bool>(
                contractAddresses.StakingAddress,
                "isPoolActive(uint256)",
                poolId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if pool {PoolId} is active", poolId);
            throw;
        }
    }

    public async Task<decimal> GetPoolAPYAsync(int poolId)
    {
        try
        {
            var contractAddresses = _blockchainService.GetContractAddresses();

            var apy = await _blockchainService.CallContractFunctionAsync<BigInteger>(
                contractAddresses.StakingAddress,
                "getPoolAPY(uint256)",
                poolId);

            // APY is returned as percentage with 2 decimals (e.g., 1250 = 12.50%)
            return (decimal)apy / 100m;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting APY for pool {PoolId}", poolId);
            throw;
        }
    }

    public async Task<DateTime> GetStakeUnlockTimeAsync(int stakeId)
    {
        try
        {
            var contractAddresses = _blockchainService.GetContractAddresses();

            var unlockTimestamp = await _blockchainService.CallContractFunctionAsync<BigInteger>(
                contractAddresses.StakingAddress,
                "getStakeUnlockTime(uint256)",
                stakeId);

            return DateTimeOffset.FromUnixTimeSeconds((long)unlockTimestamp).DateTime;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unlock time for stake {StakeId}", stakeId);
            throw;
        }
    }

    public async Task<string> StakeTokensTransactionAsync(string walletAddress, int poolId, decimal amount)
    {
        try
        {
            var contractAddresses = _blockchainService.GetContractAddresses();

            // Convert amount to wei
            var amountInWei = ConvertToWei(amount, 18);

            var transactionHash = await _blockchainService.ExecuteContractFunctionAsync(
                contractAddresses.StakingAddress,
                "stakeTokens(uint256,uint256)",
                poolId, amountInWei);

            return transactionHash;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing stake transaction for {WalletAddress}", walletAddress);
            throw;
        }
    }

    public async Task<string> UnstakeTokensTransactionAsync(string walletAddress, int stakeId)
    {
        try
        {
            var contractAddresses = _blockchainService.GetContractAddresses();

            var transactionHash = await _blockchainService.ExecuteContractFunctionAsync(
                contractAddresses.StakingAddress,
                "unstakeTokens(uint256)",
                stakeId);

            return transactionHash;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing unstake transaction for {WalletAddress}", walletAddress);
            throw;
        }
    }

    public async Task<string> ClaimRewardsTransactionAsync(string walletAddress, int stakeId)
    {
        try
        {
            var contractAddresses = _blockchainService.GetContractAddresses();

            var transactionHash = await _blockchainService.ExecuteContractFunctionAsync(
                contractAddresses.StakingAddress,
                "claimRewards(uint256)",
                stakeId);

            return transactionHash;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing claim rewards transaction for {WalletAddress}", walletAddress);
            throw;
        }
    }

    public async Task<string> CompoundRewardsTransactionAsync(string walletAddress, int stakeId)
    {
        try
        {
            var contractAddresses = _blockchainService.GetContractAddresses();

            var transactionHash = await _blockchainService.ExecuteContractFunctionAsync(
                contractAddresses.StakingAddress,
                "compoundRewards(uint256)",
                stakeId);

            return transactionHash;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing compound rewards transaction for {WalletAddress}", walletAddress);
            throw;
        }
    }

    #region Helper Methods

    private decimal ConvertFromWei(BigInteger wei, int decimals)
    {
        return (decimal)wei / (decimal)Math.Pow(10, decimals);
    }

    private BigInteger ConvertToWei(decimal amount, int decimals)
    {
        return new BigInteger(amount * (decimal)Math.Pow(10, decimals));
    }

    #endregion
}