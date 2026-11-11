using Maps.Core.WalkingRoutes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maps.Api;

[ApiController]
[Route("api/maps/walking-routes")]
public sealed class WalkingRoutesController : ControllerBase
{
    private readonly WalkingRoutesQueries _walkingRoutesQueries;

    public WalkingRoutesController(WalkingRoutesQueries walkingRoutesQueries)
    {
        _walkingRoutesQueries = walkingRoutesQueries;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<WalkingRouteDto>> Get([FromQuery] List<LocationDto> locations)
    {
        return await _walkingRoutesQueries.GetAsync(locations);
    }
}
