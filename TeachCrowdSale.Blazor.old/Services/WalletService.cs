using System.Text.Json;
using Microsoft.JSInterop;

namespace TeachTokenCrowdsale.Web.Services
{
    public class WalletService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger<WalletService> _logger;

        public WalletService(IJSRuntime jsRuntime, ILogger<WalletService> logger)
        {
            _jsRuntime = jsRuntime;
            _logger = logger;
        }

        public async Task<WalletConnectionResult> ConnectWalletAsync()
        {
            try
            {
                var result = await _jsRuntime.InvokeAsync<JsonElement>("connectWallet");
                return JsonSerializer.Deserialize<WalletConnectionResult>(result.GetRawText());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting wallet");
                return new WalletConnectionResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<WalletConnectionState> GetConnectionStateAsync()
        {
            try
            {
                var result = await _jsRuntime.InvokeAsync<JsonElement>("getWalletConnectionState");
                return JsonSerializer.Deserialize<WalletConnectionState>(result.GetRawText());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wallet connection state");
                return new WalletConnectionState
                {
                    IsConnected = false
                };
            }
        }

        public async Task<bool> DisconnectWalletAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<bool>("disconnectWallet");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting wallet");
                return false;
            }
        }

        public async Task<NetworkSwitchResult> SwitchNetworkAsync(int chainId)
        {
            try
            {
                var result = await _jsRuntime.InvokeAsync<JsonElement>("switchNetwork", chainId);
                return JsonSerializer.Deserialize<NetworkSwitchResult>(result.GetRawText());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error switching network");
                return new NetworkSwitchResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<decimal> GetBalanceAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<decimal>("getWalletBalance");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wallet balance");
                return 0;
            }
        }

        public async Task<decimal> GetTokenBalanceAsync(string tokenAddress)
        {
            try
            {
                return await _jsRuntime.InvokeAsync<decimal>("getTokenBalance", tokenAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token balance");
                return 0;
            }
        }

        public async Task<bool> ApproveTokenAsync(string tokenAddress, string spenderAddress, decimal amount)
        {
            try
            {
                return await _jsRuntime.InvokeAsync<bool>("approveToken", tokenAddress, spenderAddress, amount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving token");
                return false;
            }
        }

        public string GetWalletIcon(string walletType)
        {
            return walletType.ToLower() switch
            {
                "metamask" => "images/wallets/metamask.svg",
                "walletconnect" => "images/wallets/walletconnect.svg",
                "coinbase" => "images/wallets/coinbase.svg",
                "trustwallet" => "images/wallets/trustwallet.svg",
                _ => "images/wallets/generic-wallet.svg"
            };
        }
    }

    public class WalletConnectionResult
    {
        public bool Success { get; set; }
        public string Address { get; set; }
        public int ChainId { get; set; }
        public string NetworkName { get; set; }
        public bool NetworkValid { get; set; }
        public string WalletType { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class WalletConnectionState
    {
        public bool IsConnected { get; set; }
        public string Address { get; set; }
        public int ChainId { get; set; }
        public string NetworkName { get; set; }
        public bool NetworkValid { get; set; }
        public string WalletType { get; set; }
    }

    public class NetworkSwitchResult
    {
        public bool Success { get; set; }
        public int ChainId { get; set; }
        public string NetworkName { get; set; }
        public string ErrorMessage { get; set; }
    }
}