namespace TeachCrowdSale.Core.Interfaces;

public interface ITokenContractService
{
    Task<decimal> GetTotalSupplyAsync();
    Task<decimal> GetCirculatingSupplyAsync();
    Task<decimal> GetTokenPriceAsync();
    Task<int> GetHoldersCountAsync();
    Task<decimal> CalculateMarketCapAsync();
}