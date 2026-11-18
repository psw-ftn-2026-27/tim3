using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class GeoLocationTests
{
    [Theory]
    [InlineData(91, 0)]
    [InlineData(-91, 0)]
    [InlineData(0, 181)]
    [InlineData(0, -181)]
    public void Location_outside_the_latitude_or_longitude_range_is_rejected(double latitude, double longitude)
    {
        var creation = () => new GeoLocation(latitude, longitude);

        creation.Should().Throw<DomainException>();
    }
}
