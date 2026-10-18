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