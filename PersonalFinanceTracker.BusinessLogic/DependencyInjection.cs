using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonalFinanceTracker.BusinessLogic.Interfaces;
using PersonalFinanceTracker.BusinessLogic.Services;
using PersonalFinanceTracker.DataAccess;

namespace PersonalFinanceTracker.BusinessLogic;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogic(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDataAccess(configuration);

        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICategoryService, CategoryService>();

        return services;
    }
}