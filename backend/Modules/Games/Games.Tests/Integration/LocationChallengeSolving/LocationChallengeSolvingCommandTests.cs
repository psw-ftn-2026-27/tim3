using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Domain.LocationChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengeSolving;

public class LocationChallengeSolvingCommandTests : BaseIntegrationTest
{
    public LocationChallengeSolvingCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_at_the_start_location_activates_the_challenge_with_a_search_circle()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var oldTown = LocationChallengeSeed.CornersOfTheOldTown;
        var farthestPointDistance = oldTown.HiddenPoints.Max(point => oldTown.StartLocation.DistanceTo(point.Location));
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var participationCountBefore = arrangeContext.LocationChallengeParticipations.Count();

        var response = await client.PostAsync($"/api/games/location-challenges/solving/{oldTown.Id}/activation", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var participationId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallengeParticipations.Count().Should().Be(participationCountBefore + 1);
        var stored = assertContext.LocationChallengeParticipations.Single(participation => participation.Id == participationId);
        stored.ExplorerId.Should().Be(WellKnownUsers.SecondExplorer);
        stored.LocationChallengeId.Should().Be(oldTown.Id);
        stored.Status.Should().Be(LocationChallengeParticipationStatus.Active);
        stored.DiscoveredPointIds.Should().BeEmpty();
        stored.SearchCircle.Center.Should().Be(oldTown.StartLocation);
        stored.SearchCircle.Radius.Should().BeInRange(farthestPointDistance, farthestPointDistance + 100);
    }

    [Fact]
    public async Task Explorer_at_a_hidden_point_discovers_it()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var fortress = LocationChallengeSeed.SecretsOfTheFortress;
        var participation = LocationChallengeParticipationSeed.ExplorerHalfwayThroughTheFortress;
        var undiscovered = fortress.HiddenPoints[1];

        var response = await client.PostAsync($"/api/games/location-challenges/solving/{fortress.Id}/location-check", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallengeParticipations.Single(row => row.Id == participation.Id);
        stored.DiscoveredPointIds.Should().Contain(undiscovered.Id);
    }

    [Fact]
    public async Task Activation_far_from_the_start_location_is_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var participationCountBefore = arrangeContext.LocationChallengeParticipations.Count();

        var response = await client.PostAsync($"/api/games/location-challenges/solving/{LocationChallengeSeed.CornersOfTheOldTown.Id}/activation", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallengeParticipations.Count().Should().Be(participationCountBefore);
    }

    [Fact]
    public async Task Check_away_from_every_hidden_point_is_rejected_and_nothing_is_discovered()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.FourthExplorer, "explorer");
        var participation = LocationChallengeParticipationSeed.FourthExplorerStartedTheFortress;

        var response = await client.PostAsync($"/api/games/location-challenges/solving/{LocationChallengeSeed.SecretsOfTheFortress.Id}/location-check", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallengeParticipations.Single(row => row.Id == participation.Id);
        stored.DiscoveredPointIds.Should().BeEmpty();
    }

    [Fact]
    public async Task Check_without_a_set_position_is_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.FifthExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var participationCountBefore = arrangeContext.LocationChallengeParticipations.Count();

        var response = await client.PostAsync($"/api/games/location-challenges/solving/{LocationChallengeSeed.SecretsOfTheFortress.Id}/location-check", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallengeParticipations.Count().Should().Be(participationCountBefore);
    }

    [Fact]
    public async Task Check_in_a_challenge_the_explorer_has_not_activated_is_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var participationCountBefore = arrangeContext.LocationChallengeParticipations.Count();

        var response = await client.PostAsync($"/api/games/location-challenges/solving/{LocationChallengeSeed.CornersOfTheOldTown.Id}/location-check", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallengeParticipations.Count().Should().Be(participationCountBefore);
    }

    [Fact]
    public async Task Check_in_a_missing_challenge_is_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var participation = LocationChallengeParticipationSeed.ExplorerHalfwayThroughTheFortress;

        var response = await client.PostAsync($"/api/games/location-challenges/solving/{Guid.NewGuid()}/location-check", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallengeParticipations.Single(row => row.Id == participation.Id);
        stored.DiscoveredPointIds.Should().Equal(participation.DiscoveredPointIds);
    }

    [Fact]
    public async Task Anonymous_user_cannot_activate_a_location_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/location-challenges/solving/{LocationChallengeSeed.CornersOfTheOldTown.Id}/activation", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_activate_a_location_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/location-challenges/solving/{LocationChallengeSeed.CornersOfTheOldTown.Id}/activation", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Anonymous_user_cannot_check_a_location()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/location-challenges/solving/{LocationChallengeSeed.SecretsOfTheFortress.Id}/location-check", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_check_a_location()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/location-challenges/solving/{LocationChallengeSeed.SecretsOfTheFortress.Id}/location-check", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
