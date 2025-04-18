namespace TeachCrowdSale.Core.Models;

public class TokenInfo
{
    public string Name { get; set; }
    public string Symbol { get; set; }
    public int Decimals { get; set; }
    public decimal TotalSupply { get; set; }
    public decimal CirculatingSupply { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal MarketCap { get; set; }
    public decimal Volume24h { get; set; }
    public int HoldersCount { get; set; }
}