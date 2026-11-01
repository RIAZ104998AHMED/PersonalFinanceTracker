namespace PersonalFinanceTracker.BusinessLogic.DTOs;

public class StatisticsDto
{
    public DateOnly From { get; set; }

    public DateOnly To { get; set; }

    public string? Currency { get; set; }

    public Guid? AccountId { get; set; }

    public decimal TotalIncome { get; set; }

    public decimal TotalExpense { get; set; }

    public decimal Net { get; set; }

    public IEnumerable<CategoryStatisticsDto> ByCategory { get; set; }
        = Enumerable.Empty<CategoryStatisticsDto>();

    public IEnumerable<TimelineStatisticsDto> Timeline { get; set; }
        = Enumerable.Empty<TimelineStatisticsDto>();
}