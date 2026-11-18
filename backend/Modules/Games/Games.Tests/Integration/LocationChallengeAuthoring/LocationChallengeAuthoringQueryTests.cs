using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.LocationChallengeAuthoring;
using Games.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengeAuthoring;

public class LocationChallengeAuthoringQueryTests : BaseIntegrationTest
{
    public LocationChallengeAuthoringQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_sees_all_seeded_location_challenges_with_their_point_counts()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var challenges = await client.GetFromJsonAsync<PageResult<LocationChallengeSummaryDto>>("/api/games/location-challenges", JsonOptions);

        challenges!.TotalCount.Should().Be(LocationChallengeSeed.All.Length);
        challenges.Items.Should().BeEquivalentTo(LocationChallengeSeed.All.Select(challenge =>
            new LocationChallengeSummaryDto(challenge.Id, challenge.Name, challenge.ExperienceGained, challenge.HiddenPoints.Count)));
    }

    [Fact]
    public async Task Administrator_fetches_a_location_challenge_with_all_its_hidden_points_to_edit()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var seeded = LocationChallengeSeed.SecretsOfTheFortress;

        var challenge = await client.GetFromJsonAsync<LocationChallengeDto>($"/api/games/location-challenges/{seeded.Id}", JsonOptions);

        challenge.Should().BeEquivalentTo(new LocationChallengeDto(seeded.Id, seeded.Name,
            seeded.StartLocation.Latitude, seeded.StartLocation.Longitude, seeded.ExperienceGained,
            seeded.HiddenPoints.Select(point => new HiddenPointDto(point.Id, point.Photo,
                point.Location.Latitude, point.Location.Longitude)).ToList()));
    }

    [Fact]
    public async Task Missing_location_challenge_cannot_be_fetched()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync($"/api/games/location-challenges/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
