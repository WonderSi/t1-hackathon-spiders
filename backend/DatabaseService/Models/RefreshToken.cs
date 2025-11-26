namespace DatabaseService.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;
    public bool IsRevoked { get; set; } = false;
    public string? ReplacedByToken { get; set; }
    public ApplicationUser User { get; set; } = null!;
}