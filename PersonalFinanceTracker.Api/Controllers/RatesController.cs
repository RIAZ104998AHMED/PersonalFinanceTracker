using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Interfaces;

namespace PersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/rates")]
[Authorize]
public sealed class RatesController : ControllerBase
{
    private readonly IExchangeRateService _exchangeRateService;

    public RatesController(
        IExchangeRateService exchangeRateService)
    {
        _exchangeRateService = exchangeRateService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(ExchangeRatesDto),
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
    public async Task<ActionResult<ExchangeRatesDto>> GetRates(
        [FromQuery] DateOnly? date,
        CancellationToken cancellationToken)
    {
        var result =
            await _exchangeRateService.GetRatesAsync(
                date,
                cancellationToken);

        return Ok(result);
    }
}