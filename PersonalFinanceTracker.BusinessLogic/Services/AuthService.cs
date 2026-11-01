using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Interfaces;
using PersonalFinanceTracker.DataAccess.Data;
using PersonalFinanceTracker.DataAccess.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PersonalFinanceTracker.BusinessLogic.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly FinanceTrackerDbContext _dbContext;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        FinanceTrackerDbContext dbContext)
    {
        _userManager = userManager;
        _configuration = configuration;
        _dbContext = dbContext;
    }

    public async Task<AuthResponse?> RegisterAsync(
        RegisterRequest request)
    {
        var email = request.Email.Trim();

        var existingUser =
            await _userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            return null;
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            Name = request.Name.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var result =
            await _userManager.CreateAsync(
                user,
                request.Password);

        if (!result.Succeeded)
        {
            return null;
        }

        return await CreateTokenPairAsync(user);
    }

    public async Task<AuthResponse?> LoginAsync(
        LoginRequest request)
    {
        var email = request.Email.Trim();

        var user =
            await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return null;
        }

        var passwordValid =
            await _userManager.CheckPasswordAsync(
                user,
                request.Password);

        if (!passwordValid)
        {
            return null;
        }

        return await CreateTokenPairAsync(user);
    }

    public async Task<AuthResponse?> RefreshAsync(
        RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(
                request.RefreshToken))
        {
            return null;
        }

        var tokenHash =
            HashRefreshToken(
                request.RefreshToken);

        var existingToken =
            await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(
                    x => x.TokenHash == tokenHash);

        if (existingToken is null)
        {
            return null;
        }

        // Token already revoked
        if (existingToken.RevokedAt.HasValue)
        {
            return null;
        }

        // Token expired
        if (existingToken.ExpiresAt <=
            DateTime.UtcNow)
        {
            return null;
        }

        var user =
            await _userManager.FindByIdAsync(
                existingToken.UserId.ToString());

        if (user is null)
        {
            return null;
        }

        /*
         * Refresh-token rotation:
         *
         * 1. Generate a new opaque refresh token.
         * 2. Revoke the old refresh token.
         * 3. Link old token to the new token hash.
         * 4. Save the new refresh token.
         */

        var newRawRefreshToken =
            GenerateRefreshToken();

        var newTokenHash =
            HashRefreshToken(
                newRawRefreshToken);

        var now = DateTime.UtcNow;

        existingToken.RevokedAt = now;
        existingToken.ReplacedByTokenHash =
            newTokenHash;

        var refreshTokenDays =
            GetRefreshTokenDays();

        var newRefreshToken =
            new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = newTokenHash,
                CreatedAt = now,
                ExpiresAt =
                    now.AddDays(refreshTokenDays)
            };

        _dbContext.RefreshTokens.Add(
            newRefreshToken);

        await _dbContext.SaveChangesAsync();

        return CreateAccessTokenResponse(
            user,
            newRawRefreshToken);
    }

    public async Task<bool> LogoutAsync(
        Guid userId,
        LogoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(
                request.RefreshToken))
        {
            return false;
        }

        var tokenHash =
            HashRefreshToken(
                request.RefreshToken);

        var refreshToken =
            await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(
                    x => x.TokenHash == tokenHash);

        if (refreshToken is null)
        {
            return false;
        }

        // Token must belong to the authenticated user
        if (refreshToken.UserId != userId)
        {
            return false;
        }

        // Already revoked
        if (refreshToken.RevokedAt.HasValue)
        {
            return false;
        }

        // Expired token
        if (refreshToken.ExpiresAt <=
            DateTime.UtcNow)
        {
            return false;
        }

        refreshToken.RevokedAt =
            DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return true;
    }

    private async Task<AuthResponse>
        CreateTokenPairAsync(
            ApplicationUser user)
    {
        var rawRefreshToken =
            GenerateRefreshToken();

        var refreshToken =
            new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash =
                    HashRefreshToken(
                        rawRefreshToken),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt =
                    DateTime.UtcNow.AddDays(
                        GetRefreshTokenDays())
            };

        _dbContext.RefreshTokens.Add(
            refreshToken);

        await _dbContext.SaveChangesAsync();

        return CreateAccessTokenResponse(
            user,
            rawRefreshToken);
    }

    private AuthResponse CreateAccessTokenResponse(
        ApplicationUser user,
        string refreshToken)
    {
        var jwtKey =
            _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT key is not configured.");

        var jwtIssuer =
            _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "JWT issuer is not configured.");

        var jwtAudience =
            _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "JWT audience is not configured.");

        var accessTokenMinutes =
            GetAccessTokenMinutes();

        var now = DateTime.UtcNow;

        var expiresAt =
            now.AddMinutes(
                accessTokenMinutes);

        var claims =
            new List<Claim>
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
                Encoding.UTF8.GetBytes(
                    jwtKey));

        var credentials =
            new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                notBefore: now,
                expires: expiresAt,
                signingCredentials: credentials);

        var accessToken =
            new JwtSecurityTokenHandler()
                .WriteToken(token);

        return new AuthResponse
        {
            AccessToken = accessToken,
            ExpiresAt = expiresAt,
            RefreshToken = refreshToken
        };
    }

    private int GetAccessTokenMinutes()
    {
        var configuredValue =
            _configuration[
                "Jwt:AccessTokenMinutes"];

        var minutes =
            int.TryParse(
                configuredValue,
                out var parsed)
                ? parsed
                : 15;

        /*
         * Bonus requirement:
         * access token must not exceed
         * 15 minutes.
         */
        if (minutes < 1)
        {
            return 15;
        }

        return Math.Min(
            minutes,
            15);
    }

    private int GetRefreshTokenDays()
    {
        var configuredValue =
            _configuration[
                "Jwt:RefreshTokenDays"];

        return int.TryParse(
                   configuredValue,
                   out var days)
               && days > 0
            ? days
            : 7;
    }

    private static string GenerateRefreshToken()
    {
        var bytes =
            RandomNumberGenerator.GetBytes(64);

        return Convert.ToHexString(bytes);
    }

    private static string HashRefreshToken(
        string refreshToken)
    {
        var bytes =
            Encoding.UTF8.GetBytes(
                refreshToken);

        var hash =
            SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }
}