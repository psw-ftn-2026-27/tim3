using Maps.Core.WalkingRoutes;
using Shared.Domain.Exceptions;

namespace Maps.Core;

public sealed record RouteLocations
{
    public const int MinCount = 2;

    public IReadOnlyList<LocationDto> Locations { get; }

    public RouteLocations(IReadOnlyList<LocationDto> locations)
    {
        if (locations.Count < MinCount)
        {
            throw new DomainException("A route needs at least two locations.");
        }

        Locations = locations;
    }
}
