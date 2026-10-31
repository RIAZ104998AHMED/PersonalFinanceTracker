using PersonalFinanceTracker.DataAccess.Entities;

namespace PersonalFinanceTracker.BusinessLogic.DTOs;

public class OperationQueryParameters
{
    public Guid? AccountId { get; set; }

    public Guid? CategoryId { get; set; }

    public OperationType? Type { get; set; }

    public DateOnly? From { get; set; }

    public DateOnly? To { get; set; }

    public string SortBy { get; set; } = "dateDesc";

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}