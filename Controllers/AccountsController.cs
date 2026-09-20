using Microsoft.AspNetCore.Mvc;

namespace PersonalFinanceTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAccounts()
        {
            var accounts = new[]
            {
                new
                {
                    Id = 1,
                    Name = "Cash"
                },
                new
                {
                    Id = 2,
                    Name = "Bank Account"
                },
                new
                {
                    Id = 3,
                    Name = "Savings Account"
                }
            };

            return Ok(accounts);
        }
    }
}