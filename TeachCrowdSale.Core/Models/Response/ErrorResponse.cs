namespace TeachCrowdSale.Core.Models.Response;

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string? TraceId { get; set; }
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
    public object? DeveloperDetails { get; set; }
}