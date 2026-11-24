using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Clubs;
using Xunit;

namespace Social.Tests.Unit.Clubs;

public class ClubNameUniquenessTests
{
    private static Club CreateClub(string name) =>
        new(WellKnownUsers.Explorer, name, "Klub ljubitelja planinarskih tura.", ["/api/media/images/planinari.jpg"]);

    [Theory]
    [InlineData("Planinari")]
    [InlineData("PLANINARI")]
    public void Name_already_taken_by_another_club_is_rejected(string otherClubName)
    {
        var club = CreateClub("Planinari");
        var otherClub = CreateClub(otherClubName);

        var check = () => ClubNameUniqueness.EnsureUnique(club, otherClub);

        check.Should().Throw<DomainException>();
    }

    [Fact]
    public void Club_keeping_its_own_name_is_not_rejected()
    {
        var club = CreateClub("Planinari");

        var check = () => ClubNameUniqueness.EnsureUnique(club, club);

        check.Should().NotThrow();
    }
}
