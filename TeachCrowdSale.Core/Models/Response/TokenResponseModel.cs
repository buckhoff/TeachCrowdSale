using System.ComponentModel.DataAnnotations;

namespace TeachCrowdSale.Core.Models.Response;

public class TokenResponseModel
{
    [Required]
    public string AccessToken { get; set; } = string.Empty;
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}