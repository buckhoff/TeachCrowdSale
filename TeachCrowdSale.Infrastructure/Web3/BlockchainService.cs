using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Models;

namespace TeachCrowdSale.Infrastructure.Web3
{
    public class BlockchainService : IBlockchainService
    {
        private readonly ILogger<BlockchainService> _logger;
        private readonly Web3Helper _web3Helper;

        public BlockchainService(
            ILogger<BlockchainService> logger,
            Web3Helper web3Helper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _web3Helper = web3Helper ?? throw new ArgumentNullException(nameof(web3Helper));
        }

        public bool IsValidAddress(string address)
        {
            return _web3Helper.IsValidAddress(address);
        }

        public ContractAddresses GetContractAddresses()
        {
            return _web3Helper.GetContractAddresses();
        }

        public async Task<decimal> GetBalanceAsync(string address, string tokenAddress)
        {
            try
            {
                return await _web3Helper.GetBalanceAsync(address, tokenAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting balance for {address} in token {tokenAddress}");
                throw;
            }
        }

        public async Task<decimal> GetEthBalanceAsync(string address)
        {
            try
            {
                return await _web3Helper.GetEthBalanceAsync(address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting ETH balance for {address}");
                throw;
            }
        }

        public async Task<bool> ApproveTokenSpendingAsync(string ownerAddress, string spenderAddress, decimal amount, string tokenAddress)
        {
            try
            {
                return await _web3Helper.ApproveTokenSpendingAsync(spenderAddress, amount, tokenAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error approving token spending for {spenderAddress} on token {tokenAddress}");
                throw;
            }
        }

        public async Task<string> ExecuteContractFunctionAsync(string contractAddress, string functionSignature, params object[] parameters)
        {
            try
            {
                // Parse the function signature to get the function name and ABI
                string functionName = functionSignature.Substring(0, functionSignature.IndexOf('(')); 
                string abi = GetContractAbi(contractAddress);

                return await _web3Helper.ExecuteContractFunctionAsync(contractAddress, abi, functionName, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing contract function {functionSignature} on {contractAddress}");
                throw;
            }
        }

        public async Task<T> CallContractFunctionAsync<T>(string contractAddress, string functionSignature, params object[] parameters)
        {
            try
            {
                // Parse the function signature to get the function name and ABI
                string functionName = functionSignature.Substring(0, functionSignature.IndexOf('(')); 
                string abi = GetContractAbi(contractAddress);

                return await _web3Helper.CallContractFunctionAsync<T>(contractAddress, abi, functionName, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling contract function {functionSignature} on {contractAddress}");
                throw;
            }
        }

        // Helper method to get the appropriate ABI based on contract address
        private string GetContractAbi(string contractAddress)
        {
            var addresses = GetContractAddresses();

            if (contractAddress.Equals(addresses.PresaleAddress, StringComparison.OrdinalIgnoreCase))
            {
                return PresaleAbi;
            }
            else if (contractAddress.Equals(addresses.TokenAddress, StringComparison.OrdinalIgnoreCase))
            {
                return TokenAbi;
            }
            else if (contractAddress.Equals(addresses.StabilityFundAddress, StringComparison.OrdinalIgnoreCase))
            {
                return StabilityFundAbi;
            }
            else if (contractAddress.Equals(addresses.StakingAddress, StringComparison.OrdinalIgnoreCase))
            {
                return StakingAbi;
            }
            else if (contractAddress.Equals(addresses.GovernanceAddress, StringComparison.OrdinalIgnoreCase))
            {
                return GovernanceAbi;
            }
            else if (contractAddress.Equals(addresses.MarketplaceAddress, StringComparison.OrdinalIgnoreCase))
            {
                return MarketplaceAbi;
            }
            else if (contractAddress.Equals(addresses.RewardAddress, StringComparison.OrdinalIgnoreCase))
            {
                return RewardAbi;
            }
            else if (contractAddress.Equals(addresses.RegistryAddress, StringComparison.OrdinalIgnoreCase))
            {
                return RegistryAbi;
            }
            else
            {
                // Default to ERC20 ABI for unknown contracts
                return Erc20Abi;
            }
        }

        // Contract ABIs - would be stored in separate files in a real application
        private const string PresaleAbi = @"[
            // Add your presale contract ABI here
        ]";

        private const string TokenAbi = @"[
            // Add your token contract ABI here
        ]";

        private const string StabilityFundAbi = @"[
            // Add your stability fund contract ABI here
        ]";

        private const string StakingAbi = @"[
            // Add your staking contract ABI here
        ]";

        private const string GovernanceAbi = @"[
            // Add your governance contract ABI here
        ]";

