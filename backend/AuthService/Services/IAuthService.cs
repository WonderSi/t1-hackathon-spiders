using AuthService.Models;

namespace AuthService.Services;

public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task<bool> ConfirmEmailAsync(string userId, string token);
    Task<bool> SendConfirmationEmailAsync(string email);
}