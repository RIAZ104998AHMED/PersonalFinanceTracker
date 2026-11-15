namespace PersonalFinanceTracker.Api.Services;

public interface ICurrentUser
{
    Guid? UserId { get; }
}