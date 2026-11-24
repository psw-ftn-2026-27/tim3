using FluentAssertions;
using Games.Domain.LocationChallenges;
using Games.Domain.Locations;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.LocationChallenges;

public class SearchCircleTests
{
    [Fact]
    public void Search_circle_with_a_negative_radius_cannot_exist()
    {
        var creation = () => new SearchCircle(new Location(45.2517, 19.8634), -0.5);

        creation.Should().Throw<DomainException>();
    }
}
