using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace PersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    // GET /api/accounts?includeArchived=false
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetAll(
        [FromQuery] bool includeArchived = false)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var accounts =
            await _accountService.GetAllAsync(
                userId.Value,
                includeArchived);

        return Ok(accounts);
    }

    // GET /api/accounts/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AccountDto>> GetById(Guid id)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var account =
            await _accountService.GetByIdAsync(
                userId.Value,
                id);

        if (account is null)
        {
            return NotFound();
        }

        return Ok(account);
    }

    // POST /api/accounts
    [HttpPost]
    public async Task<ActionResult<AccountDto>> Create(
        [FromBody] CreateAccountRequest request)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var account =
            await _accountService.CreateAsync(
                userId.Value,
                request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = account.Id },
            account);
    }

    // PUT /api/accounts/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AccountDto>> Update(
        Guid id,
        [FromBody] UpdateAccountRequest request)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var account =
            await _accountService.UpdateAsync(
                userId.Value,
                id,
                request);

        if (account is null)
        {
            return NotFound();
        }

        return Ok(account);
    }

    // PATCH /api/accounts/{id}/archive
    [HttpPatch("{id:guid}/archive")]
    public async Task<ActionResult<AccountDto>> Archive(
        Guid id,
        [FromBody] ArchiveAccountRequest request)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var account =
            await _accountService.SetArchivedAsync(
                userId.Value,
                id,
                request.IsArchived);

        if (account is null)
        {
            return NotFound();
        }

        return Ok(account);
    }

    // DELETE /api/accounts/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var deleted =
            await _accountService.DeleteAsync(
                userId.Value,
                id);

        if (!deleted)
        {
            return NotFound();
        }

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