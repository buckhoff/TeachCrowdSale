using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models;

public class ContractAddresses
{
    [EthereumAddress]
    public string PresaleAddress { get; set; }
    [EthereumAddress]
    public string TokenAddress { get; set; }
    [EthereumAddress]
    public string PaymentTokenAddress { get; set; }
    [EthereumAddress]
    public string StabilityFundAddress { get; set; }
    [EthereumAddress]
    public string StakingAddress { get; set; }
    [EthereumAddress]
    public string GovernanceAddress { get; set; }
    [EthereumAddress]
    public string MarketplaceAddress { get; set; }
    [EthereumAddress]
    public string RewardAddress { get; set; }
    [EthereumAddress]
    public string RegistryAddress { get; set; }
    [Range(1, int.MaxValue)]
    public int NetworkId { get; set; }
    public string ChainName { get; set; }
}