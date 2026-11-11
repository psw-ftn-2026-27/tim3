using Shared.Domain.Exceptions;

namespace Maps.Core.WalkingRoutes;

public sealed class WalkingRoutesQueries
{
    private readonly IRoutingService _routing;

    public WalkingRoutesQueries(IRoutingService routing)
    {
        _routing = routing;
    }

    public async Task<WalkingRouteDto> GetAsync(IReadOnlyList<LocationDto> locations)
    {
        return await _routing.FindWalkingRouteAsync(locations)
            ?? throw new NotFoundException("No walking route connects the given locations.");
    }
}
