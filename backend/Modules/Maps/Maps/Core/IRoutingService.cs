using Maps.Core.WalkingRoutes;

namespace Maps.Core;

public interface IRoutingService
{
    Task<WalkingRouteDto?> FindWalkingRouteAsync(IReadOnlyList<LocationDto> locations);
}
