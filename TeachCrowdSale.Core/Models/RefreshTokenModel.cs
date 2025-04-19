using System.ComponentModel.DataAnnotations;

namespace TeachCrowdSale.Core.Models;

public class RefreshTokenModel
{
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
}