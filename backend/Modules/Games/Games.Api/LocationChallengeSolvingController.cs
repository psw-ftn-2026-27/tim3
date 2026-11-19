using Games.Application.LocationChallengeSolving;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Domain;

namespace Games.Api;

[ApiController]
[Route("api/games/location-challenges/solving")]
[Authorize(Roles = "explorer")]
public sealed class LocationChallengeSolvingController : ControllerBase
{
    private readonly LocationChallengeSolvingService _solvingService;
    private readonly LocationChallengeSolvingQueries _solvingQueries;

    public LocationChallengeSolvingController(
        LocationChallengeSolvingService solvingService,
        LocationChallengeSolvingQueries solvingQueries)
    {
        _solvingService = solvingService;
        _solvingQueries = solvingQueries;
    }

    [HttpGet]
    public async Task<ActionResult<PageResult<LocationChallengeToSolveDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _solvingQueries.GetAllAsync(User.GetUserId(), page, pageSize);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LocationChallengeHuntDto>> GetById(Guid id)
    {
        return await _solvingQueries.GetByIdAsync(User.GetUserId(), id);
    }

    [HttpPost("{id:guid}/activation")]
    public async Task<ActionResult<Guid>> Activate(Guid id)
    {
        return await _solvingService.ActivateAsync(User.GetUserId(), id);
    }

    [HttpPost("{id:guid}/location-check")]
    public async Task<ActionResult> CheckLocation(Guid id)
    {
        await _solvingService.CheckLocationAsync(User.GetUserId(), id);
        return NoContent();
    }
}
