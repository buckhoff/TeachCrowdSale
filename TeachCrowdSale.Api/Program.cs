
using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
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
using TeachCrowdSale.Infrastructure.Web3;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using TeachCrowdSale.Infrastructure.Data.Context;
using TeachCrowdSale.Infrastructure.Repositories;
using TeachCrowdSale.Api.Middleware;
using TeachCrowdSale.Api.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddProblemDetails();

DotNetEnv.Env.Load();

builder.Services.AddHttpClient();
 

builder.Services.AddDatabaseServices(builder.Configuration);
builder.Services.AddApiControllers();
builder.Services.AddBusinessServices();
builder.Services.AddValidationServices();
builder.Services.AddConfigurationServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddRateLimitingServices();
builder.Services.AddCorsServices();
builder.Services.AddCompressionServices();
builder.Services.AddTokenomicsModule(builder.Configuration);
builder.Services.AddStakingServices();
builder.Services.AddStakingRateLimiting();
builder.Services.AddLiquidityModule(builder.Configuration);


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
    app.UseRateLimiter();
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