using FluentAssertions;
using Maps.Core.UserPositions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Maps.Tests.Unit;

public class CoordinatesTests
{
    [Theory]
    [InlineData(-90.0001, 19.8451)]
    [InlineData(90.0001, 19.8451)]
    [InlineData(45.2551, -180.0001)]
    [InlineData(45.2551, 180.0001)]
    public void Coordinates_outside_the_allowed_latitude_or_longitude_range_are_rejected(double latitude, double longitude)
    {
        var creation = () => new Coordinates(latitude, longitude);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(-90, -180)]
    [InlineData(90, 180)]
    public void Coordinates_on_the_edge_of_the_allowed_range_are_accepted(double latitude, double longitude)
    {
        var creation = () => new Coordinates(latitude, longitude);

        creation.Should().NotThrow();
    }
}
