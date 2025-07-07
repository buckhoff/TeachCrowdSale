using Microsoft.EntityFrameworkCore;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models.Configuration;
using TeachCrowdSale.Infrastructure.Data.Context;
using TeachCrowdSale.Infrastructure.Data.SeedData;
using TeachCrowdSale.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TeachCrowdSaleDbContext>(options =>
    options.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"]));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "PencilImpact.Session";
});

// Add memory cache and response compression
builder.Services.AddMemoryCache();
builder.Services.AddResponseCompression();

builder.Services.AddScoped<IPlatformDashboardService,PlatformDashboardService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddHttpClient();

builder.Services.AddHttpClient<IPlatformDashboardService, PlatformDashboardService>("PlatformAPI", client =>
{
    var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7054";
    client.BaseAddress = new Uri($"{apiBaseUrl}/api/platform/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "PencilImpact-Web/1.0");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddLogging(logging =>
{
    logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
    logging.AddFilter("PencilImpact.Web.Concept", LogLevel.Information);
});

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

// Add services to the container.
builder.Services.AddControllersWithViews();
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
app.UseSession();
app.UseAuthorization();

app.MapRazorPages().WithStaticAssets();
app.MapStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TeachCrowdSaleDbContext>();

    // Ensure database is created and migrated
    await context.Database.MigrateAsync();

    // Seed PencilImpact data
    await PencilImpactSeeder.SeedAsync(context);

    // Optionally seed development data (only in development environment)
    if (app.Environment.IsDevelopment())
    {
        await PencilImpactSeeder.SeedDevelopmentDataAsync(context);
    }
}

// PencilImpact.Web.Concept/Program.cs - Add this routing configuration

app.MapControllerRoute(
    name: "home_vision",
    pattern: "vision",
    defaults: new { controller = "Home", action = "Vision" });

app.MapControllerRoute(
    name: "home_demo",
    pattern: "demo",
    defaults: new { controller = "Home", action = "Demo" });

app.MapControllerRoute(
    name: "home_roadmap",
    pattern: "roadmap",
    defaults: new { controller = "Home", action = "Roadmap" });

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
