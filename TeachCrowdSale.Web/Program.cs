using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MVC services
builder.Services.AddControllersWithViews();

// Add memory cache
builder.Services.AddMemoryCache();
builder.Services.AddResponseCompression();

builder.Services.AddHttpClient();

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

builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IBuyTradeService, BuyTradeService>();
builder.Services.AddScoped<ITokenomicsService, TokenomicsService>();
builder.Services.AddScoped<IRoadmapService, RoadmapService>();
builder.Services.AddScoped<IAnalyticsDashboardService, AnalyticsDashboardService>();


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

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
