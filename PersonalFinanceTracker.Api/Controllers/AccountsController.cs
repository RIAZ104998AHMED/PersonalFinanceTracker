using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Interfaces;
using PersonalFinanceTracker.Api.Services;

namespace PersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    private readonly ICurrentUser _currentUser;

    public AccountsController(
     IAccountService accountService,
     ICurrentUser currentUser)
    {
        _accountService = accountService;
        _currentUser = currentUser;
    }

    // GET /api/accounts?includeArchived=false
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetAll(
        [FromQuery] bool includeArchived = false)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        var accounts =
            await _accountService.GetAllAsync(
                userId,
                includeArchived);

        return Ok(accounts);
    }

    // GET /api/accounts/{id}
    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(AccountDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountDto>> GetById(Guid id)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        var account =
            await _accountService.GetByIdAsync(
                userId,
                id);

        return Ok(account);
    }

    // POST /api/accounts
    [HttpPost]
    [ProducesResponseType(
        typeof(AccountDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccountDto>> Create(
        [FromBody] CreateAccountRequest request)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        var account =
            await _accountService.CreateAsync(
                userId,
                request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = account.Id },
            account);
    }
    // PUT /api/accounts/{id}
    [HttpPut("{id:guid}")]
    [ProducesResponseType(
     typeof(AccountDto),
     StatusCodes.Status200OK)]
    [ProducesResponseType(
     typeof(ValidationProblemDetails),
     StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
     typeof(ProblemDetails),
     StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
     typeof(ProblemDetails),
     StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
     typeof(ProblemDetails),
     StatusCodes.Status404NotFound)]
    [ProducesResponseType(
     typeof(ProblemDetails),
     StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AccountDto>> Update(
        Guid id,
        [FromBody] UpdateAccountRequest request)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        var account =
            await _accountService.UpdateAsync(
                userId,
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
        if (_currentUser.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        var account =
            await _accountService.SetArchivedAsync(
                userId,
                id,
                request.IsArchived!.Value);

        if (account is null)
        {
            return NotFound();
        }

        return Ok(account);
    }

    // DELETE /api/accounts/{id}
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]                                           
    public async Task<IActionResult> Delete(Guid id)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        var deleted =
            await _accountService.DeleteAsync(
                userId,
                id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    // GET /api/accounts/summary?currency=USD&date=2026-09-30
    [HttpGet("summary")]
    [ProducesResponseType(
        typeof(AccountSummaryDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<AccountSummaryDto>> GetSummary(
        [FromQuery] string currency,
        [FromQuery] DateOnly? date,
        [FromServices] IAccountSummaryService accountSummaryService,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        var result =
            await accountSummaryService.GetSummaryAsync(
                userId,
                currency,
                date,
                cancellationToken);

        return Ok(result);
    }
}