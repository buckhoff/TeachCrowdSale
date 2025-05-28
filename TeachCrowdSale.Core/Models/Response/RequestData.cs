namespace TeachCrowdSale.Core.Models.Response;

public class RequestData
{
    public string TraceId { get; set; }
    public DateTime RequestTime { get; set; }
    public string Method { get; set; }
    public string Path { get; set; }
    public string QueryString { get; set; }
    public string ClientIp { get; set; }
    public string Headers { get; set; }
    public string Body { get; set; }
}