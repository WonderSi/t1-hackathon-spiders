using System.ComponentModel.DataAnnotations;

namespace AuthService.Models;

public class OAuthLoginRequest
{
    [Required] public string Provider { get; set; } = string.Empty;
    [Required] public string Code { get; set; } = string.Empty;
}