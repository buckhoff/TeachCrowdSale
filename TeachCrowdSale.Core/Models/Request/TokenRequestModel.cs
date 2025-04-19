using System.ComponentModel.DataAnnotations;

namespace TeachCrowdSale.Core.Models.Request;

public class TokenRequestModel
{
    [Required(ErrorMessage = "API key is required")]
    [StringLength(64, MinimumLength = 32, ErrorMessage = "API key must be between 32 and 64 characters")]
    public string ApiKey { get; set; } = string.Empty;
}