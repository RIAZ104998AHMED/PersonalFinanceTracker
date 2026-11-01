using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace PersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;

    public StatisticsController(
        IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    // GET /api/statistics
    [HttpGet]
    public async Task<ActionResult<StatisticsDto>> Get(
        [FromQuery] StatisticsQueryParameters parameters)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result =
            await _statisticsService.GetAsync(
                userId.Value,
                parameters);

        return Ok(result);
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