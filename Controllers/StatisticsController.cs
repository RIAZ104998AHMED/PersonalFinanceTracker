using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Interfaces;
using PersonalFinanceTracker.Api.Services;

namespace PersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;

    private readonly ICurrentUser _currentUser;

    public StatisticsController(
        IStatisticsService statisticsService,
        ICurrentUser currentUser)
    {
        _statisticsService = statisticsService;
        _currentUser = currentUser;
    }

    // GET /api/statistics
    [HttpGet]
    public async Task<ActionResult<StatisticsDto>> Get(
        [FromQuery] StatisticsQueryParameters parameters)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        var result =
            await _statisticsService.GetAsync(
                userId,
                parameters);

        return Ok(result);
    }
}