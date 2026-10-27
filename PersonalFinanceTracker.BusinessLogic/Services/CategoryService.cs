using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Exceptions;
using PersonalFinanceTracker.BusinessLogic.Interfaces;
using PersonalFinanceTracker.DataAccess.Data;
using PersonalFinanceTracker.DataAccess.Entities;

namespace PersonalFinanceTracker.BusinessLogic.Services;

public class CategoryService : ICategoryService
{
    private readonly FinanceTrackerDbContext _dbContext;

    public CategoryService(FinanceTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync(
        Guid userId,
        OperationType? type = null)
    {
        var query = _dbContext.Categories
            .Where(c =>
                c.UserId == null ||
                c.UserId == userId);

        if (type.HasValue)
        {
            query = query.Where(c => c.Type == type.Value);
        }

        var categories = await query
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Name)
            .ToListAsync();

        return categories.Select(Map);
    }

    public async Task<CategoryDto> CreateAsync(
        Guid userId,
        CreateCategoryRequest request)
    {
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BadRequestException(
                "Category name is required.");
        }

        if (!request.Type.HasValue)
        {
            throw new BadRequestException(
                "Category type is required.");
        }

        var type = request.Type.Value;

        // R10:
        // Category names must be unique per user and type,
        // case-insensitive.
        // A user category also cannot duplicate
        // a system category of the same type.
        var normalizedName = name.ToLower();

        var duplicateExists =
            await _dbContext.Categories.AnyAsync(c =>
                c.Type == type &&
                (c.UserId == null || c.UserId == userId) &&
                c.Name.ToLower() == normalizedName);

        if (duplicateExists)
        {
            throw new ConflictException(
                "A category with this name and type already exists.");
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            Type = type
        };

        _dbContext.Categories.Add(category);

        await _dbContext.SaveChangesAsync();

        return Map(category);
    }

    public async Task<CategoryDto> UpdateAsync(
        Guid userId,
        Guid id,
        UpdateCategoryRequest request)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
        {
            throw new NotFoundException(
                "Category was not found.");
        }

        // R11:
        // System categories cannot be renamed.
        if (category.UserId is null)
        {
            throw new ForbiddenException(
                "System categories cannot be renamed.");
        }

        // R15:
        // Category belongs to another user.
        if (category.UserId != userId)
        {
            throw new ForbiddenException(
                "You do not have access to this category.");
        }

        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BadRequestException(
                "Category name is required.");
        }

        var normalizedName = name.ToLower();

        // R10:
        // Prevent duplicate category names
        // for the same type.
        var duplicateExists =
            await _dbContext.Categories.AnyAsync(c =>
                c.Id != id &&
                c.Type == category.Type &&
                (c.UserId == null || c.UserId == userId) &&
                c.Name.ToLower() == normalizedName);

        if (duplicateExists)
        {
            throw new ConflictException(
                "A category with this name and type already exists.");
        }

        category.Name = name;

        await _dbContext.SaveChangesAsync();

        return Map(category);
    }

    public async Task DeleteAsync(
        Guid userId,
        Guid id)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
        {
            throw new NotFoundException(
                "Category was not found.");
        }

        // R11:
        // System categories cannot be deleted.
        if (category.UserId is null)
        {
            throw new ForbiddenException(
                "System categories cannot be deleted.");
        }

        // R15:
        // Category belongs to another user.
        if (category.UserId != userId)
        {
            throw new ForbiddenException(
                "You do not have access to this category.");
        }

        // R12:
        // A category referenced by an operation
        // cannot be deleted.
        var isUsed = await _dbContext.Operations
            .AnyAsync(o => o.CategoryId == id);

        if (isUsed)
        {
            throw new ConflictException(
                "Category cannot be deleted because it is used by an operation.");
        }

        _dbContext.Categories.Remove(category);

        await _dbContext.SaveChangesAsync();
    }

    private static CategoryDto Map(Category category)
    {
        return new CategoryDto(
            category.Id,
            category.Name,
            category.Type,
            category.UserId is null
        );
    }
}