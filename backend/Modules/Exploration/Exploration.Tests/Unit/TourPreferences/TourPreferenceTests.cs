using Exploration.Domain.TourPreferences;
using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.TourPreferences;

public class TourPreferenceTests
{
    private static TourPreference CreatePreference() =>
        new(WellKnownUsers.Explorer, [TourDifficulty.Easy], TransportMode.Walking, ["istorija"], true);

    [Fact]
    public void Preference_needs_only_one_of_difficulty_transport_and_tags()
    {
        var preference = new TourPreference(WellKnownUsers.Explorer, [], null, ["istorija"], false);

        preference.Difficulties.Should().BeEmpty();
        preference.Transport.Should().BeNull();
        preference.Tags.Should().Equal("istorija");
    }

    [Fact]
    public void New_preference_requires_a_difficulty_a_transport_or_a_tag()
    {
        var creation = () => new TourPreference(WellKnownUsers.Explorer, [], null, [], true);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Changed_preference_requires_a_difficulty_a_transport_or_a_tag()
    {
        var preference = CreatePreference();

        var change = () => preference.Change([], null, [], true);

        change.Should().Throw<DomainException>();
    }

    [Fact]
    public void Preference_cannot_repeat_a_difficulty()
    {
        var creation = () => new TourPreference(WellKnownUsers.Explorer, [TourDifficulty.Easy, TourDifficulty.Easy], null, [], false);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("istorija", "istorija")]
    [InlineData("istorija", "Istorija")]
    public void Preference_cannot_repeat_a_tag(string first, string second)
    {
        var creation = () => new TourPreference(WellKnownUsers.Explorer, [], null, [first, second], false);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Preference_cannot_hold_an_empty_tag(string tag)
    {
        var creation = () => new TourPreference(WellKnownUsers.Explorer, [], null, [tag], false);

        creation.Should().Throw<DomainException>();
    }
}
