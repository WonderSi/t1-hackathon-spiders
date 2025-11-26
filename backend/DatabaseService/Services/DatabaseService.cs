using DatabaseService.Data;
using DatabaseService.Models;
using Microsoft.EntityFrameworkCore;

namespace DatabaseService.Services;

public class DatabaseService : IDatabaseService
{
    private readonly DatabaseContext _context;

        public DatabaseService(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<bool> SaveUserAsync(ApplicationUser user)
        {
            _ = user.Id == null ? _context.Users.Add(user) : _context.Users.Update(user);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> CreateRefreshTokenAsync(string userId, string token, DateTime expiry)
        {
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = token,
                ExpiresAt = expiry
            };
            _context.RefreshTokens.Add(refreshToken);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task<bool> RevokeRefreshTokenAsync(string token)
        {
            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
            if (refreshToken == null) return false;

            refreshToken.IsRevoked = true;
            _ = _context.RefreshTokens.Update(refreshToken);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> UpdateRefreshTokenAsync(string oldToken, string newToken, DateTime expiry)
        {
            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == oldToken);
            if (refreshToken == null) return false;

            refreshToken.Token = newToken;
            refreshToken.ExpiresAt = expiry;
            refreshToken.ReplacedByToken = newToken;

            _ = _context.RefreshTokens.Update(refreshToken);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
}