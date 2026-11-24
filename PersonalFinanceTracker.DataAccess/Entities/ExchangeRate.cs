namespace PersonalFinanceTracker.DataAccess.Entities;

public class ExchangeRate
{
    public DateOnly Date { get; set; }

    public string Currency { get; set; } = string.Empty;

    public decimal Rate { get; set; }
}