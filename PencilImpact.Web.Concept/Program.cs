using Microsoft.EntityFrameworkCore;
using TeachCrowdSale.Infrastructure.Data.Context;
using TeachCrowdSale.Infrastructure.Data.SeedData;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TeachCrowdSaleDbContext>(options =>
    options.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"]));

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

app.UseRouting();

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

app.Run();
