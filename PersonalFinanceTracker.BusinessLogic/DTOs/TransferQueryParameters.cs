namespace PersonalFinanceTracker.BusinessLogic.DTOs;

public class TransferQueryParameters
{
    public Guid? AccountId { get; set; }

    public DateOnly? From { get; set; }

    public DateOnly? To { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}