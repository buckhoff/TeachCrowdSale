using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TeachCrowdSale.Core.Models.Configuration;
using TeachCrowdSale.Core.Models;
using TeachCrowdSale.Core.Interfaces.Services;

namespace TeachCrowdSale.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly JwtSettings _jwtSettings;
    private readonly Dictionary<string, string> _refreshTokens = new();
    
    // In production, use a secure API key storage mechanism
    private readonly Dictionary<string, string> _apiKeys = new()
    {
        { "web-client", "site-specific-api-key" }
    };

    public AuthService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<TokenResponse> AuthenticateAsync(string apiKey)
    {
        if (!await ValidateApiKeyAsync(apiKey))
        {
            throw new UnauthorizedAccessException("Invalid API key");
        }

        var clientId = _apiKeys.FirstOrDefault(x => x.Value == apiKey).Key;
        
        var token = GenerateJwtToken(clientId);
        var refreshToken = GenerateRefreshToken();
        
        // Store refresh token (in a real app, use a database)
        _refreshTokens[refreshToken] = clientId;
        
        return new TokenResponse
        {
            AccessToken = token,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes)
        };
    }

    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
    {
        if (!_refreshTokens.TryGetValue(refreshToken, out var clientId))
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }
        
        // Remove the old refresh token
        _refreshTokens.Remove(refreshToken);
        
        // Generate new tokens
        var token = GenerateJwtToken(clientId);
        var newRefreshToken = GenerateRefreshToken();
        
        // Store the new refresh token
        _refreshTokens[newRefreshToken] = clientId;
        
        return new TokenResponse
        {
            AccessToken = token,
            RefreshToken = newRefreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes)
        };
    }

    public Task<bool> ValidateApiKeyAsync(string apiKey)
    {
        return Task.FromResult(_apiKeys.ContainsValue(apiKey));
    }

    private string GenerateJwtToken(string clientId)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, clientId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "Client")
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}