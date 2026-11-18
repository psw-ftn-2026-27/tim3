using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.LocationChallengeAuthoring;
using Games.Domain.LocationChallenges;
using Games.Domain.Locations;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengeAuthoring;

public class LocationChallengeAuthoringCommandTests : BaseIntegrationTest
{
    public LocationChallengeAuthoringCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_creates_a_location_challenge_with_all_its_hidden_points()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateLocationChallengeDto("Skriveni Dunavski park", 45.2546, 19.8510, 400,
            [new CreateHiddenPointDto("/api/media/images/fontana.jpg", 45.2549, 19.8515),
             new CreateHiddenPointDto("/api/media/images/most-u-parku.jpg", 45.2541, 19.8503)]);
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();

        var response = await client.PostAsJsonAsync("/api/games/location-challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore + 1);
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == createdId);
        stored.Name.Should().Be("Skriveni Dunavski park");
        stored.StartLocation.Should().Be(new Location(45.2546, 19.8510));
        stored.ExperienceGained.Should().Be(400);
        stored.HiddenPoints.Select(point => (point.Photo, point.Location)).Should().BeEquivalentTo(
        [
            ("/api/media/images/fontana.jpg", new Location(45.2549, 19.8515)),
            ("/api/media/images/most-u-parku.jpg", new Location(45.2541, 19.8503))
        ]);
    }

    [Fact]
    public async Task Location_challenge_without_hidden_points_is_not_saved()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateLocationChallengeDto("Skriveni Dunavski park", 45.2546, 19.8510, 400, []);
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();

        var response = await client.PostAsJsonAsync("/api/games/location-challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_location_challenge()
    {
        var client = Factory.CreateClient();
        var request = new CreateLocationChallengeDto("Skriveni Dunavski park", 45.2546, 19.8510, 400,
            [new CreateHiddenPointDto("/api/media/images/fontana.jpg", 45.2549, 19.8515)]);

        var response = await client.PostAsJsonAsync("/api/games/location-challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_create_a_location_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateLocationChallengeDto("Skriveni Dunavski park", 45.2546, 19.8510, 400,
            [new CreateHiddenPointDto("/api/media/images/fontana.jpg", 45.2549, 19.8515)]);

        var response = await client.PostAsJsonAsync("/api/games/location-challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_replaces_the_hidden_points_with_the_new_list()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new UpdateLocationChallengeDto("Tajne tvrđave i podgrađa", 45.2520, 19.8630, 500,
            [new CreateHiddenPointDto("/api/media/images/bedem.jpg", 45.2531, 19.8660)]);

        var response = await client.PutAsJsonAsync($"/api/games/location-challenges/{LocationChallengeSeed.SecretsOfTheFortress.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == LocationChallengeSeed.SecretsOfTheFortress.Id);
        stored.Name.Should().Be("Tajne tvrđave i podgrađa");
        stored.StartLocation.Should().Be(new Location(45.2520, 19.8630));
        stored.ExperienceGained.Should().Be(500);
        stored.HiddenPoints.Select(point => (point.Photo, point.Location)).Should().BeEquivalentTo(
        [
            ("/api/media/images/bedem.jpg", new Location(45.2531, 19.8660))
        ]);
        var replacedPointIds = LocationChallengeSeed.SecretsOfTheFortress.HiddenPoints.Select(point => point.Id).ToList();
        assertContext.Set<HiddenPoint>().Should().NotContain(point => replacedPointIds.Contains(point.Id));
    }

    [Fact]
    public async Task Anonymous_user_cannot_update_a_location_challenge()
    {
        var client = Factory.CreateClient();
        var request = new UpdateLocationChallengeDto("Tajne tvrđave i podgrađa", 45.2520, 19.8630, 500,
            [new CreateHiddenPointDto("/api/media/images/bedem.jpg", 45.2531, 19.8660)]);

        var response = await client.PutAsJsonAsync($"/api/games/location-challenges/{LocationChallengeSeed.SecretsOfTheFortress.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_update_a_location_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new UpdateLocationChallengeDto("Tajne tvrđave i podgrađa", 45.2520, 19.8630, 500,
            [new CreateHiddenPointDto("/api/media/images/bedem.jpg", 45.2531, 19.8660)]);

        var response = await client.PutAsJsonAsync($"/api/games/location-challenges/{LocationChallengeSeed.SecretsOfTheFortress.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Deleted_location_challenge_no_longer_exists()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();

        var response = await client.DeleteAsync($"/api/games/location-challenges/{LocationChallengeSeed.CornersOfTheOldTown.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore - 1);
        assertContext.LocationChallenges.Should().NotContain(challenge => challenge.Id == LocationChallengeSeed.CornersOfTheOldTown.Id);
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_a_location_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/games/location-challenges/{LocationChallengeSeed.CornersOfTheOldTown.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_delete_a_location_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync($"/api/games/location-challenges/{LocationChallengeSeed.CornersOfTheOldTown.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Missing_location_challenge_cannot_be_updated()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new UpdateLocationChallengeDto("Tajne tvrđave i podgrađa", 45.2520, 19.8630, 500,
            [new CreateHiddenPointDto("/api/media/images/bedem.jpg", 45.2531, 19.8660)]);

        var response = await client.PutAsJsonAsync($"/api/games/location-challenges/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Missing_location_challenge_cannot_be_deleted()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();

        var response = await client.DeleteAsync($"/api/games/location-challenges/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore);
    }
}
