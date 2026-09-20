namespace PersonalFinanceTracker.Api.Models
{
    public record AccountDto(
        Guid Id,
        string Name,
        decimal Balance
    );
}