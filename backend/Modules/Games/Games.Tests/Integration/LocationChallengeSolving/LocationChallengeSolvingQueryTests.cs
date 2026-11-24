using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.LocationChallengeSolving;
using Games.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengeSolving;

public class LocationChallengeSolvingQueryTests : BaseIntegrationTest
{
    public LocationChallengeSolvingQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_sees_every_location_challenge_with_its_start_experience_and_own_state()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var fortress = LocationChallengeSeed.SecretsOfTheFortress;
        var oldTown = LocationChallengeSeed.CornersOfTheOldTown;

        var challenges = await client.GetFromJsonAsync<PageResult<LocationChallengeToSolveDto>>("/api/games/location-challenges/solving", JsonOptions);

        challenges!.TotalCount.Should().Be(LocationChallengeSeed.All.Length);
        challenges.Items.Should().BeEquivalentTo(new[]
        {
            new LocationChallengeToSolveDto(fortress.Id, fortress.Name, fortress.StartLocation.Latitude,
                fortress.StartLocation.Longitude, fortress.ExperienceGained, LocationChallengeState.Active),
            new LocationChallengeToSolveDto(oldTown.Id, oldTown.Name, oldTown.StartLocation.Latitude,
                oldTown.StartLocation.Longitude, oldTown.ExperienceGained, LocationChallengeState.Solved)
        });
    }

    [Fact]
    public async Task Explorer_without_participations_sees_every_location_challenge_as_not_started()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var challenges = await client.GetFromJsonAsync<PageResult<LocationChallengeToSolveDto>>("/api/games/location-challenges/solving", JsonOptions);

        challenges!.Items.Select(challenge => challenge.Id)
            .Should().BeEquivalentTo(LocationChallengeSeed.All.Select(challenge => challenge.Id));
        challenges.Items.Should().OnlyContain(challenge => challenge.State == LocationChallengeState.NotStarted);
    }

    [Fact]
    public async Task Challenge_view_does_not_contain_the_location_of_an_undiscovered_point()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var fortress = LocationChallengeSeed.SecretsOfTheFortress;
        var participation = LocationChallengeParticipationSeed.ExplorerHalfwayThroughTheFortress;
        var discovered = fortress.HiddenPoints[0];
        var undiscovered = fortress.HiddenPoints[1];

        var challenge = await client.GetFromJsonAsync<LocationChallengeHuntDto>($"/api/games/location-challenges/solving/{fortress.Id}", JsonOptions);

        challenge.Should().BeEquivalentTo(new LocationChallengeHuntDto(fortress.Id, fortress.Name,
            fortress.ExperienceGained, LocationChallengeState.Active,
            new SearchCircleDto(participation.SearchCircle.Center.Latitude,
                participation.SearchCircle.Center.Longitude, participation.SearchCircle.Radius),
            [new HiddenPointClueDto(discovered.Id, discovered.Photo, true, discovered.Location.Latitude, discovered.Location.Longitude),
             new HiddenPointClueDto(undiscovered.Id, undiscovered.Photo, false, null, null)]));
    }

    [Fact]
    public async Task Challenge_view_of_a_not_started_challenge_has_photos_but_no_search_circle_or_locations()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var fortress = LocationChallengeSeed.SecretsOfTheFortress;

        var challenge = await client.GetFromJsonAsync<LocationChallengeHuntDto>($"/api/games/location-challenges/solving/{fortress.Id}", JsonOptions);

        challenge.Should().BeEquivalentTo(new LocationChallengeHuntDto(fortress.Id, fortress.Name,
            fortress.ExperienceGained, LocationChallengeState.NotStarted, null,
            fortress.HiddenPoints.Select(point => new HiddenPointClueDto(point.Id, point.Photo, false, null, null)).ToList()));
    }

    [Fact]
    public async Task Missing_location_challenge_cannot_be_viewed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync($"/api/games/location-challenges/solving/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Anonymous_user_cannot_list_location_challenges_to_solve()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/games/location-challenges/solving");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_list_location_challenges_to_solve()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync("/api/games/location-challenges/solving");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Anonymous_user_cannot_view_a_location_challenge_to_solve()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync($"/api/games/location-challenges/solving/{LocationChallengeSeed.SecretsOfTheFortress.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_view_a_location_challenge_to_solve()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync($"/api/games/location-challenges/solving/{LocationChallengeSeed.SecretsOfTheFortress.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
