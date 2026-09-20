using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Api.Models;

namespace PersonalFinanceTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private static readonly List<AccountDto> Accounts =
        [
            new(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "Cash",
                500.00m
            ),

            new(
                Guid.Parse("22222222-2222-2222-2222-222222222222"),
                "Bank Account",
                2500.00m
            )
        ];

        // GET /api/accounts
        [HttpGet]
        public ActionResult<IEnumerable<AccountDto>> GetAll()
        {
            return Ok(Accounts);
        }

        // GET /api/accounts/{id}
        [HttpGet("{id:guid}")]
        public ActionResult<AccountDto> GetById(Guid id)
        {
            var account = Accounts.FirstOrDefault(a => a.Id == id);

            if (account == null)
            {
                return NotFound();
            }

            return Ok(account);
        }

        // POST /api/accounts
        [HttpPost]
        public ActionResult<AccountDto> Create(
            [FromBody] CreateAccountRequest request)
        {
            var account = new AccountDto(
                Guid.NewGuid(),
                request.Name,
                request.Balance
            );

            Accounts.Add(account);

            return CreatedAtAction(
                nameof(GetById),
                new { id = account.Id },
                account
            );
        }
    }
}