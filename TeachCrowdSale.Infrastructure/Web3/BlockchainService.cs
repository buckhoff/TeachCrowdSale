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

        public ContractAddressesModel GetContractAddresses()
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
            else if (contractAddress.Equals(addresses.LiquidityManagerAddress, StringComparison.OrdinalIgnoreCase))
            {
                return LiquidityManagerAbi;
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
  {""inputs"":[],""name"":""getCurrentTier"",""outputs"":[{""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[{""name"":""_tierId"",""type"":""uint256""}],""name"":""getTierInfo"",""outputs"":[{""name"":""price"",""type"":""uint256""},{""name"":""allocation"",""type"":""uint256""},{""name"":""sold"",""type"":""uint256""},{""name"":""minPurchase"",""type"":""uint256""},{""name"":""maxPurchase"",""type"":""uint256""},{""name"":""vestingTGE"",""type"":""uint256""},{""name"":""vestingMonths"",""type"":""uint256""},{""name"":""isActive"",""type"":""bool""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[],""name"":""getTotalTiers"",""outputs"":[{""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[],""name"":""getTotalTokensSold"",""outputs"":[{""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[],""name"":""getTotalFundsRaised"",""outputs"":[{""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[{""name"":""_user"",""type"":""address""}],""name"":""getUserPurchaseAmount"",""outputs"":[{""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[{""name"":""_user"",""type"":""address""},{""name"":""_token"",""type"":""address""}],""name"":""getUserPurchaseDetails"",""outputs"":[{""name"":""tokenAmount"",""type"":""uint256""},{""name"":""usdAmount"",""type"":""uint256""},{""name"":""vestingSchedule"",""type"":""uint256""},{""name"":""claimedAmount"",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[{""name"":""_user"",""type"":""address""}],""name"":""getClaimableAmount"",""outputs"":[{""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[{""name"":""_user"",""type"":""address""}],""name"":""getClaimHistory"",""outputs"":[{""components"":[{""name"":""amount"",""type"":""uint128""},{""name"":""timestamp"",""type"":""uint64""}],""name"":"""",""type"":""tuple[]""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[],""name"":""isPresaleActive"",""outputs"":[{""name"":"""",""type"":""bool""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[],""name"":""paused"",""outputs"":[{""name"":"""",""type"":""bool""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[{""name"":""_tierId"",""type"":""uint256""},{""name"":""_usdAmount"",""type"":""uint256""}],""name"":""buyWithUSDC"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},
  {""inputs"":[],""name"":""claimTokens"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},
  {""inputs"":[],""name"":""withdrawTokens"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},
  {""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""buyer"",""type"":""address""},{""indexed"":false,""name"":""tierId"",""type"":""uint256""},{""indexed"":false,""name"":""usdAmount"",""type"":""uint256""},{""indexed"":false,""name"":""tokenAmount"",""type"":""uint256""}],""name"":""TokensPurchased"",""type"":""event""},
  {""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""user"",""type"":""address""},{""indexed"":false,""name"":""amount"",""type"":""uint256""}],""name"":""TokensClaimed"",""type"":""event""}
]";

        private const string TokenAbi = @"[
  {""inputs"":[],""name"":""name"",""outputs"":[{""name"":"""",""type"":""string""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[],""name"":""symbol"",""outputs"":[{""name"":"""",""type"":""string""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[],""name"":""decimals"",""outputs"":[{""name"":"""",""type"":""uint8""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[],""name"":""totalSupply"",""outputs"":[{""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[{""name"":""account"",""type"":""address""}],""name"":""balanceOf"",""outputs"":[{""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[{""name"":""spender"",""type"":""address""},{""name"":""value"",""type"":""uint256""}],""name"":""approve"",""outputs"":[{""name"":"""",""type"":""bool""}],""stateMutability"":""nonpayable"",""type"":""function""},
  {""inputs"":[{""name"":""owner"",""type"":""address""},{""name"":""spender"",""type"":""address""}],""name"":""allowance"",""outputs"":[{""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[{""name"":""to"",""type"":""address""},{""name"":""value"",""type"":""uint256""}],""name"":""transfer"",""outputs"":[{""name"":"""",""type"":""bool""}],""stateMutability"":""nonpayable"",""type"":""function""},
  {""inputs"":[{""name"":""from"",""type"":""address""},{""name"":""to"",""type"":""address""},{""name"":""value"",""type"":""uint256""}],""name"":""transferFrom"",""outputs"":[{""name"":"""",""type"":""bool""}],""stateMutability"":""nonpayable"",""type"":""function""},
  {""inputs"":[{""name"":""amount"",""type"":""uint256""}],""name"":""burn"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},
  {""inputs"":[{""name"":""from"",""type"":""address""},{""name"":""amount"",""type"":""uint256""}],""name"":""burnFrom"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},
  {""inputs"":[],""name"":""paused"",""outputs"":[{""name"":"""",""type"":""bool""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[],""name"":""pause"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},
  {""inputs"":[],""name"":""unpause"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""}
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
  {""inputs"":[],""name"":""getPoolCount"",""outputs"":[{""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[{""name"":""_poolId"",""type"":""uint256""}],""name"":""getPoolInfo"",""outputs"":[{""name"":""rewardRate"",""type"":""uint256""},{""name"":""lockupPeriod"",""type"":""uint256""},{""name"":""totalStaked"",""type"":""uint256""},{""name"":""isActive"",""type"":""bool""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[{""name"":""_poolId"",""type"":""uint256""}],""name"":""getPoolTotalStaked"",""outputs"":[{""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[{""name"":""_user"",""type"":""address""},{""name"":""_poolId"",""type"":""uint256""}],""name"":""getUserStakeInfo"",""outputs"":[{""name"":""stakedAmount"",""type"":""uint256""},{""name"":""stakeTime"",""type"":""uint256""},{""name"":""unlockTime"",""type"":""uint256""},{""name"":""pendingRewards"",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[{""name"":""_user"",""type"":""address""},{""name"":""_poolId"",""type"":""uint256""}],""name"":""getPendingRewards"",""outputs"":[{""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[{""name"":""_user"",""type"":""address""}],""name"":""getUserTotalStaked"",""outputs"":[{""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[{""name"":""_user"",""type"":""address""}],""name"":""getUserTotalRewards"",""outputs"":[{""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[{""name"":""_poolId"",""type"":""uint256""},{""name"":""_amount"",""type"":""uint256""}],""name"":""stake"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},
  {""inputs"":[{""name"":""_poolId"",""type"":""uint256""},{""name"":""_amount"",""type"":""uint256""}],""name"":""unstake"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},
  {""inputs"":[{""name"":""_poolId"",""type"":""uint256""}],""name"":""claimRewards"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},
  {""inputs"":[{""name"":""_poolId"",""type"":""uint256""}],""name"":""compound"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},
  {""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""user"",""type"":""address""},{""indexed"":false,""name"":""poolId"",""type"":""uint256""},{""indexed"":false,""name"":""amount"",""type"":""uint256""}],""name"":""Staked"",""type"":""event""},
  {""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""user"",""type"":""address""},{""indexed"":false,""name"":""poolId"",""type"":""uint256""},{""indexed"":false,""name"":""amount"",""type"":""uint256""}],""name"":""Unstaked"",""type"":""event""},
  {""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""user"",""type"":""address""},{""indexed"":false,""name"":""poolId"",""type"":""uint256""},{""indexed"":false,""name"":""reward"",""type"":""uint256""}],""name"":""RewardsClaimed"",""type"":""event""}
]";

        private const string LiquidityManagerAbi = @"[
  {""inputs"":[],""name"":""getActivePoolCount"",""outputs"":[{""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[{""name"":""_poolId"",""type"":""uint256""}],""name"":""getPoolInfo"",""outputs"":[{""name"":""token0"",""type"":""address""},{""name"":""token1"",""type"":""address""},{""name"":""fee"",""type"":""uint24""},{""name"":""liquidity"",""type"":""uint128""},{""name"":""sqrtPriceX96"",""type"":""uint256""},{""name"":""tick"",""type"":""int24""},{""name"":""isActive"",""type"":""bool""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[{""name"":""_user"",""type"":""address""}],""name"":""getUserPositions"",""outputs"":[{""components"":[{""name"":""poolId"",""type"":""uint256""},{""name"":""liquidity"",""type"":""uint128""},{""name"":""tokensOwed0"",""type"":""uint256""},{""name"":""tokensOwed1"",""type"":""uint256""},{""name"":""feeGrowthInside0LastX128"",""type"":""uint256""},{""name"":""feeGrowthInside1LastX128"",""type"":""uint256""}],""name"":"""",""type"":""tuple[]""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[{""name"":""_poolId"",""type"":""uint256""}],""name"":""getPoolTVL"",""outputs"":[{""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[{""name"":""_poolId"",""type"":""uint256""}],""name"":""getPoolAPR"",""outputs"":[{""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},
  {""inputs"":[{""name"":""_poolId"",""type"":""uint256""},{""name"":""_amount0Desired"",""type"":""uint256""},{""name"":""_amount1Desired"",""type"":""uint256""},{""name"":""_amount0Min"",""type"":""uint256""},{""name"":""_amount1Min"",""type"":""uint256""}],""name"":""addLiquidity"",""outputs"":[{""name"":""liquidity"",""type"":""uint128""},{""name"":""amount0"",""type"":""uint256""},{""name"":""amount1"",""type"":""uint256""}],""stateMutability"":""nonpayable"",""type"":""function""},
  {""inputs"":[{""name"":""_poolId"",""type"":""uint256""},{""name"":""_liquidity"",""type"":""uint128""},{""name"":""_amount0Min"",""type"":""uint256""},{""name"":""_amount1Min"",""type"":""uint256""}],""name"":""removeLiquidity"",""outputs"":[{""name"":""amount0"",""type"":""uint256""},{""name"":""amount1"",""type"":""uint256""}],""stateMutability"":""nonpayable"",""type"":""function""},
  {""inputs"":[{""name"":""_poolId"",""type"":""uint256""}],""name"":""collectFees"",""outputs"":[{""name"":""amount0"",""type"":""uint256""},{""name"":""amount1"",""type"":""uint256""}],""stateMutability"":""nonpayable"",""type"":""function""},
  {""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""user"",""type"":""address""},{""indexed"":false,""name"":""poolId"",""type"":""uint256""},{""indexed"":false,""name"":""liquidity"",""type"":""uint128""},{""indexed"":false,""name"":""amount0"",""type"":""uint256""},{""indexed"":false,""name"":""amount1"",""type"":""uint256""}],""name"":""LiquidityAdded"",""type"":""event""},
  {""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""user"",""type"":""address""},{""indexed"":false,""name"":""poolId"",""type"":""uint256""},{""indexed"":false,""name"":""liquidity"",""type"":""uint128""},{""indexed"":false,""name"":""amount0"",""type"":""uint256""},{""indexed"":false,""name"":""amount1"",""type"":""uint256""}],""name"":""LiquidityRemoved"",""type"":""event""}
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