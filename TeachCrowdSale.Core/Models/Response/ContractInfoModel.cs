namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Contract information model
    /// </summary>
    public class ContractInfoModel
    {
        public string PresaleAddress { get; set; } = string.Empty;
        public string TokenAddress { get; set; } = string.Empty;
        public string PaymentTokenAddress { get; set; } = string.Empty;
        public int NetworkId { get; set; }
        public string ChainName { get; set; } = string.Empty;
    }
}
