using Maps.Core;
using Maps.Core.WalkingRoutes;

namespace Maps.Tests.Integration;

public sealed class FakeRoutingService : IRoutingService
{
    public static readonly LocationDto Unreachable = new(45.2490, 19.8700);

    public static readonly WalkingRouteDto Route = new(
        [new LocationDto(45.2671, 19.8335), new LocationDto(45.2600, 19.8400), new LocationDto(45.2551, 19.8451)],
        2.35);

    public Task<WalkingRouteDto?> FindWalkingRouteAsync(IReadOnlyList<LocationDto> locations)
    {
        var route = locations.Contains(Unreachable) ? null : Route;
        return Task.FromResult(route);
    }
}
