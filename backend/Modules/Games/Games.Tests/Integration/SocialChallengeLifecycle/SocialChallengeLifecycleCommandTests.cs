using System.Net;
using FluentAssertions;
using Games.Domain.SocialChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengeLifecycle;

public class SocialChallengeLifecycleCommandTests : BaseIntegrationTest
{
    public SocialChallengeLifecycleCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_publishes_a_draft_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.SunsetOnTheFortress.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.SunsetOnTheFortress.Id);
        stored.Status.Should().Be(SocialChallengeStatus.Published);
    }

    [Fact]
    public async Task Published_social_challenge_cannot_be_published_again()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.PublishedNoonOnTheSquare.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.PublishedNoonOnTheSquare.Id);
        stored.Status.Should().Be(SocialChallengeStatus.Published);
    }

    [Fact]
    public async Task Publishing_an_unknown_social_challenge_changes_nothing()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var statusesBefore = arrangeContext.SocialChallenges.Select(challenge => new { challenge.Id, challenge.Status }).ToList();

        var response = await client.PostAsync($"/api/games/social-challenges/{Guid.NewGuid()}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Select(challenge => new { challenge.Id, challenge.Status }).ToList()
            .Should().BeEquivalentTo(statusesBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_publish_a_social_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.SunsetOnTheFortress.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_publish_a_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.SunsetOnTheFortress.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_retires_a_published_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.PublishedNoonOnTheSquare.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.PublishedNoonOnTheSquare.Id);
        stored.Status.Should().Be(SocialChallengeStatus.Retired);
    }

    [Fact]
    public async Task Draft_social_challenge_cannot_be_retired()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.SunsetOnTheFortress.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.SunsetOnTheFortress.Id);
        stored.Status.Should().Be(SocialChallengeStatus.Draft);
    }

    [Fact]
    public async Task Retiring_an_unknown_social_challenge_changes_nothing()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var statusesBefore = arrangeContext.SocialChallenges.Select(challenge => new { challenge.Id, challenge.Status }).ToList();

        var response = await client.PostAsync($"/api/games/social-challenges/{Guid.NewGuid()}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Select(challenge => new { challenge.Id, challenge.Status }).ToList()
            .Should().BeEquivalentTo(statusesBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_retire_a_social_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.PublishedNoonOnTheSquare.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_retire_a_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.PublishedNoonOnTheSquare.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_publishes_a_retired_social_challenge_again()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.RetiredMorningOnTheQuay.Id}/reactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.RetiredMorningOnTheQuay.Id);
        stored.Status.Should().Be(SocialChallengeStatus.Published);
    }

    [Fact]
    public async Task Archived_social_challenge_cannot_be_published_again()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.ArchivedPicnicInThePark.Id}/reactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.ArchivedPicnicInThePark.Id);
        stored.Status.Should().Be(SocialChallengeStatus.Archived);
    }

    [Fact]
    public async Task Republishing_an_unknown_social_challenge_changes_nothing()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var statusesBefore = arrangeContext.SocialChallenges.Select(challenge => new { challenge.Id, challenge.Status }).ToList();

        var response = await client.PostAsync($"/api/games/social-challenges/{Guid.NewGuid()}/reactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Select(challenge => new { challenge.Id, challenge.Status }).ToList()
            .Should().BeEquivalentTo(statusesBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_publish_a_social_challenge_again()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.RetiredMorningOnTheQuay.Id}/reactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_publish_a_social_challenge_again()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.RetiredMorningOnTheQuay.Id}/reactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_archives_a_retired_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.RetiredMorningOnTheQuay.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.RetiredMorningOnTheQuay.Id);
        stored.Status.Should().Be(SocialChallengeStatus.Archived);
    }

    [Fact]
    public async Task Draft_social_challenge_cannot_be_archived()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.SunsetOnTheFortress.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.SunsetOnTheFortress.Id);
        stored.Status.Should().Be(SocialChallengeStatus.Draft);
    }

    [Fact]
    public async Task Archiving_an_unknown_social_challenge_changes_nothing()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var statusesBefore = arrangeContext.SocialChallenges.Select(challenge => new { challenge.Id, challenge.Status }).ToList();

        var response = await client.PostAsync($"/api/games/social-challenges/{Guid.NewGuid()}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Select(challenge => new { challenge.Id, challenge.Status }).ToList()
            .Should().BeEquivalentTo(statusesBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_archive_a_social_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.RetiredMorningOnTheQuay.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_archive_a_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.RetiredMorningOnTheQuay.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_deletes_an_archived_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.DeleteAsync($"/api/games/social-challenges/{SocialChallengeSeed.ArchivedPicnicInThePark.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore - 1);
        assertContext.SocialChallenges.Should().NotContain(challenge => challenge.Id == SocialChallengeSeed.ArchivedPicnicInThePark.Id);
    }

    [Fact]
    public async Task Published_social_challenge_cannot_be_deleted()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync($"/api/games/social-challenges/{SocialChallengeSeed.PublishedNoonOnTheSquare.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.PublishedNoonOnTheSquare.Id);
        stored.Status.Should().Be(SocialChallengeStatus.Published);
    }

    [Fact]
    public async Task Deleting_an_unknown_social_challenge_changes_nothing()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.DeleteAsync($"/api/games/social-challenges/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_a_social_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/games/social-challenges/{SocialChallengeSeed.ArchivedPicnicInThePark.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_delete_a_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync($"/api/games/social-challenges/{SocialChallengeSeed.ArchivedPicnicInThePark.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
