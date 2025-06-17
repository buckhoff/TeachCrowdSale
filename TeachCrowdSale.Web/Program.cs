using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Nethereum.JsonRpc.Client;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Interfaces.Repositories;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Infrastructure.Configuration;
using TeachCrowdSale.Infrastructure.Data.Context;
using TeachCrowdSale.Infrastructure.Repositories;
using TeachCrowdSale.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.Configure<CacheConfiguration>(builder.Configuration.GetSection("CacheSettings"));
builder.Services.Configure<GitHubSettings>(builder.Configuration.GetSection("GitHub"));

// Add MVC services
builder.Services.AddControllersWithViews();

// Add memory cache
builder.Services.AddMemoryCache();
builder.Services.AddResponseCompression();

builder.Services.AddHttpClient();
var logging = builder.Configuration.GetSection("Logging");

builder.Services.AddLogging(builder =>
{
    builder.AddConfiguration(logging);

    // Add custom filters for roadmap components
    builder.AddFilter("TeachCrowdSale.Web.Services.RoadmapDashboardService", LogLevel.Information);
    builder.AddFilter("TeachCrowdSale.Web.Controllers.RoadmapController", LogLevel.Information);
});


builder.Services.AddDbContext<TeachCrowdSaleDbContext>(options =>
    options.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"]));

// Add HTTP client for API calls
builder.Services.AddHttpClient("TeachAPI", client =>
{
    var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7054";
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Analytics-specific HTTP client configuration
builder.Services.AddHttpClient("AnalyticsAPI", client =>
{
    var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7054";
    client.BaseAddress = new Uri($"{apiBaseUrl}/api/analytics/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(45); // Longer timeout for analytics data
});
builder.Services.AddHttpClient<IStakingDashboardService, StakingDashboardService>("StakingApi", client =>
{
    var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7054";
    client.BaseAddress = new Uri($"{apiBaseUrl}/api/staking/");
    client.DefaultRequestHeaders.Add("User-Agent", "TeachCrowdSale-Web/1.0");
    client.Timeout = TimeSpan.FromSeconds(30);
});


builder.Services.AddHttpClient("GitHubApi", client =>
{
    client.BaseAddress = new Uri("https://api.github.com/");
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
    client.DefaultRequestHeaders.Add("User-Agent", "TeachCrowdSale-Web");

    // Add GitHub token if configured
    var githubToken = builder.Configuration.GetValue<string>("GitHub:AccessToken");
    if (!string.IsNullOrEmpty(githubToken))
    {
        client.DefaultRequestHeaders.Add("Authorization", $"token {githubToken}");
    }
});

builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IBuyTradeService, BuyTradeService>();
builder.Services.AddScoped<ITokenomicsService, TokenomicsService>();
builder.Services.AddScoped<IAnalyticsDashboardService, AnalyticsDashboardService>();
builder.Services.AddScoped<IStakingDashboardService, StakingDashboardService>();


// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseResponseCompression();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "analytics",
    pattern: "analytics/{action=Index}/{id?}",
    defaults: new { controller = "Analytics" });

app.MapControllerRoute(
    name: "staking",
    pattern: "staking/{action=Index}/{id?}",
    defaults: new { controller = "Staking" });

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
