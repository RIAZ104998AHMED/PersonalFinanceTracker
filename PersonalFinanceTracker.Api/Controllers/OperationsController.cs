using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Api.Services;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Interfaces;

namespace PersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OperationsController : ControllerBase
{
    private readonly IOperationService _operationService;
    private readonly ICurrentUser _currentUser;
    public OperationsController(
        IOperationService operationService,
        ICurrentUser currentUser)
    {
        _operationService = operationService;
        _currentUser = currentUser;
    }

    // GET /api/operations
    [HttpGet]
    public async Task<ActionResult<PagedResult<OperationDto>>> GetAll(
        [FromQuery] OperationQueryParameters parameters)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        var result =
            await _operationService.GetAllAsync(
                userId,
                parameters);

        return Ok(result);
    }

    // GET /api/operations/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OperationDto>> GetById(
        Guid id)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        var operation =
            await _operationService.GetByIdAsync(
                userId,
                id);

        return Ok(operation);
    }

    // POST /api/operations
    [HttpPost]
    public async Task<ActionResult<OperationDto>> Create(
        [FromBody] CreateOperationRequest request)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        var operation =
            await _operationService.CreateAsync(
                userId,
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
        if (_currentUser.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        var operation =
            await _operationService.UpdateAsync(
                userId,
                id,
                request);

        return Ok(operation);
    }

    // DELETE /api/operations/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        await _operationService.DeleteAsync(
            userId,
            id);

        return NoContent();
    }
}