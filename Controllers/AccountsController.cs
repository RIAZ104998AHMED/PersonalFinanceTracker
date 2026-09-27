using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Interfaces;

namespace PersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        var accounts =
            await _accountService.GetAllAsync(includeArchived);

        return Ok(accounts);
    }

    [HttpPost]
    public async Task<ActionResult<AccountDto>> Create(
        [FromBody] CreateAccountRequest request)
    {
        var account =
            await _accountService.CreateAsync(request);

        return Created(
            $"/api/accounts/{account.Id}",
            account);
    }
}