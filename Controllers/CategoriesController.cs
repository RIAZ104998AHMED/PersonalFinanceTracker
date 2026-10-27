using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Interfaces;
using PersonalFinanceTracker.DataAccess.Entities;
using System.IdentityModel.Tokens.Jwt;

namespace PersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(
        ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // GET /api/categories?type=Expense
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll(
        [FromQuery] OperationType? type = null)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var categories =
            await _categoryService.GetAllAsync(
                userId.Value,
                type);

        return Ok(categories);
    }

    // POST /api/categories
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(
        [FromBody] CreateCategoryRequest request)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var category =
            await _categoryService.CreateAsync(
                userId.Value,
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
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var category =
            await _categoryService.UpdateAsync(
                userId.Value,
                id,
                request);

        return Ok(category);
    }

    // DELETE /api/categories/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        await _categoryService.DeleteAsync(
            userId.Value,
            id);

        return NoContent();
    }

    private Guid? GetUserId()
    {
        var value =
            User.FindFirst(
                JwtRegisteredClaimNames.Sub)?.Value;

        return Guid.TryParse(value, out var userId)
            ? userId
            : null;
    }
}