using System.ComponentModel.DataAnnotations;

namespace AuthService.Models;

public class RefreshTokenRequest
{
    [Required] public string RefreshToken { get; set; } = string.Empty;
}