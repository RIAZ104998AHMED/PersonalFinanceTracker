using PersonalFinanceTracker.BusinessLogic.DTOs;

namespace PersonalFinanceTracker.BusinessLogic.Interfaces;

public interface IStatisticsService
{
    Task<StatisticsDto> GetAsync(
        Guid userId,
        StatisticsQueryParameters parameters);
}