using FluentAssertions;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.SocialChallenges;

public class SocialChallengeTests
{
    private static readonly TimeOnly Evening = new(18, 0);
    private static readonly TimeOnly LateEvening = new(20, 0);

    private static SocialChallenge CreateChallenge(string name = "Zalazak na tvrđavi", int peopleRequired = 5,
        double gatheringRadius = 10, int experienceGained = 300, string endMessage = "Bravo!") =>
        new(name, Evening, LateEvening, peopleRequired, gatheringRadius, experienceGained, endMessage);

    [Theory]
    [InlineData("", "Bravo!")]
    [InlineData("   ", "Bravo!")]
    [InlineData("Zalazak na tvrđavi", "")]
    [InlineData("Zalazak na tvrđavi", "   ")]
    public void Challenge_requires_a_name_and_an_end_message(string name, string endMessage)
    {
        var creation = () => CreateChallenge(name: name, endMessage: endMessage);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    public void Challenge_requires_at_least_two_people(int peopleRequired)
    {
        var creation = () => CreateChallenge(peopleRequired: peopleRequired);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(50.1)]
    public void Gathering_radius_is_greater_than_zero_and_at_most_fifty_meters(double gatheringRadius)
    {
        var creation = () => CreateChallenge(peopleRequired: 2, gatheringRadius: gatheringRadius);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(71, 15)]
    [InlineData(2, 2.5)]
    public void Challenge_for_more_people_than_fit_in_the_gathering_circle_is_rejected(int peopleRequired, double gatheringRadius)
    {
        var creation = () => CreateChallenge(peopleRequired: peopleRequired, gatheringRadius: gatheringRadius);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(70, 15)]
    [InlineData(2, 2.6)]
    public void Challenge_for_as_many_people_as_fit_in_the_gathering_circle_is_accepted(int peopleRequired, double gatheringRadius)
    {
        var challenge = CreateChallenge(peopleRequired: peopleRequired, gatheringRadius: gatheringRadius);

        challenge.PeopleRequired.Should().Be(peopleRequired);
        challenge.GatheringRadius.Should().Be(gatheringRadius);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(20001)]
    public void Experience_is_between_zero_and_twenty_thousand(int experienceGained)
    {
        var creation = () => CreateChallenge(experienceGained: experienceGained);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Active_period_cannot_start_and_end_at_the_same_time()
    {
        var creation = () => new SocialChallenge("Zalazak na tvrđavi", Evening, Evening, 5, 10, 300, "Bravo!");

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Active_period_may_span_midnight()
    {
        var challenge = new SocialChallenge("Ponoć na mostu", new TimeOnly(22, 0), new TimeOnly(2, 0), 5, 10, 300, "Bravo!");

        challenge.ActivePeriod.Start.Should().Be(new TimeOnly(22, 0));
        challenge.ActivePeriod.End.Should().Be(new TimeOnly(2, 0));
    }

    [Theory]
    [InlineData("", 5, 10, 300, "Bravo!")]
    [InlineData("Zalazak na tvrđavi", 5, 10, 300, "")]
    [InlineData("Zalazak na tvrđavi", 1, 10, 300, "Bravo!")]
    [InlineData("Zalazak na tvrđavi", 5, 51, 300, "Bravo!")]
    [InlineData("Zalazak na tvrđavi", 71, 15, 300, "Bravo!")]
    [InlineData("Zalazak na tvrđavi", 5, 10, 20001, "Bravo!")]
    public void Update_with_invalid_values_is_rejected(string name, int peopleRequired, double gatheringRadius,
        int experienceGained, string endMessage)
    {
        var challenge = CreateChallenge();

        var update = () => challenge.Update(name, Evening, LateEvening, peopleRequired, gatheringRadius, experienceGained, endMessage);

        update.Should().Throw<DomainException>();
    }
}
