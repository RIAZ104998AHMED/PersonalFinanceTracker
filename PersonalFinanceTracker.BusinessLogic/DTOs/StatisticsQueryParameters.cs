namespace PersonalFinanceTracker.BusinessLogic.DTOs;

public class StatisticsQueryParameters
{
    public DateOnly? From { get; set; }

    public DateOnly? To { get; set; }

    public Guid? AccountId { get; set; }

    public string? Currency { get; set; }

    public string GroupBy { get; set; } = "day";
}