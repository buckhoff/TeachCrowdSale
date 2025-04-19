using TeachCrowdSale.Api.Models;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Web.Services;

public class ApiAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private string? _cachedToken;
    private string? _refreshToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public ApiAuthService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> GetTokenAsync()
    {
        if (_cachedToken != null && DateTime.UtcNow < _tokenExpiry)
        {
            return _cachedToken;
        }

        if (_refreshToken != null)
        {
            await RefreshTokenAsync();
            return _cachedToken!;
        }

        return await AuthenticateAsync();
    }

    private async Task<string> AuthenticateAsync()
    {
        var apiKey = _configuration["ApiSettings:ApiKey"];
        var response = await _httpClient.PostAsJsonAsync("api/auth/token", new { ApiKey = apiKey });
        
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<TokenResponseModel>();
        _cachedToken = result!.AccessToken;
        _refreshToken = result.RefreshToken;
        _tokenExpiry = result.ExpiresAt.AddMinutes(-5); // Refresh 5 minutes early
        
        return _cachedToken;
    }

    private async Task RefreshTokenAsync()
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/refresh", new { RefreshToken = _refreshToken });
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TokenResponseModel>();
                _cachedToken = result!.AccessToken;
                _refreshToken = result.RefreshToken;
                _tokenExpiry = result.ExpiresAt.AddMinutes(-5);
            }
            else
            {
                // If refresh fails, authenticate again
                await AuthenticateAsync();
            }
        }
        catch
        {
            // If refresh fails, authenticate again
            await AuthenticateAsync();
        }
    }
}