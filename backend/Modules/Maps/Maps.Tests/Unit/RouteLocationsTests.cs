using FluentAssertions;
using Maps.Core;
using Maps.Core.WalkingRoutes;
using Shared.Domain.Exceptions;
using Xunit;

namespace Maps.Tests.Unit;

public class RouteLocationsTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Route_cannot_have_fewer_than_two_locations(int count)
    {
        var locations = Locations(count);

        var creation = () => new RouteLocations(locations);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Route_with_two_locations_is_accepted()
    {
        var locations = Locations(2);

        var creation = () => new RouteLocations(locations);

        creation.Should().NotThrow();
    }

    private static List<LocationDto> Locations(int count)
    {
        return Enumerable.Range(0, count)
            .Select(index => new LocationDto(45.25, 19.84 + index * 0.01))
            .ToList();
    }
}
