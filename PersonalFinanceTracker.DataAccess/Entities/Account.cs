namespace PersonalFinanceTracker.DataAccess.Entities;

public class Account
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Currency { get; set; } = string.Empty;

    public decimal StartAmount { get; set; }

    public bool IsArchived { get; set; }

    public DateTime CreatedAt { get; set; }
}