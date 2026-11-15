using Maps.Core.UserPositioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Maps.Api;

[ApiController]
[Route("api/maps/user-positions")]
[Authorize]
public sealed class UserPositioningController : ControllerBase
{
    private readonly UserPositioningService _positioningService;
    private readonly UserPositioningQueries _positioningQueries;

    public UserPositioningController(
        UserPositioningService positioningService,
        UserPositioningQueries positioningQueries)
    {
        _positioningService = positioningService;
        _positioningQueries = positioningQueries;
    }

    [HttpPut("me")]
    public async Task<ActionResult> SetMyPosition(SetUserPositionDto dto)
    {
        await _positioningService.SetAsync(User.GetUserId(), dto);
        return NoContent();
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserPositionDto>> GetMyPosition()
    {
        return await _positioningQueries.GetMyPositionAsync(User.GetUserId());
    }
}
