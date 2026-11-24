using System.Net;
using FluentAssertions;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengePlaying;

public class SocialChallengePlayingCommandTests : BaseIntegrationTest
{
    public SocialChallengePlayingCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Activation_with_a_stale_position_is_rejected_and_no_execution_is_written()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var executionCountBefore = arrangeContext.SocialChallengeExecutions.Count();

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.PublishedNoonOnTheSquare.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallengeExecutions.Count().Should().Be(executionCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_activate_a_social_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.PublishedNoonOnTheSquare.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_activate_a_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.PublishedNoonOnTheSquare.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Anonymous_user_cannot_report_a_position_in_a_social_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.PublishedNoonOnTheSquare.Id}/position-reports", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_report_a_position_in_a_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.PublishedNoonOnTheSquare.Id}/position-reports", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
