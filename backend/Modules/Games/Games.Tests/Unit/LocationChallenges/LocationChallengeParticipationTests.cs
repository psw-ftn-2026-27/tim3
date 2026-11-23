using FluentAssertions;
using Games.Domain.LocationChallenges;
using Games.Domain.Locations;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.LocationChallenges;

public class LocationChallengeParticipationTests
{
    private static readonly DateTime Now = new(2026, 11, 18, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Already_discovered_point_cannot_be_discovered_again()
    {
        var hiddenPointId = Guid.NewGuid();
        var participation = new LocationChallengeParticipation(Guid.NewGuid(), Guid.NewGuid(),
            new SearchCircle(new Location(45.2517, 19.8634), 150));
        participation.Discover(hiddenPointId, 2, 300, Now);

        var discovery = () => participation.Discover(hiddenPointId, 2, 300, Now);

        discovery.Should().Throw<DomainException>();
        participation.DiscoveredPointIds.Should().Equal(hiddenPointId);
    }
}
