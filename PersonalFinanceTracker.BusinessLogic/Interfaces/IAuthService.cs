using PersonalFinanceTracker.BusinessLogic.DTOs;

namespace PersonalFinanceTracker.BusinessLogic.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(
        RegisterRequest request);

    Task<AuthResponse?> LoginAsync(
        LoginRequest request);

    Task<AuthResponse?> RefreshAsync(
        RefreshTokenRequest request);

    Task<bool> LogoutAsync(
        Guid userId,
        LogoutRequest request);
}