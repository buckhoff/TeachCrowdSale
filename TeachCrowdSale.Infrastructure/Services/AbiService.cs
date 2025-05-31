using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Interfaces.Services;

namespace TeachCrowdSale.Infrastructure.Services
{
    public class AbiService : IAbiService
    {
        private readonly ILogger<AbiService> _logger;
        private readonly Dictionary<string, string> _abiCache = new();
        private readonly string _abiDirectory;

        public AbiService(ILogger<AbiService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _abiDirectory = configuration["AbiSettings:Directory"] ?? "abis";
        }

        public async Task<string> GetContractAbiAsync(string contractName)
        {
            if (_abiCache.TryGetValue(contractName, out var cachedAbi))
                return cachedAbi;

            var abiPath = Path.Combine(_abiDirectory, $"{contractName}.json");

            if (!File.Exists(abiPath))
            {
                _logger.LogWarning($"ABI file not found: {abiPath}");
                return GetFallbackAbi(contractName);
            }

            try
            {
                var abiJson = await File.ReadAllTextAsync(abiPath);
                _abiCache[contractName] = abiJson;
                return abiJson;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading ABI for {contractName}");
                return GetFallbackAbi(contractName);
            }
        }

        public async Task RefreshAbisAsync()
        {
            _abiCache.Clear();

            if (!Directory.Exists(_abiDirectory))
            {
                Directory.CreateDirectory(_abiDirectory);
                return;
            }

            var abiFiles = Directory.GetFiles(_abiDirectory, "*.json");

            foreach (var file in abiFiles)
            {
                var contractName = Path.GetFileNameWithoutExtension(file);
                try
                {
                    var abi = await File.ReadAllTextAsync(file);
                    _abiCache[contractName] = abi;
                    _logger.LogInformation($"Loaded ABI for {contractName}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to load ABI from {file}");
                }
            }
        }

        private string GetFallbackAbi(string contractName)
        {
            // Return basic ERC20 ABI as fallback
            return contractName.ToLower() switch
            {
                "token" => ERC20_ABI,
                "presale" => BASIC_PRESALE_ABI,
                _ => ERC20_ABI
            };
        }

        private const string ERC20_ABI = @"[{""constant"":true,""inputs"":[],""name"":""totalSupply"",""outputs"":[{""name"":"""",""type"":""uint256""}],""type"":""function""}]";
        private const string BASIC_PRESALE_ABI = @"[{""constant"":false,""inputs"":[{""name"":""amount"",""type"":""uint256""}],""name"":""purchase"",""outputs"":[],""type"":""function""}]";
    }
}
