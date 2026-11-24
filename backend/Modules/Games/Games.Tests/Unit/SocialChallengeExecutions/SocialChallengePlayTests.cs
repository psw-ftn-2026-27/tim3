using FluentAssertions;
using Games.Domain.Locations;
using Games.Domain.SocialChallengeExecutions;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Unit.SocialChallengeExecutions;

public class SocialChallengePlayTests
{
    private const double GatheringRadius = 10;
    private const double MetresPerDegreeOfLatitude = 6_371_000 * Math.PI / 180;
    private static readonly Location ChallengeLocation = new(45.2551, 19.8452);
    // The active period is a local time of day, so the moment is set on the local clock.
    private static readonly DateTime Now = new DateTime(2026, 11, 24, 12, 30, 0, DateTimeKind.Local).ToUniversalTime();

    private readonly SocialChallengePlay _play = new();

    private static SocialChallenge CreateDraftChallenge(int peopleRequired) =>
        new("Podne na trgu", new ActivePeriod(new TimeOnly(12, 0), new TimeOnly(13, 0)), peopleRequired,
            GatheringRadius, 300, "Podne je vaše!", ChallengeLocation);

    private static SocialChallenge CreatePublishedChallenge(int peopleRequired = 2)
    {
        var challenge = CreateDraftChallenge(peopleRequired);
        challenge.Publish();
        return challenge;
    }

    // A position due north of the challenge, the given number of metres away.
    private static Location PositionNorthOfChallenge(double metres) =>
        new(ChallengeLocation.Latitude + metres / MetresPerDegreeOfLatitude, ChallengeLocation.Longitude);

    private SocialChallengeExecution ActivateAt(SocialChallenge challenge, SocialChallengeExecution? current,
        Guid explorerId, DateTime now) =>
        _play.Activate(challenge, current, explorerId, ChallengeLocation, now, now);

    [Fact]
    public void Explorer_at_the_gathering_radius_activates_the_challenge()
    {
        var challenge = CreatePublishedChallenge();

        var execution = _play.Activate(challenge, null, WellKnownUsers.Explorer,
            PositionNorthOfChallenge(GatheringRadius), Now, Now);

        execution.Participants.Should().ContainSingle(participant => participant.ExplorerId == WellKnownUsers.Explorer);
    }

    [Fact]
    public void Explorer_beyond_the_gathering_radius_cannot_activate_the_challenge()
    {
        var challenge = CreatePublishedChallenge();

        var activation = () => _play.Activate(challenge, null, WellKnownUsers.Explorer,
            PositionNorthOfChallenge(GatheringRadius + 0.1), Now, Now);

        activation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Unpublished_challenge_cannot_be_activated()
    {
        var challenge = CreateDraftChallenge(2);

        var activation = () => _play.Activate(challenge, null, WellKnownUsers.Explorer, ChallengeLocation, Now, Now);

        activation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Challenge_cannot_be_activated_outside_its_active_period()
    {
        var challenge = CreatePublishedChallenge();
        var afterThePeriod = new DateTime(2026, 11, 24, 13, 30, 0, DateTimeKind.Local).ToUniversalTime();

        var activation = () => _play.Activate(challenge, null, WellKnownUsers.Explorer, ChallengeLocation,
            afterThePeriod, afterThePeriod);

        activation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Position_exactly_as_old_as_allowed_is_enough_to_activate_the_challenge()
    {
        var challenge = CreatePublishedChallenge();

        var execution = _play.Activate(challenge, null, WellKnownUsers.Explorer, ChallengeLocation,
            Now - SocialChallengePlay.MaxPositionAge, Now);

        execution.Participants.Should().ContainSingle();
    }

    [Fact]
    public void Position_older_than_allowed_cannot_activate_the_challenge()
    {
        var challenge = CreatePublishedChallenge();

        var activation = () => _play.Activate(challenge, null, WellKnownUsers.Explorer, ChallengeLocation,
            Now - SocialChallengePlay.MaxPositionAge - TimeSpan.FromSeconds(1), Now);

        activation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Activation_joins_the_explorer_to_the_active_execution()
    {
        var challenge = CreatePublishedChallenge();
        var current = ActivateAt(challenge, null, WellKnownUsers.Explorer, Now);

        var execution = ActivateAt(challenge, current, WellKnownUsers.SecondExplorer, Now.AddSeconds(5));

        execution.Should().BeSameAs(current);
        execution.Participants.Select(participant => participant.ExplorerId)
            .Should().BeEquivalentTo([WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer]);
    }

    [Fact]
    public void Activation_without_an_execution_starts_a_new_one()
    {
        var challenge = CreatePublishedChallenge();

        var execution = ActivateAt(challenge, null, WellKnownUsers.Explorer, Now);

        execution.ChallengeId.Should().Be(challenge.Id);
        execution.Status.Should().Be(SocialChallengeExecutionStatus.Active);
    }

    [Fact]
    public void Activation_after_five_minutes_of_silence_expires_the_execution_and_starts_a_new_one()
    {
        var challenge = CreatePublishedChallenge();
        var current = ActivateAt(challenge, null, WellKnownUsers.Explorer, Now);

        var execution = ActivateAt(challenge, current, WellKnownUsers.SecondExplorer,
            Now + SocialChallengeExecution.ExpiryTime + TimeSpan.FromSeconds(1));

        execution.Should().NotBeSameAs(current);
        execution.Participants.Should().ContainSingle(participant => participant.ExplorerId == WellKnownUsers.SecondExplorer);
        current.Status.Should().Be(SocialChallengeExecutionStatus.Expired);
    }

    [Fact]
    public void Report_that_brings_the_active_count_to_the_people_required_solves_the_execution()
    {
        var challenge = CreatePublishedChallenge(peopleRequired: 2);
        var execution = ActivateAt(challenge, null, WellKnownUsers.Explorer, Now);
        ActivateAt(challenge, execution, WellKnownUsers.SecondExplorer, Now.AddSeconds(5));
        var reportedAt = Now.AddSeconds(10);

        _play.Report(challenge, execution, WellKnownUsers.SecondExplorer, ChallengeLocation, reportedAt, reportedAt, []);

        execution.Status.Should().Be(SocialChallengeExecutionStatus.Solved);
        execution.SolvedAt.Should().Be(reportedAt);
        execution.Participants.Should().OnlyContain(participant => participant.ExperienceGained != null);
    }

    [Fact]
    public void Report_below_the_people_required_does_not_solve_the_execution()
    {
        var challenge = CreatePublishedChallenge(peopleRequired: 3);
        var execution = ActivateAt(challenge, null, WellKnownUsers.Explorer, Now);
        ActivateAt(challenge, execution, WellKnownUsers.SecondExplorer, Now.AddSeconds(5));
        var reportedAt = Now.AddSeconds(10);

        _play.Report(challenge, execution, WellKnownUsers.SecondExplorer, ChallengeLocation, reportedAt, reportedAt, []);

        execution.Status.Should().Be(SocialChallengeExecutionStatus.Active);
        execution.SolvedAt.Should().BeNull();
    }
}
