using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.DataAccess.Entities;

namespace PersonalFinanceTracker.BusinessLogic.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync(
        Guid userId,
        OperationType? type = null);

    Task<CategoryDto> CreateAsync(
        Guid userId,
        CreateCategoryRequest request);

    Task<CategoryDto> UpdateAsync(
        Guid userId,
        Guid id,
        UpdateCategoryRequest request);

    Task DeleteAsync(
        Guid userId,
        Guid id);
}