using PersonalFinanceTracker.BusinessLogic.DTOs;

namespace PersonalFinanceTracker.BusinessLogic.Interfaces;

public interface ITransferService
{
    Task<PagedResult<TransferDto>> GetAllAsync(
        Guid userId,
        TransferQueryParameters parameters);

    Task<TransferDto> GetByIdAsync(
        Guid userId,
        Guid id);

    Task<TransferDto> CreateAsync(
        Guid userId,
        CreateTransferRequest request);

    Task DeleteAsync(
        Guid userId,
        Guid id);
}