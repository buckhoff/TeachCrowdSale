namespace TeachCrowdSale.Api.Models;

public class ContractAddressesModel
{
    public string PresaleAddress { get; set; }
    public string TokenAddress { get; set; }
    public string PaymentTokenAddress { get; set; }
    public int NetworkId { get; set; }
    public string ChainName { get; set; }
}