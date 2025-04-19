using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachCrowdSale.Api.Models;
using TeachCrowdSale.Core.Models.Request;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Models;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("token")]
    public async Task<ActionResult<TokenResponseModel>> GetToken([FromBody] TokenRequestModel request)
    {
        try
        {
            var result = await _authService.AuthenticateAsync(request.ApiKey);
            
            return Ok(new TokenResponseModel
            {
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken,
                ExpiresAt = result.Expiration
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Invalid API key" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving token", error = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponseModel>> RefreshToken([FromBody] RefreshTokenModel request)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            
            return Ok(new TokenResponseModel
            {
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken,
                ExpiresAt = result.Expiration
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Invalid refresh token" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error refreshing token", error = ex.Message });
        }
    }
}