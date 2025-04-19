using TeachCrowdSale.Web.Services;

namespace TeachCrowdSale.Web.Helper;

public class AuthenticatedHttpClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApiAuthService _authService;

    public AuthenticatedHttpClientFactory(
        IHttpClientFactory httpClientFactory,
        ApiAuthService authService)
    {
        _httpClientFactory = httpClientFactory;
        _authService = authService;
    }

    public async Task<HttpClient> CreateClientAsync()
    {
        var client = _httpClientFactory.CreateClient("TeachApi");
        var token = await _authService.GetTokenAsync();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}