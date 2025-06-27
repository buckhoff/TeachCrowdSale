using System;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Web3;
using Nethereum;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts;
using Nethereum.Util;
using Nethereum.Hex.HexTypes;
using TeachCrowdSale.Core.Models;
using TeachCrowdSale.Core.Models.Configuration;

namespace TeachCrowdSale.Infrastructure.Web3
{
    public class Web3Helper
    {
        private readonly ILogger<Web3Helper> _logger;
        private readonly BlockchainSettings _settings;
        private readonly IWeb3 _web3;
        private readonly IWeb3 _adminWeb3;

        public Web3Helper(
            ILogger<Web3Helper> logger,
            IOptions<BlockchainSettings> settings)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

            // Initialize read-only Web3 instance
            _web3 = new Nethereum.Web3.Web3(_settings.RpcUrl);

            // Initialize admin Web3 instance with private key for transaction signing
            if (!string.IsNullOrEmpty(_settings.AdminPrivateKey))
            {
                var account = new Account(_settings.AdminPrivateKey, _settings.NetworkId);
                _adminWeb3 = new Nethereum.Web3.Web3(account, _settings.RpcUrl);
            }
        }

        public bool IsValidAddress(string address)
        {
            return AddressUtil.Current.IsValidEthereumAddressHexFormat(address);
        }

        public async Task<decimal> GetBalanceAsync(string address, string tokenAddress)
        {
            try
            {
                // Create ERC20 contract service
                var contract = _web3.Eth.GetContract(Erc20Abi, tokenAddress);
                var balanceOfFunction = contract.GetFunction("balanceOf");
                var decimalsFunction = contract.GetFunction("decimals");

                // Get token balance and decimals
                var balance = await balanceOfFunction.CallAsync<BigInteger>(address);
                var decimals = await decimalsFunction.CallAsync<int>();

                // Convert to decimal with correct decimal places
                return (decimal)balance / (decimal)Math.Pow(10, decimals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting token balance for {address} in token {tokenAddress}");
                throw;
            }
        }

        public async Task<decimal> GetEthBalanceAsync(string address)
        {
            try
            {
                var weiBalance = await _web3.Eth.GetBalance.SendRequestAsync(address);
                var etherBalance = UnitConversion.Convert.FromWei(weiBalance.Value);
                return (decimal)etherBalance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting ETH balance for {address}");
                throw;
            }
        }

        public ContractAddressesModel GetContractAddresses()
        {
            return new ContractAddressesModel
            {
                PresaleAddress = _settings.PresaleAddress,
                TokenAddress = _settings.TokenAddress,
                PaymentTokenAddress = _settings.PaymentTokenAddress,
                StabilityFundAddress = _settings.StabilityFundAddress,
                StakingAddress = _settings.StakingAddress,
                GovernanceAddress = _settings.GovernanceAddress,
                MarketplaceAddress = _settings.MarketplaceAddress,
            LiquidityManagerAddress = _settings.LiquidityManagerAddress, // Assuming this is the liquidity manager
                RewardAddress = _settings.RewardAddress,
                RegistryAddress = _settings.RegistryAddress,
                NetworkId = _settings.NetworkId,
                ChainName = _settings.NetworkId switch
                {
                    1 => "Ethereum Mainnet",
                    3 => "Ropsten Testnet",
                    4 => "Rinkeby Testnet",
                    5 => "Goerli Testnet",
                    42 => "Kovan Testnet",
                    56 => "Binance Smart Chain",
                    97 => "BSC Testnet",
                    137 => "Polygon Mainnet",
                    80001 => "Polygon Mumbai Testnet",
                    _ => $"Network {_settings.NetworkId}"
                }
            };
        }

        public async Task<string> ExecuteContractFunctionAsync(string contractAddress, string abi, string functionName, params object[] parameters)
        {
            try
            {
                // Ensure we have an admin web3 instance
                if (_adminWeb3 == null)
                {
                    throw new InvalidOperationException("Admin private key not configured");
                }

                // Create contract and function
                var contract = _adminWeb3.Eth.GetContract(abi, contractAddress);
                var function = contract.GetFunction(functionName);

                // Estimate gas
                var gas = await function.EstimateGasAsync(parameters);

                // Send transaction
                var receipt = await function.SendTransactionAndWaitForReceiptAsync(
                    from: _adminWeb3.TransactionManager.Account.Address,
                    gas: new HexBigInteger(gas.Value),
                    value: new HexBigInteger(0),
                    functionInput: parameters);

                // Check status
                if (receipt.Status.Value == 0)
                {
                    throw new Exception($"Transaction {receipt.TransactionHash} failed");
                }

                return receipt.TransactionHash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing contract function {functionName} on {contractAddress}");
                throw;
            }
        }

        public async Task<T> CallContractFunctionAsync<T>(string contractAddress, string abi, string functionName, params object[] parameters)
        {
            try
            {
                // Create contract and function
                var contract = _web3.Eth.GetContract(abi, contractAddress);
                var function = contract.GetFunction(functionName);

                // Call function
                return await function.CallAsync<T>(parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling contract function {functionName} on {contractAddress}");
                throw;
            }
        }

        public async Task<bool> ApproveTokenSpendingAsync(string spenderAddress, decimal amount, string tokenAddress)
        {
            try
            {
                // Ensure we have an admin web3 instance
                if (_adminWeb3 == null)
                {
                    throw new InvalidOperationException("Admin private key not configured");
                }

                // Create ERC20 contract service
                var contract = _adminWeb3.Eth.GetContract(Erc20Abi, tokenAddress);
                var decimalsFunction = contract.GetFunction("decimals");
                var approveFunction = contract.GetFunction("approve");

                // Get token decimals
                var decimals = await decimalsFunction.CallAsync<int>();

                // Convert amount to token units
                var tokenAmount = new BigInteger(amount * (decimal)Math.Pow(10, decimals));

                // Send approve transaction
                var receipt = await approveFunction.SendTransactionAndWaitForReceiptAsync(
                    from: _adminWeb3.TransactionManager.Account.Address,
                    gas: new HexBigInteger(100000), // Gas limit
                    value: new HexBigInteger(0),
                    functionInput: new object[] { spenderAddress, tokenAmount });

                // Check status
                return receipt.Status.Value == 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error approving token spending for {spenderAddress} on token {tokenAddress}");
                throw;
            }
        }

        // Standard ERC20 ABI
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

        // TokenCrowdSale contract ABI (add the ABI specific to your contract)
        private const string PresaleAbi = @"[
            // Add your presale contract ABI here
        ]";
    }
}