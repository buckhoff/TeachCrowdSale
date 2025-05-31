namespace TeachCrowdSale.Core.Interfaces.Services;

public interface ITokenContractService
{
    Task<decimal> GetTotalSupplyAsync();
    Task<decimal> GetCirculatingSupplyAsync();
    Task<decimal> GetTokenPriceAsync();
    Task<int> GetHoldersCountAsync();
    Task<decimal> CalculateMarketCapAsync();
    Task<decimal> GetVolume24hAsync();
    Task<decimal> GetPriceChange24hAsync();
    Task<decimal> GetBurnedTokensAsync();
    Task<decimal> GetStakedTokensAsync();
    Task<decimal> GetLiquidityTokensAsync();
}