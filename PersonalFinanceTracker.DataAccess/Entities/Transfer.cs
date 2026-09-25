namespace PersonalFinanceTracker.DataAccess.Entities;

public class Transfer
{
    public Guid Id { get; set; }

    public Guid FromAccountId { get; set; }

    public Guid ToAccountId { get; set; }

    public decimal Amount { get; set; }

    public DateOnly Date { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }
}