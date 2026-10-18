using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Interfaces;
using PersonalFinanceTracker.DataAccess.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PersonalFinanceTracker.BusinessLogic.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<AuthResponse?> RegisterAsync(
        RegisterRequest request)
    {
        var existingUser =
            await _userManager.FindByEmailAsync(request.Email);

        if (existingUser is not null)
        {
            return null;
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            Name = request.Name.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var result =
            await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return null;
        }

        return CreateToken(user);
    }

    public async Task<AuthResponse?> LoginAsync(
        LoginRequest request)
    {
        var user =
            await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            return null;
        }

        var validPassword =
            await _userManager.CheckPasswordAsync(
                user,
                request.Password);

        if (!validPassword)
        {
            return null;
        }

        return CreateToken(user);
    }

    private AuthResponse CreateToken(ApplicationUser user)
    {
        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT key is not configured.");

        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "JWT issuer is not configured.");

        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "JWT audience is not configured.");

        var accessTokenMinutesText =
    _configuration["Jwt:AccessTokenMinutes"];

        var accessTokenMinutes =
            int.TryParse(accessTokenMinutesText, out var minutes)
                ? minutes
                : 60;

        var expiresAt =
            DateTime.UtcNow.AddMinutes(accessTokenMinutes);

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new(
                JwtRegisteredClaimNames.Email,
                user.Email ?? string.Empty),

            new(
                "name",
                user.Name)
        };

        var securityKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));

        var credentials =
            new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new AuthResponse
        {
            AccessToken =
                new JwtSecurityTokenHandler()
                    .WriteToken(token),

            ExpiresAt = expiresAt,

            RefreshToken = null
        };
    }
}