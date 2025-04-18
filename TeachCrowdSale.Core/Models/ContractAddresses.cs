namespace TeachCrowdSale.Core.Models;

public class ContractAddresses
{
    public string PresaleAddress { get; set; }
    public string TokenAddress { get; set; }
    public string PaymentTokenAddress { get; set; }
    public string StabilityFundAddress { get; set; }
    public string StakingAddress { get; set; }
    public string GovernanceAddress { get; set; }
    public string MarketplaceAddress { get; set; }
    public string RewardAddress { get; set; }
    public string RegistryAddress { get; set; }
    public int NetworkId { get; set; }
    public string ChainName { get; set; }
}