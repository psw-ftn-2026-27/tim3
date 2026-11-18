using Games.Application.LocationChallengeAuthoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain;

namespace Games.Api;

[ApiController]
[Route("api/games/location-challenges")]
[Authorize(Roles = "administrator")]
public sealed class LocationChallengeAuthoringController : ControllerBase
{
    private readonly LocationChallengeAuthoringService _authoringService;
    private readonly LocationChallengeAuthoringQueries _authoringQueries;

    public LocationChallengeAuthoringController(
        LocationChallengeAuthoringService authoringService,
        LocationChallengeAuthoringQueries authoringQueries)
    {
        _authoringService = authoringService;
        _authoringQueries = authoringQueries;
    }

    [HttpGet]
    public async Task<ActionResult<PageResult<LocationChallengeSummaryDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _authoringQueries.GetAllAsync(page, pageSize);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LocationChallengeDto>> GetById(Guid id)
    {
        return await _authoringQueries.GetByIdAsync(id);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateLocationChallengeDto dto)
    {
        return await _authoringService.CreateAsync(dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, UpdateLocationChallengeDto dto)
    {
        await _authoringService.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _authoringService.DeleteAsync(id);
        return NoContent();
    }
}
