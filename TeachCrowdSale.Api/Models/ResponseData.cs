namespace TeachCrowdSale.Api.Models;

public class ResponseData
{
    public string TraceId { get; set; }
    public DateTime ResponseTime { get; set; }
    public int StatusCode { get; set; }
    public string ContentType { get; set; }
    public long ContentLength { get; set; }
    public long ExecutionTime { get; set; }
    public string Headers { get; set; }
    public string Body { get; set; }
}