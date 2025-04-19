
using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using TeachCrowdSale.Api.Converter;
using TeachCrowdSale.Api.Extensions;
using TeachCrowdSale.Api.ModelBinding;
using TeachCrowdSale.Api.Validator;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Models.Request;
using TeachCrowdSale.Infrastructure.Configuration;
using TeachCrowdSale.Infrastructure.Services;
using TeachCrowdSale.Infrastructure.Web3;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddProblemDetails();

// Add services to the container
builder.Services.AddControllers(options =>
    {
        // Add custom model binder providers
        options.ModelBinderProviders.Insert(0, new EthereumAddressModelBinderProvider());
        options.ModelValidatorProviders.Clear();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new EthereumAddressJsonConverter());
    })
    .AddApiControllerConventions();

// Configure options from appsettings
builder.Services.Configure<BlockchainSettings>(
    builder.Configuration.GetSection("Blockchain"));

// Configure Open API
builder.Services.AddOpenApi("TeachCrowdSale API");
builder.Services.AddFluentValidationAutoValidation();


// Add custom services
builder.Services.AddSingleton<Web3Helper>();
builder.Services.AddScoped<IBlockchainService, BlockchainService>();
builder.Services.AddScoped<IPresaleService, PresaleService>();
builder.Services.AddScoped<ITokenContractService, TokenContractService>();
builder.Services.AddSingleton<ProblemDetailsFactory, CustomProblemDetailsFactory>();
builder.Services.AddScoped<IValidator<PurchaseRequestModel>, PurchaseRequestValidator>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "https://localhost:5173",
            "http://localhost:5173",
            "https://teachtoken.io")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

// Add API response compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Configure JWT
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

// Add authentication
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

// Add services
builder.Services.AddScoped<IAuthService, AuthService>();


var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseGlobalExceptionHandling();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().DisableRateLimiting();
    app.UseDeveloperExceptionPage();
    // Add request/response logging in development
    app.UseRequestResponseLogging(new TeachCrowdSale.Api.Middleware.RequestResponseLoggingOptions
    {
        LogRequestBody = true,
        LogResponseBody = true,
        LogHeaders = true,
        RedactWalletAddresses = false, // In dev, we might want to see full addresses
        MaxBodyLogLength = 8192 // Larger value for dev
    });
}
else
{
    app.UseExceptionHandler("/api/error");
    app.UseHsts();
    
    // Add request/response logging in production with stricter settings
    app.UseRequestResponseLogging(new TeachCrowdSale.Api.Middleware.RequestResponseLoggingOptions
    {
        LogRequestBody = true,
        LogResponseBody = false, // Don't log response bodies in production
        LogHeaders = false,      // Don't log headers in production
        RedactWalletAddresses = true, // Redact addresses in production
        MaxBodyLogLength = 2048  // Smaller value for production
    });
}

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseCors();

app.MapControllers();

app.Run();