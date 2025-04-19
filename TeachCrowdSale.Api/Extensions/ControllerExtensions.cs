using TeachCrowdSale.Core.Models.Response;
using Microsoft.AspNetCore.Mvc;

public static class ControllerExtensions
{
    public static IMvcBuilder AddApiControllerConventions(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.ConfigureApiBehaviorOptions(options =>
        {
            // Customize the response for model validation errors
            options.InvalidModelStateResponseFactory = context =>
            {
                var problemDetails = new ValidationProblemDetails(context.ModelState)
                {
                    Title = "One or more validation errors occurred",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = context.HttpContext.Request.Path
                };
                
                // Convert to our custom error response
                var errors = new Dictionary<string, string[]>();
                
                foreach (var keyModelStatePair in context.ModelState)
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
                
                var errorResponse = new ErrorResponse
                {
                    Message = "Validation failed",
                    TraceId = context.HttpContext.TraceIdentifier,
                    ValidationErrors = errors
                };
                
                return new BadRequestObjectResult(errorResponse);
            };
        });
        
        return mvcBuilder;
    }
}