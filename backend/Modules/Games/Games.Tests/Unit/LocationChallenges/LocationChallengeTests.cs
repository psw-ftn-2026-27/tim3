using FluentAssertions;
using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.LocationChallenges;

public class LocationChallengeTests
{
    private static readonly Location FortressGate = new(45.2517, 19.8634);

    private static List<HiddenPoint> CreateHiddenPoints() =>
        [new HiddenPoint("/api/media/images/sat.jpg", new Location(45.2524, 19.8619))];

    private static LocationChallenge CreateChallenge() =>
        new("Tajne tvrđave", FortressGate, 300, CreateHiddenPoints());

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Challenge_requires_a_name(string name)
    {
        var creation = () => new LocationChallenge(name, FortressGate, 300, CreateHiddenPoints());

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Challenge_requires_a_start_location()
    {
        var creation = () => new LocationChallenge("Tajne tvrđave", null, 300, CreateHiddenPoints());

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Challenge_requires_at_least_one_hidden_point()
    {
        var creation = () => new LocationChallenge("Tajne tvrđave", FortressGate, 300, []);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7001)]
    public void Experience_is_between_one_and_seven_thousand(int experienceGained)
    {
        var creation = () => new LocationChallenge("Tajne tvrđave", FortressGate, experienceGained, CreateHiddenPoints());

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Update_that_removes_all_hidden_points_is_rejected()
    {
        var challenge = CreateChallenge();

        var update = () => challenge.Update("Tajne tvrđave", FortressGate, 300, []);

        update.Should().Throw<DomainException>();
        challenge.HiddenPoints.Should().HaveCount(1);
    }
}
