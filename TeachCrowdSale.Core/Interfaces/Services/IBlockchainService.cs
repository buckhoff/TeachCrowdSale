using TeachCrowdSale.Core.Models;

namespace TeachCrowdSale.Core.Interfaces.Services;

public interface IBlockchainService
{
    bool IsValidAddress(string address);
    ContractAddressesModel GetContractAddresses();
    Task<decimal> GetBalanceAsync(string address, string tokenAddress);
    Task<decimal> GetEthBalanceAsync(string address);
    Task<bool> ApproveTokenSpendingAsync(string ownerAddress, string spenderAddress, decimal amount, string tokenAddress);
    Task<string> ExecuteContractFunctionAsync(string contractAddress, string functionSignature, params object[] parameters);
    Task<T> CallContractFunctionAsync<T>(string contractAddress, string functionSignature, params object[] parameters);
    Task<bool> IsConnectedAsync();
}