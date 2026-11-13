using FluentAssertions;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.SocialChallenges;

public class SocialChallengeTests
{
    private static readonly ActivePeriod Evening = new(new TimeOnly(18, 0), new TimeOnly(20, 0));

    private static SocialChallenge CreateChallenge(string name = "Zalazak na tvrđavi", int peopleRequired = 5,
        double gatheringRadius = 10, int experienceGained = 300, string endMessage = "Bravo!") =>
        new(name, Evening, peopleRequired, gatheringRadius, experienceGained, endMessage);

    private static SocialChallenge CreateChallengeIn(SocialChallengeStatus status)
    {
        var challenge = CreateChallenge();
        if (status == SocialChallengeStatus.Draft)
        {
            return challenge;
        }

        challenge.Publish();
        if (status == SocialChallengeStatus.Retired)
        {
            challenge.Retire();
        }
        if (status == SocialChallengeStatus.Archived)
        {
            challenge.Archive();
        }
        return challenge;
    }

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

        var update = () => challenge.Update(name, Evening, peopleRequired, gatheringRadius, experienceGained, endMessage);

        update.Should().Throw<DomainException>();
    }

    [Fact]
    public void Draft_challenge_is_published()
    {
        var challenge = CreateChallengeIn(SocialChallengeStatus.Draft);

        challenge.Publish();

        challenge.Status.Should().Be(SocialChallengeStatus.Published);
    }

    [Theory]
    [InlineData(SocialChallengeStatus.Published)]
    [InlineData(SocialChallengeStatus.Retired)]
    [InlineData(SocialChallengeStatus.Archived)]
    public void Challenge_that_is_not_a_draft_cannot_be_published(SocialChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        var publishing = () => challenge.Publish();

        publishing.Should().Throw<DomainException>();
        challenge.Status.Should().Be(status);
    }

    [Fact]
    public void Published_challenge_is_retired()
    {
        var challenge = CreateChallengeIn(SocialChallengeStatus.Published);

        challenge.Retire();

        challenge.Status.Should().Be(SocialChallengeStatus.Retired);
    }

    [Theory]
    [InlineData(SocialChallengeStatus.Draft)]
    [InlineData(SocialChallengeStatus.Retired)]
    [InlineData(SocialChallengeStatus.Archived)]
    public void Challenge_that_is_not_published_cannot_be_retired(SocialChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        var retiring = () => challenge.Retire();

        retiring.Should().Throw<DomainException>();
        challenge.Status.Should().Be(status);
    }

    [Fact]
    public void Retired_challenge_is_published_again()
    {
        var challenge = CreateChallengeIn(SocialChallengeStatus.Retired);

        challenge.Reactivate();

        challenge.Status.Should().Be(SocialChallengeStatus.Published);
    }

    [Theory]
    [InlineData(SocialChallengeStatus.Draft)]
    [InlineData(SocialChallengeStatus.Published)]
    [InlineData(SocialChallengeStatus.Archived)]
    public void Challenge_that_is_not_retired_cannot_be_published_again(SocialChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        var reactivation = () => challenge.Reactivate();

        reactivation.Should().Throw<DomainException>();
        challenge.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(SocialChallengeStatus.Published)]
    [InlineData(SocialChallengeStatus.Retired)]
    public void Published_or_retired_challenge_is_archived(SocialChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        challenge.Archive();

        challenge.Status.Should().Be(SocialChallengeStatus.Archived);
    }

    [Theory]
    [InlineData(SocialChallengeStatus.Draft)]
    [InlineData(SocialChallengeStatus.Archived)]
    public void Draft_or_already_archived_challenge_cannot_be_archived(SocialChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        var archiving = () => challenge.Archive();

        archiving.Should().Throw<DomainException>();
        challenge.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(SocialChallengeStatus.Draft)]
    [InlineData(SocialChallengeStatus.Archived)]
    public void Draft_or_archived_challenge_can_be_deleted(SocialChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        var deletionCheck = () => challenge.EnsureDeletable();

        deletionCheck.Should().NotThrow();
    }

    [Theory]
    [InlineData(SocialChallengeStatus.Published)]
    [InlineData(SocialChallengeStatus.Retired)]
    public void Published_or_retired_challenge_cannot_be_deleted(SocialChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        var deletionCheck = () => challenge.EnsureDeletable();

        deletionCheck.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_challenge_cannot_be_changed()
    {
        var challenge = CreateChallengeIn(SocialChallengeStatus.Published);

        var update = () => challenge.Update("Zalazak na bedemu", Evening, 8, 12, 500, "Zalazak je vaš!");

        update.Should().Throw<DomainException>();
        challenge.Name.Should().Be("Zalazak na tvrđavi");
    }
}
