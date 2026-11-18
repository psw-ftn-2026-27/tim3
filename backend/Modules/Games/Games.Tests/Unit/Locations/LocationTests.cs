using FluentAssertions;
using Games.Domain.Locations;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.Locations;

public class LocationTests
{
    [Theory]
    [InlineData(-90.000001, 0)]
    [InlineData(90.000001, 0)]
    [InlineData(0, -180.000001)]
    [InlineData(0, 180.000001)]
    public void Location_outside_the_latitude_or_longitude_range_cannot_be_created(double latitude, double longitude)
    {
        var creation = () => new Location(latitude, longitude);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(-90, -180)]
    [InlineData(90, 180)]
    public void Location_on_the_latitude_and_longitude_boundaries_is_created(double latitude, double longitude)
    {
        var location = new Location(latitude, longitude);

        location.Latitude.Should().Be(latitude);
        location.Longitude.Should().Be(longitude);
    }

    [Fact]
    public void One_degree_of_latitude_along_a_meridian_is_about_111_kilometres()
    {
        var equator = new Location(0, 0);

        var distance = equator.DistanceTo(new Location(1, 0));

        distance.Should().BeApproximately(111_195, 1);
    }
}
