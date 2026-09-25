namespace PersonalFinanceTracker.DataAccess.Entities;

public class Operation
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    public Guid CategoryId { get; set; }

    public OperationType Type { get; set; }

    public decimal Amount { get; set; }

    public DateOnly Date { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }
}