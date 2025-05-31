using TeachCrowdSale.Core.Models;

namespace TeachCrowdSale.Core.Interfaces.Services;

public interface IAuthService
{
    Task<TokenResponse> AuthenticateAsync(string apiKey);
    Task<TokenResponse> RefreshTokenAsync(string refreshToken);
    Task<bool> ValidateApiKeyAsync(string apiKey);
}