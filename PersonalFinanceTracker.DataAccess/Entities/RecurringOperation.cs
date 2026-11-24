namespace PersonalFinanceTracker.DataAccess.Entities;

public class RecurringOperation
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    public Guid CategoryId { get; set; }

    public OperationType Type { get; set; }

    public decimal Amount { get; set; }

    public string? Comment { get; set; }

    public Frequency Frequency { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public DateOnly NextRunDate { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}