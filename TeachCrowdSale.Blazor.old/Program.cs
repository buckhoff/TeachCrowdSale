using TeachCrowdSale.Web.Components;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using TeachCrowdSale.Web.Helper;
using TeachCrowdSale.Web.Services;
using TeachTokenCrowdsale.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();

builder.Services.AddScoped<TokenInfoService>();
builder.Services.AddScoped<PresaleService>();
builder.Services.AddScoped<WalletService>();
builder.Services.AddScoped<ContractService>();
builder.Services.AddScoped<ChartService>();
builder.Services.AddScoped<JsInteropService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ApiCorsPolicy", corsbuilder =>
    {
        corsbuilder.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>()
            )
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddHttpClient("TeachApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]);
});

builder.Services.AddScoped<ApiAuthService>();
builder.Services.AddScoped<AuthenticatedHttpClientFactory>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseCors("ApiCorsPolicy");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();