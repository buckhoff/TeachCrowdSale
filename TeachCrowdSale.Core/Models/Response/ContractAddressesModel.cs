using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Response;

public class ContractAddressesModel
{
    [EthereumAddress]
    public string PresaleAddress { get; set; }
    [EthereumAddress]
    public string TokenAddress { get; set; }
    [EthereumAddress]
    public string PaymentTokenAddress { get; set; }
    [Range(1, int.MaxValue)]
    public int NetworkId { get; set; }
    public string ChainName { get; set; }
}