        private const string MarketplaceAbi = @"[
            // Add your marketplace contract ABI here
        ]";

        private const string RewardAbi = @"[
            // Add your reward contract ABI here
        ]";

        private const string RegistryAbi = @"[
            // Add your registry contract ABI here
        ]";

        private const string Erc20Abi = @"[
            {
                ""constant"": true,
                ""inputs"": [],
                ""name"": ""name"",
                ""outputs"": [{ ""name"": """", ""type"": ""string"" }],
                ""payable"": false,
                ""stateMutability"": ""view"",
                ""type"": ""function""
            },
            {
                ""constant"": false,
                ""inputs"": [
                    { ""name"": ""_spender"", ""type"": ""address"" },
                    { ""name"": ""_value"", ""type"": ""uint256"" }
                ],
                ""name"": ""approve"",
                ""outputs"": [{ ""name"": """", ""type"": ""bool"" }],
                ""payable"": false,
                ""stateMutability"": ""nonpayable"",
                ""type"": ""function""
            },
            {
                ""constant"": true,
                ""inputs"": [],
                ""name"": ""totalSupply"",
                ""outputs"": [{ ""name"": """", ""type"": ""uint256"" }],
                ""payable"": false,
                ""stateMutability"": ""view"",
                ""type"": ""function""
            },
            {
                ""constant"": false,
                ""inputs"": [
                    { ""name"": ""_from"", ""type"": ""address"" },
                    { ""name"": ""_to"", ""type"": ""address"" },
                    { ""name"": ""_value"", ""type"": ""uint256"" }
                ],
                ""name"": ""transferFrom"",
                ""outputs"": [{ ""name"": """", ""type"": ""bool"" }],
                ""payable"": false,
                ""stateMutability"": ""nonpayable"",
                ""type"": ""function""
            },
            {
                ""constant"": true,
                ""inputs"": [],
                ""name"": ""decimals"",
                ""outputs"": [{ ""name"": """", ""type"": ""uint8"" }],
                ""payable"": false,
                ""stateMutability"": ""view"",
                ""type"": ""function""
            },
            {
                ""constant"": true,
                ""inputs"": [{ ""name"": ""_owner"", ""type"": ""address"" }],
                ""name"": ""balanceOf"",
                ""outputs"": [{ ""name"": """", ""type"": ""uint256"" }],
                ""payable"": false,
                ""stateMutability"": ""view"",
                ""type"": ""function""
            },
            {
                ""constant"": true,
                ""inputs"": [],
                ""name"": ""symbol"",
                ""outputs"": [{ ""name"": """", ""type"": ""string"" }],
                ""payable"": false,
                ""stateMutability"": ""view"",
                ""type"": ""function""
            },
            {
                ""constant"": false,
                ""inputs"": [
                    { ""name"": ""_to"", ""type"": ""address"" },
                    { ""name"": ""_value"", ""type"": ""uint256"" }
                ],
                ""name"": ""transfer"",
                ""outputs"": [{ ""name"": """", ""type"": ""bool"" }],
                ""payable"": false,
                ""stateMutability"": ""nonpayable"",
                ""type"": ""function""
            },
            {
                ""constant"": true,
                ""inputs"": [
                    { ""name"": ""_owner"", ""type"": ""address"" },
                    { ""name"": ""_spender"", ""type"": ""address"" }
                ],
                ""name"": ""allowance"",
                ""outputs"": [{ ""name"": """", ""type"": ""uint256"" }],
                ""payable"": false,
                ""stateMutability"": ""view"",
                ""type"": ""function""
            }
        ]";
    }
}