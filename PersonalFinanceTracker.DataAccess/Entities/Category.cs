namespace PersonalFinanceTracker.DataAccess.Entities;

public class Category
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public OperationType Type { get; set; }
}