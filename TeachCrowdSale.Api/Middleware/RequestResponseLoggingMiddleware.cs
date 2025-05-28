using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Api.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private readonly RequestResponseLoggingOptions _options;

        public RequestResponseLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestResponseLoggingMiddleware> logger,
            RequestResponseLoggingOptions options = null)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
            _options = options ?? new RequestResponseLoggingOptions();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!ShouldLog(context))
            {
                await _next(context);
                return;
            }

            // Copy a pointer to the original response body stream
            var originalBodyStream = context.Response.Body;

            // Create a new memory stream for the request
            await using var requestBodyStream = _recyclableMemoryStreamManager.GetStream();
            
            // Log the request
            var request = await LogRequest(context, requestBodyStream);

            // Create a new memory stream for the response
            await using var responseBodyStream = _recyclableMemoryStreamManager.GetStream();
            
            // Replace the response body with our memory stream
            context.Response.Body = responseBodyStream;

            // Create a stopwatch to measure execution time
            var sw = Stopwatch.StartNew();
            
            try
            {
                // Call the next middleware in the pipeline
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log the exception
                LogException(context, ex, sw.ElapsedMilliseconds);
                throw;
            }
            
            sw.Stop();

            // Log the response
            await LogResponse(context, responseBodyStream, request, sw.ElapsedMilliseconds);

            // Copy the response stream back to the original stream
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(originalBodyStream);
            
            // Restore the original stream
            context.Response.Body = originalBodyStream;
        }

        private async Task<RequestData> LogRequest(HttpContext context, MemoryStream requestBodyStream)
        {
            // Allow reading the request body multiple times
            context.Request.EnableBuffering();
            
            // Copy the request body to our memory stream
            await context.Request.Body.CopyToAsync(requestBodyStream);
            
            // Reset the position of both streams
            requestBodyStream.Seek(0, SeekOrigin.Begin);
            context.Request.Body.Seek(0, SeekOrigin.Begin);
            
            // Read the request body
            var requestBodyText = await new StreamReader(requestBodyStream, Encoding.UTF8).ReadToEndAsync();
            
            // Create request data for logging
            var request = new RequestData
            {
                TraceId = context.TraceIdentifier,
                RequestTime = DateTime.UtcNow,
                Method = context.Request.Method,
                Path = context.Request.Path,
                QueryString = context.Request.QueryString.ToString(),
                ClientIp = context.Connection.RemoteIpAddress?.ToString(),
                Headers = _options.LogHeaders ? FormatHeaders(context.Request.Headers) : null,
                Body = _options.LogRequestBody && !string.IsNullOrEmpty(requestBodyText) ? 
                    SanitizeRequestBody(requestBodyText) : null
            };
            
            // Log the request data
            _logger.LogInformation("HTTP Request: {Method} {Path} {QueryString} {ClientIp}", 
                request.Method, request.Path, request.QueryString, request.ClientIp);
            
            if (_options.LogRequestBody && !string.IsNullOrEmpty(request.Body))
            {
                _logger.LogDebug("Request Body: {Body}", request.Body);
            }
            
            return request;
        }

        private async Task LogResponse(HttpContext context, MemoryStream responseBodyStream, RequestData request, long executionTime)
        {
            // Reset the position of the response body stream
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            
            // Read the response body
            var responseBodyText = await new StreamReader(responseBodyStream, Encoding.UTF8).ReadToEndAsync();
            
            // Reset the position of the response body stream again
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            
            // Create response data for logging
            var response = new ResponseData
            {
                TraceId = context.TraceIdentifier,
                ResponseTime = DateTime.UtcNow,
                StatusCode = context.Response.StatusCode,
                ContentType = context.Response.ContentType,
                ContentLength = responseBodyStream.Length,
                ExecutionTime = executionTime,
                Headers = _options.LogHeaders ? FormatHeaders(context.Response.Headers) : null,
                Body = _options.LogResponseBody ? SanitizeResponseBody(responseBodyText) : null
            };
            
            // Log the response data
            _logger.LogInformation(
                "HTTP Response: {StatusCode} {ContentType} {ContentLength} bytes in {ExecutionTime}ms for {Method} {Path}", 
                response.StatusCode, response.ContentType, response.ContentLength, 
                response.ExecutionTime, request.Method, request.Path);
            
            if (_options.LogResponseBody && !string.IsNullOrEmpty(response.Body))
            {
                _logger.LogDebug("Response Body: {Body}", response.Body);
            }
        }

        private void LogException(HttpContext context, Exception exception, long executionTime)
        {
            _logger.LogError(
                exception, 
                "HTTP Request failed after {ExecutionTime}ms: {Method} {Path} {QueryString}", 
                executionTime, context.Request.Method, context.Request.Path, 
                context.Request.QueryString.ToString());
        }

        private bool ShouldLog(HttpContext context)
        {
            // Skip logging for health checks, metrics, etc.
            if (_options.PathsToIgnore != null)
            {
                foreach (var path in _options.PathsToIgnore)
                {
                    if (context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }

        private string FormatHeaders(IHeaderDictionary headers)
        {
            var sb = new StringBuilder();
            
            foreach (var header in headers)
            {
                // Skip sensitive headers
                if (_options.SensitiveHeaders.Contains(header.Key.ToLower()))
                {
                    continue;
                }
                
                sb.AppendLine($"{header.Key}: {header.Value}");
            }
            
            return sb.ToString();
        }

        private string SanitizeRequestBody(string body)
        {
            if (body.Length > _options.MaxBodyLogLength)
            {
                return body.Substring(0, _options.MaxBodyLogLength) + "... (truncated)";
            }
            
            // Redact sensitive information
            return RedactSensitiveInfo(body);
        }

        private string SanitizeResponseBody(string body)
        {
            if (body.Length > _options.MaxBodyLogLength)
            {
                return body.Substring(0, _options.MaxBodyLogLength) + "... (truncated)";
            }
            
            // Redact sensitive information
            return RedactSensitiveInfo(body);
        }

        private string RedactSensitiveInfo(string text)
        {
            // Redact API keys
            text = _options.ApiKeyPattern.Replace(text, "\"apiKey\":\"[REDACTED]\"");
            
            // Redact private keys
            text = _options.PrivateKeyPattern.Replace(text, "\"privateKey\":\"[REDACTED]\"");
            
            // Redact wallet addresses (optional depending on your privacy needs)
            if (_options.RedactWalletAddresses)
            {
                text = _options.WalletAddressPattern.Replace(text, m => 
                {
                    var address = m.Value;
                    // Keep first 6 and last 4 characters, replace the rest with asterisks
                    if (address.Length > 10)
                    {
                        return address.Substring(0, 6) + "****" + address.Substring(address.Length - 4);
                    }
                    return address;
                });
            }
            
            return text;
        }
    }

   
}