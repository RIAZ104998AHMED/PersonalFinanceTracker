using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Interfaces;
using PersonalFinanceTracker.DataAccess.Entities;
using PersonalFinanceTracker.Api.Services;

namespace PersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ICurrentUser _currentUser;

    public CategoriesController(
        ICategoryService categoryService,
        ICurrentUser currentUser)
    {
        _categoryService = categoryService;
        _currentUser = currentUser;
    }

    // GET /api/categories?type=Expense
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll(
        [FromQuery] OperationType? type = null)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        var categories =
            await _categoryService.GetAllAsync(
                userId,
                type);

        return Ok(categories);
    }

    // POST /api/categories
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(
        [FromBody] CreateCategoryRequest request)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        var category =
            await _categoryService.CreateAsync(
                userId,
                request);

        return Created(
            $"/api/categories/{category.Id}",
            category);
    }

    // PUT /api/categories/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> Update(
        Guid id,
        [FromBody] UpdateCategoryRequest request)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        var category =
            await _categoryService.UpdateAsync(
                userId,
                id,
                request);

        return Ok(category);
    }

    // DELETE /api/categories/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        await _categoryService.DeleteAsync(
            userId,
            id);

        return NoContent();
    }

}