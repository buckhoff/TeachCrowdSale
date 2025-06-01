using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models;

namespace TeachCrowdSale.Infrastructure.Web3
{
    public class BlockchainService : IBlockchainService
    {
        private readonly ILogger<BlockchainService> _logger;
        private readonly Web3Helper _web3Helper;
        private readonly IAbiService _abiService;

        public BlockchainService(
            ILogger<BlockchainService> logger,
            Web3Helper web3Helper,
            IAbiService abiService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _web3Helper = web3Helper ?? throw new ArgumentNullException(nameof(web3Helper));
            _abiService = abiService;
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
        public async Task<bool> IsConnectedAsync()
        {
            try
            {
                // Check if web3 provider is connected
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                var response = await client.GetAsync("https://polygon-rpc.com/");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Blockchain connectivity check failed");
                return false;
            }
        }

        // Contract ABIs - would be stored in separate files in a real application
        private const string PresaleAbi = @"[
            {
        ""constant"": true,
        ""inputs"": [],
        ""name"": ""getCurrentTier"",
        ""outputs"": [{""name"": """", ""type"": ""uint256""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""constant"": true,
        ""inputs"": [],
        ""name"": ""tierCount"",
        ""outputs"": [{""name"": """", ""type"": ""uint256""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""constant"": true,
        ""inputs"": [{""name"": """", ""type"": ""uint256""}],
        ""name"": ""tiers"",
        ""outputs"": [
            {""name"": ""price"", ""type"": ""uint256""},
            {""name"": ""allocation"", ""type"": ""uint256""},
            {""name"": ""sold"", ""type"": ""uint256""},
            {""name"": ""minPurchase"", ""type"": ""uint256""},
            {""name"": ""maxPurchase"", ""type"": ""uint256""},
            {""name"": ""vestingTGE"", ""type"": ""uint256""},
            {""name"": ""vestingMonths"", ""type"": ""uint256""}
        ],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""constant"": true,
        ""inputs"": [{""name"": """", ""type"": ""uint256""}],
        ""name"": ""tierDeadlines"",
        ""outputs"": [{""name"": """", ""type"": ""uint256""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""constant"": true,
        ""inputs"": [],
        ""name"": ""totalTokensSold"",
        ""outputs"": [{""name"": """", ""type"": ""uint256""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""constant"": true,
        ""inputs"": [],
        ""name"": ""totalUsdRaised"",
        ""outputs"": [{""name"": """", ""type"": ""uint256""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""constant"": true,
        ""inputs"": [],
        ""name"": ""participantsCount"",
        ""outputs"": [{""name"": """", ""type"": ""uint256""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""constant"": true,
        ""inputs"": [],
        ""name"": ""presaleStart"",
        ""outputs"": [{""name"": """", ""type"": ""uint256""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""constant"": true,
        ""inputs"": [],
        ""name"": ""presaleEnd"",
        ""outputs"": [{""name"": """", ""type"": ""uint256""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""constant"": true,
        ""inputs"": [{""name"": """", ""type"": ""address""}],
        ""name"": ""purchases"",
        ""outputs"": [
            {""name"": ""tokens"", ""type"": ""uint256""},
            {""name"": ""usdAmount"", ""type"": ""uint256""}
        ],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""constant"": true,
        ""inputs"": [{""name"": """", ""type"": ""address""}],
        ""name"": ""lastClaimTime"",
        ""outputs"": [{""name"": """", ""type"": ""uint256""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""constant"": true,
        ""inputs"": [
            {""name"": """", ""type"": ""address""},
            {""name"": """", ""type"": ""uint256""}
        ],
        ""name"": ""getUserTierAmount"",
        ""outputs"": [{""name"": """", ""type"": ""uint256""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""constant"": true,
        ""inputs"": [{""name"": """", ""type"": ""address""}],
        ""name"": ""claimableTokens"",
        ""outputs"": [{""name"": """", ""type"": ""uint256""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""constant"": true,
        ""inputs"": [{""name"": """", ""type"": ""address""}],
        ""name"": ""getNextVestingMilestone"",
        ""outputs"": [
            {""name"": ""timestamp"", ""type"": ""uint256""},
            {""name"": ""amount"", ""type"": ""uint256""}
        ],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""constant"": false,
        ""inputs"": [
            {""name"": ""tierId"", ""type"": ""uint256""},
            {""name"": ""amount"", ""type"": ""uint256""}
        ],
        ""name"": ""purchase"",
        ""outputs"": [],
        ""payable"": false,
        ""stateMutability"": ""nonpayable"",
        ""type"": ""function""
    },
    {
        ""constant"": false,
        ""inputs"": [],
        ""name"": ""withdrawTokens"",
        ""outputs"": [],
        ""payable"": false,
        ""stateMutability"": ""nonpayable"",
        ""type"": ""function""
    }
]";

        private const string TokenAbi = @"[
          {
        ""constant"": true,
        ""inputs"": [],
        ""name"": ""name"",
        ""outputs"": [{""name"": """", ""type"": ""string""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""constant"": true,
        ""inputs"": [],
        ""name"": ""symbol"",
        ""outputs"": [{""name"": """", ""type"": ""string""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""constant"": true,
        ""inputs"": [],
        ""name"": ""decimals"",
        ""outputs"": [{""name"": """", ""type"": ""uint8""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""constant"": true,
        ""inputs"": [],
        ""name"": ""totalSupply"",
        ""outputs"": [{""name"": """", ""type"": ""uint256""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""constant"": true,
        ""inputs"": [],
        ""name"": ""MAX_SUPPLY"",
        ""outputs"": [{""name"": """", ""type"": ""uint256""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""constant"": true,
        ""inputs"": [{""name"": ""_owner"", ""type"": ""address""}],
        ""name"": ""balanceOf"",
        ""outputs"": [{""name"": """", ""type"": ""uint256""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""constant"": false,
        ""inputs"": [
            {""name"": ""_to"", ""type"": ""address""},
            {""name"": ""_value"", ""type"": ""uint256""}
        ],
        ""name"": ""transfer"",
        ""outputs"": [{""name"": """", ""type"": ""bool""}],
        ""payable"": false,
        ""stateMutability"": ""nonpayable"",
        ""type"": ""function""
    },
    {
        ""constant"": false,
        ""inputs"": [
            {""name"": ""_spender"", ""type"": ""address""},
            {""name"": ""_value"", ""type"": ""uint256""}
        ],
        ""name"": ""approve"",
        ""outputs"": [{""name"": """", ""type"": ""bool""}],
        ""payable"": false,
        ""stateMutability"": ""nonpayable"",
        ""type"": ""function""
    },
    {
        ""constant"": true,
        ""inputs"": [
            {""name"": ""_owner"", ""type"": ""address""},
            {""name"": ""_spender"", ""type"": ""address""}
        ],
        ""name"": ""allowance"",
        ""outputs"": [{""name"": """", ""type"": ""uint256""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    }
]";

        private const string StabilityFundAbi = @"[
            {
        ""constant"": true,
        ""inputs"": [],
        ""name"": ""getVerifiedPrice"",
        ""outputs"": [{""name"": """", ""type"": ""uint256""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""constant"": true,
        ""inputs"": [],
        ""name"": ""get24hVolume"",
        ""outputs"": [{""name"": """", ""type"": ""uint256""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""constant"": true,
        ""inputs"": [],
        ""name"": ""get24hPriceChange"",
        ""outputs"": [{""name"": """", ""type"": ""int256""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    }
        ]";

        private const string StakingAbi = @"[
            {
        ""constant"": true,
        ""inputs"": [],
        ""name"": ""totalStaked"",
        ""outputs"": [{""name"": """", ""type"": ""uint256""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""constant"": true,
        ""inputs"": [{""name"": """", ""type"": ""address""}],
        ""name"": ""stakedBalance"",
        ""outputs"": [{""name"": """", ""type"": ""uint256""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""constant"": true,
        ""inputs"": [{""name"": """", ""type"": ""address""}],
        ""name"": ""pendingRewards"",
        ""outputs"": [{""name"": """", ""type"": ""uint256""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""constant"": false,
        ""inputs"": [{""name"": ""amount"", ""type"": ""uint256""}],
        ""name"": ""stake"",
        ""outputs"": [],
        ""payable"": false,
        ""stateMutability"": ""nonpayable"",
        ""type"": ""function""
    },
    {
        ""constant"": false,
        ""inputs"": [{""name"": ""amount"", ""type"": ""uint256""}],
        ""name"": ""unstake"",
        ""outputs"": [],
        ""payable"": false,
        ""stateMutability"": ""nonpayable"",
        ""type"": ""function""
    },
    {
        ""constant"": false,
        ""inputs"": [],
        ""name"": ""claimRewards"",
        ""outputs"": [],
        ""payable"": false,
        ""stateMutability"": ""nonpayable"",
        ""type"": ""function""
    }
        ]";

        private const string GovernanceAbi = @"[
           {
        ""constant"": true,
        ""inputs"": [],
        ""name"": ""proposalCount"",
        ""outputs"": [{""name"": """", ""type"": ""uint256""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""constant"": true,
        ""inputs"": [{""name"": """", ""type"": ""uint256""}],
        ""name"": ""proposals"",
        ""outputs"": [
            {""name"": ""id"", ""type"": ""uint256""},
            {""name"": ""proposer"", ""type"": ""address""},
            {""name"": ""description"", ""type"": ""string""},
            {""name"": ""forVotes"", ""type"": ""uint256""},
            {""name"": ""againstVotes"", ""type"": ""uint256""},
            {""name"": ""endTime"", ""type"": ""uint256""},
            {""name"": ""executed"", ""type"": ""bool""}
        ],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    }
        ]";

        private const string MarketplaceAbi = @"[
            {
        ""constant"": true,
        ""inputs"": [],
        ""name"": ""totalListings"",
        ""outputs"": [{""name"": """", ""type"": ""uint256""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    }
        ]";

        private const string RewardAbi = @"[
            {
        ""constant"": true,
        ""inputs"": [{""name"": """", ""type"": ""address""}],
        ""name"": ""pendingRewards"",
        ""outputs"": [{""name"": """", ""type"": ""uint256""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    }
        ]";

        private const string RegistryAbi = @"[
            {
        ""constant"": true,
        ""inputs"": [{""name"": """", ""type"": ""string""}],
        ""name"": ""getAddress"",
        ""outputs"": [{""name"": """", ""type"": ""address""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""
    }
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