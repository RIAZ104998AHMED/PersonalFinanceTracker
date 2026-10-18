using PersonalFinanceTracker.BusinessLogic.DTOs;

namespace PersonalFinanceTracker.BusinessLogic.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);

    Task<AuthResponse?> LoginAsync(LoginRequest request);
}