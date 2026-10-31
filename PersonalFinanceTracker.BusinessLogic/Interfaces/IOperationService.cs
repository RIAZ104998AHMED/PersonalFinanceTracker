using PersonalFinanceTracker.BusinessLogic.DTOs;

namespace PersonalFinanceTracker.BusinessLogic.Interfaces;

public interface IOperationService
{
    Task<PagedResult<OperationDto>> GetAllAsync(
        Guid userId,
        OperationQueryParameters parameters);

    Task<OperationDto> GetByIdAsync(
        Guid userId,
        Guid id);

    Task<OperationDto> CreateAsync(
        Guid userId,
        CreateOperationRequest request);

    Task<OperationDto> UpdateAsync(
        Guid userId,
        Guid id,
        UpdateOperationRequest request);

    Task DeleteAsync(
        Guid userId,
        Guid id);
}