using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace PersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransfersController : ControllerBase
{
    private readonly ITransferService _transferService;

    public TransfersController(
        ITransferService transferService)
    {
        _transferService = transferService;
    }

    // GET /api/transfers
    [HttpGet]
    public async Task<ActionResult<PagedResult<TransferDto>>> GetAll(
        [FromQuery] TransferQueryParameters parameters)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result =
            await _transferService.GetAllAsync(
                userId.Value,
                parameters);

        return Ok(result);
    }

    // GET /api/transfers/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TransferDto>> GetById(
        Guid id)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var transfer =
            await _transferService.GetByIdAsync(
                userId.Value,
                id);

        return Ok(transfer);
    }

    // POST /api/transfers
    [HttpPost]
    public async Task<ActionResult<TransferDto>> Create(
        [FromBody] CreateTransferRequest request)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var transfer =
            await _transferService.CreateAsync(
                userId.Value,
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
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        await _transferService.DeleteAsync(
            userId.Value,
            id);

        return NoContent();
    }

    private Guid? GetUserId()
    {
        var value =
            User.FindFirst(
                JwtRegisteredClaimNames.Sub)?.Value;

        return Guid.TryParse(
            value,
            out var userId)
            ? userId
            : null;
    }
}