using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models;

/// <summary>
/// Contract addresses model for web consumption
/// Consolidates ContractAddresses and ContractAddressesModel
/// </summary>
public class ContractAddressesModel
{
    [EthereumAddress]
    public string PresaleAddress { get; set; } = string.Empty;

    [EthereumAddress]
    public string TokenAddress { get; set; } = string.Empty;

    [EthereumAddress]
    public string PaymentTokenAddress { get; set; } = string.Empty;

    [EthereumAddress]
    public string StabilityFundAddress { get; set; } = string.Empty;

    [EthereumAddress]
    public string StakingAddress { get; set; } = string.Empty;

    [EthereumAddress]
    public string GovernanceAddress { get; set; } = string.Empty;

    [EthereumAddress]
    public string MarketplaceAddress { get; set; } = string.Empty;

    [EthereumAddress]
    public string RewardAddress { get; set; } = string.Empty;

    [EthereumAddress]
    public string RegistryAddress { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int NetworkId { get; set; }

    public string ChainName { get; set; } = string.Empty;
}