using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Models;
using DatabaseService.Models;
using DatabaseService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services;

public class AuthService : IAuthService
{
    private readonly IDatabaseService _dbService;
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthService(IDatabaseService dbService, IConfiguration configuration, UserManager<ApplicationUser> userManager)
    {
        _dbService = dbService;
        _configuration = configuration;
        _userManager = userManager;
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _dbService.GetUserByEmailAsync(request.Email);
        if (existingUser != null) return false;

        var user = new ApplicationUser
        { 
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded) return false;
        
        if (!string.IsNullOrEmpty(request.Role))
        {
            await _userManager.AddToRoleAsync(user, request.Role);
        }

        await SendConfirmationEmailAsync(request.Email);
        return true;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return new AuthResponse { Token = "", Role = "" };

        // if (!user.IsEmailConfirmed)
        //     return new AuthResponse { Token = "", Role = "", RefreshToken = "Email not confirmed" };

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Candidate";

        var token = GenerateJwtToken(user, role);
        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        await _dbService.CreateRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiry);

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            Role = role
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var tokenEntity = await _dbService.GetRefreshTokenAsync(request.RefreshToken);
        if (tokenEntity == null || !tokenEntity.IsActive)
            return new AuthResponse { Token = "", Role = "" };

        var user = await _dbService.GetUserByIdAsync(tokenEntity.UserId);
        if (user == null) return new AuthResponse { Token = "", Role = "" };

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Candidate";

        var newToken = GenerateJwtToken(user, role);
        var newRefreshToken = GenerateRefreshToken();
        var newExpiry = DateTime.UtcNow.AddDays(7);

        await _dbService.UpdateRefreshTokenAsync(request.RefreshToken, newRefreshToken, newExpiry);

        return new AuthResponse
        { 
            Token = newToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            Role = role
        };
    }

    public async Task<bool> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _dbService.GetUserByIdAsync(userId);
        
        if (user == null) return false;

        var result = await _userManager.ConfirmEmailAsync(user, token);
        
        if (result.Succeeded)
        {
            user.IsEmailConfirmed = true;
            await _dbService.SaveUserAsync(user);
        }

        return result.Succeeded;
    }

    public async Task<bool> SendConfirmationEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return false;

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        // TODO: отправить email через SMTP/Service (например, SendGrid)
        Console.WriteLine($"Confirmation token for {email}: {token}");
        return true;
    }

    private string GenerateJwtToken(ApplicationUser user, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "default_secret_key"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Role, role),
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }
}