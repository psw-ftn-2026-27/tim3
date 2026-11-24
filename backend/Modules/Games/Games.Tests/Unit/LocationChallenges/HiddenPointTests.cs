using FluentAssertions;
using Games.Domain.LocationChallenges;
using Games.Domain.Locations;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.LocationChallenges;

public class HiddenPointTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Hidden_point_requires_a_photo(string photo)
    {
        var creation = () => new HiddenPoint(photo, new Location(45.2524, 19.8619));

        creation.Should().Throw<DomainException>();
    }
}
