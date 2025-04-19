using Microsoft.AspNetCore.Builder;
using TeachCrowdSale.Api.Middleware;

namespace TeachCrowdSale.Api.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseRequestResponseLogging(
            this IApplicationBuilder builder, 
            RequestResponseLoggingOptions options = null)
        {
            return builder.UseMiddleware<RequestResponseLoggingMiddleware>(options ?? new RequestResponseLoggingOptions());
        }
        
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        }
    }
}