using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Clubs;
using Xunit;

namespace Social.Tests.Unit.Clubs;

public class ClubTests
{
    private static Club CreateClub() =>
        new(WellKnownUsers.Explorer, "Planinari", "Klub ljubitelja planinarskih tura.", ["/api/media/images/planinari.jpg"]);

    private static List<string> CreateImages(int count) =>
        Enumerable.Range(1, count).Select(number => $"/api/media/images/slika-{number}.jpg").ToList();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_club_requires_a_name(string name)
    {
        var creation = () => new Club(WellKnownUsers.Explorer, name, "Klub ljubitelja planinarskih tura.", CreateImages(1));

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_club_requires_a_description(string description)
    {
        var creation = () => new Club(WellKnownUsers.Explorer, "Planinari", description, CreateImages(1));

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void New_club_requires_one_to_five_images(int imageCount)
    {
        var creation = () => new Club(WellKnownUsers.Explorer, "Planinari", "Klub ljubitelja planinarskih tura.", CreateImages(imageCount));

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("", "Novi opis kluba.", 1)]
    [InlineData("Novo ime", "   ", 1)]
    [InlineData("Novo ime", "Novi opis kluba.", 0)]
    [InlineData("Novo ime", "Novi opis kluba.", 6)]
    public void Updated_club_must_remain_valid(string name, string description, int imageCount)
    {
        var club = CreateClub();

        var update = () => club.Update(WellKnownUsers.Explorer, name, description, CreateImages(imageCount));

        update.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_who_is_not_the_owner_cannot_update_the_club()
    {
        var club = CreateClub();

        var update = () => club.Update(WellKnownUsers.SecondExplorer, "Novo ime", "Novi opis kluba.", CreateImages(1));

        update.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void User_who_is_not_the_owner_cannot_delete_the_club()
    {
        var club = CreateClub();

        var deletion = () => club.EnsureCanBeDeletedBy(WellKnownUsers.SecondExplorer);

        deletion.Should().Throw<NotFoundException>();
    }
}
