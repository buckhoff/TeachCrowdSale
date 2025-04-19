using TeachCrowdSale.Core.Models;

namespace TeachCrowdSale.Core.Interfaces;

public interface IAuthService
{
    Task<TokenResponse> AuthenticateAsync(string apiKey);
    Task<TokenResponse> RefreshTokenAsync(string refreshToken);
    Task<bool> ValidateApiKeyAsync(string apiKey);
}