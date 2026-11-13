using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.SocialChallengeAuthoring;
using Games.Domain.SocialChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengeAuthoring;

public class SocialChallengeAuthoringCommandTests : BaseIntegrationTest
{
    public SocialChallengeAuthoringCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_creates_a_draft_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateSocialChallengeDto("Jutro na keju", new TimeOnly(7, 0), new TimeOnly(9, 0), 4, 5, 150, "Dobro jutro, ekipo!");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.PostAsJsonAsync("/api/games/social-challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await response.Content.ReadFromJsonAsync<SocialChallengeDto>(JsonOptions);
        created!.Status.Should().Be(SocialChallengeStatus.Draft);
        created.Name.Should().Be("Jutro na keju");
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore + 1);
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == created.Id);
        stored.Status.Should().Be(SocialChallengeStatus.Draft);
        stored.ActivePeriod.Should().Be(new ActivePeriod(new TimeOnly(7, 0), new TimeOnly(9, 0)));
        stored.PeopleRequired.Should().Be(4);
    }

    [Fact]
    public async Task Invalid_social_challenge_is_not_saved()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateSocialChallengeDto("Gužva na trgu", new TimeOnly(12, 0), new TimeOnly(13, 0), 71, 15, 150, "Uspeli ste!");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.PostAsJsonAsync("/api/games/social-challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_social_challenge()
    {
        var client = Factory.CreateClient();
        var request = new CreateSocialChallengeDto("Jutro na keju", new TimeOnly(7, 0), new TimeOnly(9, 0), 4, 5, 150, "Dobro jutro, ekipo!");

        var response = await client.PostAsJsonAsync("/api/games/social-challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_create_a_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateSocialChallengeDto("Jutro na keju", new TimeOnly(7, 0), new TimeOnly(9, 0), 4, 5, 150, "Dobro jutro, ekipo!");

        var response = await client.PostAsJsonAsync("/api/games/social-challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_updates_a_draft_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new UpdateSocialChallengeDto("Zalazak na bedemu", new TimeOnly(19, 0), new TimeOnly(21, 0), 8, 12, 500, "Zalazak je vaš!");

        var response = await client.PutAsJsonAsync($"/api/games/social-challenges/{SocialChallengeSeed.SunsetOnTheFortress.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.SunsetOnTheFortress.Id);
        stored.Name.Should().Be("Zalazak na bedemu");
        stored.ActivePeriod.Should().Be(new ActivePeriod(new TimeOnly(19, 0), new TimeOnly(21, 0)));
        stored.PeopleRequired.Should().Be(8);
        stored.GatheringRadius.Should().Be(12);
        stored.ExperienceGained.Should().Be(500);
        stored.EndMessage.Should().Be("Zalazak je vaš!");
    }

    [Fact]
    public async Task Updating_an_unknown_social_challenge_changes_nothing()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new UpdateSocialChallengeDto("Nepostojeći izazov", new TimeOnly(19, 0), new TimeOnly(21, 0), 8, 12, 500, "Kraj.");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.PutAsJsonAsync($"/api/games/social-challenges/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore);
        assertContext.SocialChallenges.Should().NotContain(challenge => challenge.Name == "Nepostojeći izazov");
    }

    [Fact]
    public async Task Anonymous_user_cannot_update_a_social_challenge()
    {
        var client = Factory.CreateClient();
        var request = new UpdateSocialChallengeDto("Zalazak na bedemu", new TimeOnly(19, 0), new TimeOnly(21, 0), 8, 12, 500, "Zalazak je vaš!");

        var response = await client.PutAsJsonAsync($"/api/games/social-challenges/{SocialChallengeSeed.SunsetOnTheFortress.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_update_a_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new UpdateSocialChallengeDto("Zalazak na bedemu", new TimeOnly(19, 0), new TimeOnly(21, 0), 8, 12, 500, "Zalazak je vaš!");

        var response = await client.PutAsJsonAsync($"/api/games/social-challenges/{SocialChallengeSeed.SunsetOnTheFortress.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
