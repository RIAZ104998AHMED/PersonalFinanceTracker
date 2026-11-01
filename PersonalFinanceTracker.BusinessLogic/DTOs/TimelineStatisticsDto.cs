namespace PersonalFinanceTracker.BusinessLogic.DTOs;

public record TimelineStatisticsDto(
    string Period,
    decimal Income,
    decimal Expense
);