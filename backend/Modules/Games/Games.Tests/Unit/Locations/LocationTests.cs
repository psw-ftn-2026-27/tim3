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

    [Theory]
    [InlineData(45.2517, null)]
    [InlineData(null, 19.8616)]
    public void Location_with_only_one_of_its_coordinates_cannot_be_created(double? latitude, double? longitude)
    {
        var creation = () => Location.FromOptionalCoordinates(latitude, longitude);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(45.0, 19.0, 46.0, 19.0, 111195)]
    [InlineData(0.0, 19.0, 0.0, 20.0, 111195)]
    [InlineData(45.2517, 19.8634, 45.2522, 19.8634, 56)]
    public void Distance_between_two_known_locations_is_accurate_to_a_few_metres(
        double fromLatitude, double fromLongitude, double toLatitude, double toLongitude, double expectedMetres)
    {
        var from = new Location(fromLatitude, fromLongitude);
        var to = new Location(toLatitude, toLongitude);

        var distance = from.DistanceTo(to);

        distance.Should().BeApproximately(expectedMetres, 3);
    }
}
