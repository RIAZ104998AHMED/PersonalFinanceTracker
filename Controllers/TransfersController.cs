using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Interfaces;
using PersonalFinanceTracker.Api.Services;

namespace PersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransfersController : ControllerBase
{
    private readonly ITransferService _transferService;

    private readonly ICurrentUser _currentUser;

    public TransfersController(
        ITransferService transferService,
        ICurrentUser currentUser)
    {
        _transferService = transferService;
        _currentUser = currentUser;
    }

    // GET /api/transfers
    [HttpGet]
    public async Task<ActionResult<PagedResult<TransferDto>>> GetAll(
        [FromQuery] TransferQueryParameters parameters)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        var result =
            await _transferService.GetAllAsync(
                userId,
                parameters);

        return Ok(result);
    }

    // GET /api/transfers/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TransferDto>> GetById(
        Guid id)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        var transfer =
            await _transferService.GetByIdAsync(
                userId,
                id);

        return Ok(transfer);
    }

    // POST /api/transfers
    [HttpPost]
    public async Task<ActionResult<TransferDto>> Create(
        [FromBody] CreateTransferRequest request)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        var transfer =
            await _transferService.CreateAsync(
                userId,
                request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = transfer.Id },
            transfer);
    }

    // DELETE /api/transfers/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        await _transferService.DeleteAsync(
            userId,
            id);

        return NoContent();
    }

}