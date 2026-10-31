using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace PersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OperationsController : ControllerBase
{
    private readonly IOperationService _operationService;

    public OperationsController(
        IOperationService operationService)
    {
        _operationService = operationService;
    }

    // GET /api/operations
    [HttpGet]
    public async Task<ActionResult<PagedResult<OperationDto>>> GetAll(
        [FromQuery] OperationQueryParameters parameters)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result =
            await _operationService.GetAllAsync(
                userId.Value,
                parameters);

        return Ok(result);
    }

    // GET /api/operations/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OperationDto>> GetById(
        Guid id)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var operation =
            await _operationService.GetByIdAsync(
                userId.Value,
                id);

        return Ok(operation);
    }

    // POST /api/operations
    [HttpPost]
    public async Task<ActionResult<OperationDto>> Create(
        [FromBody] CreateOperationRequest request)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var operation =
            await _operationService.CreateAsync(
                userId.Value,
                request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = operation.Id },
            operation);
    }

    // PUT /api/operations/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<OperationDto>> Update(
        Guid id,
        [FromBody] UpdateOperationRequest request)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var operation =
            await _operationService.UpdateAsync(
                userId.Value,
                id,
                request);

        return Ok(operation);
    }

    // DELETE /api/operations/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        await _operationService.DeleteAsync(
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