using FluentAssertions;
using Games.Domain.LocationChallenges;
using Games.Domain.Locations;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.LocationChallenges;

public class LocationChallengeHuntTests
{
    private const double MetresPerDegreeOfLatitude = 111195;

    private static readonly Guid ExplorerId = Guid.NewGuid();
    private static readonly Location Start = new(45.2517, 19.8634);
    private static readonly DateTime Now = new(2026, 11, 18, 12, 0, 0, DateTimeKind.Utc);

    private static Location NorthOfStart(double metres) =>
        new(Start.Latitude + metres / MetresPerDegreeOfLatitude, Start.Longitude);

    private static LocationChallenge CreateChallenge() =>
        new("Tajne tvrđave", Start, 300,
            [new HiddenPoint("/api/media/images/sat.jpg", NorthOfStart(120)),
             new HiddenPoint("/api/media/images/kapija.jpg", NorthOfStart(-60))]);

    private static LocationChallengeParticipation CreateParticipation(LocationChallenge challenge) =>
        new(ExplorerId, challenge.Id, new SearchCircle(Start, 150));

    [Fact]
    public void Activation_farther_than_fifty_metres_from_the_start_is_rejected()
    {
        var challenge = CreateChallenge();

        var activation = () => new LocationChallengeHunt().Activate(ExplorerId, challenge, [], NorthOfStart(52), 0);

        activation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Solved_challenge_cannot_be_activated_again()
    {
        var challenge = CreateChallenge();
        var solved = CreateParticipation(challenge);
        solved.Discover(challenge.HiddenPoints[0].Id, 2, 300, Now);
        solved.Discover(challenge.HiddenPoints[1].Id, 2, 300, Now);

        var activation = () => new LocationChallengeHunt().Activate(ExplorerId, challenge, [solved], Start, 0);

        activation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Explorer_with_an_active_challenge_cannot_activate_another()
    {
        var challenge = CreateChallenge();
        var active = CreateParticipation(CreateChallenge());

        var activation = () => new LocationChallengeHunt().Activate(ExplorerId, challenge, [active], Start, 0);

        activation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Search_circle_is_centred_on_the_start_and_reaches_the_farthest_point_plus_the_extra_metres()
    {
        var challenge = CreateChallenge();

        var participation = new LocationChallengeHunt().Activate(ExplorerId, challenge, [], NorthOfStart(45), 37.5);

        participation.ExplorerId.Should().Be(ExplorerId);
        participation.LocationChallengeId.Should().Be(challenge.Id);
        participation.Status.Should().Be(LocationChallengeParticipationStatus.Active);
        participation.SearchCircle.Center.Should().Be(Start);
        participation.SearchCircle.Radius.Should().BeApproximately(120 + 37.5, 1);
    }

    [Theory]
    [InlineData(-0.5)]
    [InlineData(100.5)]
    public void Search_circle_cannot_be_enlarged_by_less_than_zero_or_more_than_a_hundred_metres(double extraMetres)
    {
        var challenge = CreateChallenge();

        var activation = () => new LocationChallengeHunt().Activate(ExplorerId, challenge, [], Start, extraMetres);

        activation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Check_within_five_metres_of_an_undiscovered_point_discovers_it()
    {
        var challenge = CreateChallenge();
        var participation = CreateParticipation(challenge);

        new LocationChallengeHunt().CheckLocation(challenge, participation, NorthOfStart(124), Now);

        participation.DiscoveredPointIds.Should().Equal(challenge.HiddenPoints[0].Id);
        participation.Status.Should().Be(LocationChallengeParticipationStatus.Active);
    }

    [Fact]
    public void Check_farther_than_five_metres_from_every_undiscovered_point_is_rejected()
    {
        var challenge = CreateChallenge();
        var participation = CreateParticipation(challenge);

        var check = () => new LocationChallengeHunt().CheckLocation(challenge, participation, NorthOfStart(126), Now);

        check.Should().Throw<DomainException>();
        participation.DiscoveredPointIds.Should().BeEmpty();
    }

    [Fact]
    public void Check_in_a_solved_challenge_is_rejected_and_the_solution_stays_unchanged()
    {
        var challenge = CreateChallenge();
        var solved = CreateParticipation(challenge);
        solved.Discover(challenge.HiddenPoints[0].Id, 2, 300, Now);
        solved.Discover(challenge.HiddenPoints[1].Id, 2, 300, Now);

        var check = () => new LocationChallengeHunt().CheckLocation(challenge, solved, NorthOfStart(120), Now.AddMinutes(5));

        check.Should().Throw<DomainException>();
        solved.Status.Should().Be(LocationChallengeParticipationStatus.Solved);
        solved.SolvedAt.Should().Be(Now);
        solved.DiscoveredPointIds.Should().HaveCount(2);
    }

    [Fact]
    public void Discovering_the_last_point_records_the_solution_with_its_time_and_experience()
    {
        var challenge = CreateChallenge();
        var participation = CreateParticipation(challenge);
        participation.Discover(challenge.HiddenPoints[0].Id, 2, 300, Now.AddMinutes(-10));

        new LocationChallengeHunt().CheckLocation(challenge, participation, NorthOfStart(-60), Now);

        participation.Status.Should().Be(LocationChallengeParticipationStatus.Solved);
        participation.SolvedAt.Should().Be(Now);
        participation.ExperienceGained.Should().Be(300);
    }
}
