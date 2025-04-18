namespace TeachCrowdSale.Infrastructure.Configuration;

public class BlockchainSettings
{
    public string RpcUrl { get; set; }
    public int NetworkId { get; set; }
    public string AdminPrivateKey { get; set; }
    public string PresaleAddress { get; set; }
    public string TokenAddress { get; set; }
    public string PaymentTokenAddress { get; set; }
    public string StabilityFundAddress { get; set; }
    public string StakingAddress { get; set; }
    public string GovernanceAddress { get; set; }
    public string MarketplaceAddress { get; set; }
    public string RewardAddress { get; set; }
    public string RegistryAddress { get; set; }
    public int GasLimit { get; set; } = 500000;
    public int GasPrice { get; set; } = 50; // Gwei
    public int TransactionTimeout { get; set; } = 180; // Seconds
}