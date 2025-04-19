using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using TeachCrowdSale.Api.Models;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Api.ModelBinding
{
    public class CustomProblemDetailsFactory : ProblemDetailsFactory
    {
        private readonly ApiBehaviorOptions _options;
        
        public CustomProblemDetailsFactory(IOptions<ApiBehaviorOptions> options)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }
        
        public override ProblemDetails CreateProblemDetails(
            HttpContext httpContext,
            int? statusCode = null,
            string title = null,
            string type = null,
            string detail = null,
            string instance = null)
        {
            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Type = type,
                Detail = detail,
                Instance = instance
            };
            
            // Add trace ID
            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
            
            return problemDetails;
        }
        
        public override ValidationProblemDetails CreateValidationProblemDetails(
            HttpContext httpContext,
            ModelStateDictionary modelStateDictionary,
            int? statusCode = null,
            string title = null,
            string type = null,
            string detail = null,
            string instance = null)
        {
            // Convert ModelState errors to the format we want
            var errors = new Dictionary<string, string[]>();
            
            foreach (var keyModelStatePair in modelStateDictionary)
            {
                var key = keyModelStatePair.Key;
                var errorMessages = keyModelStatePair.Value.Errors
                    .Select(error => error.ErrorMessage)
                    .ToArray();
                
                if (errorMessages.Any())
                {
                    errors.Add(key, errorMessages);
                }
            }
            
            // Create custom error response
            var errorResponse = new ErrorResponse
            {
                Message = title ?? "One or more validation errors occurred",
                TraceId = httpContext.TraceIdentifier,
                ValidationErrors = errors
            };
            
            // Set status code
            httpContext.Response.StatusCode = statusCode ?? StatusCodes.Status400BadRequest;
            
            // Create validation problem details
            var validationProblemDetails = new ValidationProblemDetails(modelStateDictionary)
            {
                Status = statusCode,
                Title = title,
                Type = type,
                Detail = detail,
                Instance = instance
            };
            
            // Add trace ID
            validationProblemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
            
            // Add our custom error response
            validationProblemDetails.Extensions["errors"] = errors;
            
            return validationProblemDetails;
        }
    }
}