using DatabaseService.Models;

namespace DatabaseService.Services;

public interface IDatabaseService
{
    Task<ApplicationUser?> GetUserByEmailAsync(string email);
    Task<ApplicationUser?> GetUserByIdAsync(string id);
    Task<bool> SaveUserAsync(ApplicationUser user);
    Task<bool> CreateRefreshTokenAsync(string userId, string token, DateTime expiry);
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task<bool> RevokeRefreshTokenAsync(string token);
    Task<bool> UpdateRefreshTokenAsync(string oldToken, string newToken, DateTime expiry);
}