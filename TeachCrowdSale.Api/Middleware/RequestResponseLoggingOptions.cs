namespace TeachCrowdSale.Api.Middleware;

public class RequestResponseLoggingOptions
{
    public bool LogRequestBody { get; set; } = true;
    public bool LogResponseBody { get; set; } = true;
    public bool LogHeaders { get; set; } = false;
    public bool RedactWalletAddresses { get; set; } = false;
    public int MaxBodyLogLength { get; set; } = 4096;
    public HashSet<string> SensitiveHeaders { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "Cookie",
        "X-Api-Key"
    };
    public string[] PathsToIgnore { get; set; } = new[]
    {
        "/health",
        "/metrics",
        "/favicon.ico"
    };
    public System.Text.RegularExpressions.Regex ApiKeyPattern { get; set; } = 
        new System.Text.RegularExpressions.Regex("\"apiKey\":\"[^\"]+\"", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    public System.Text.RegularExpressions.Regex PrivateKeyPattern { get; set; } = 
        new System.Text.RegularExpressions.Regex("\"privateKey\":\"[^\"]+\"", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    public System.Text.RegularExpressions.Regex WalletAddressPattern { get; set; } = 
        new System.Text.RegularExpressions.Regex("0x[a-fA-F0-9]{40}", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
}