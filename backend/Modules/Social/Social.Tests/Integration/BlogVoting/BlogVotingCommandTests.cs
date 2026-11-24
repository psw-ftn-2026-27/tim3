using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.BlogVoting;
using Social.Domain.Blogs;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.BlogVoting;

public class BlogVotingCommandTests : BaseIntegrationTest
{
    public BlogVotingCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task User_votes_on_a_published_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CastVoteDto(VoteType.Up);

        var response = await client.PostAsJsonAsync(
            $"/api/social/blogs/{BlogSeed.PublishedRiverbank.Id}/votes", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.PublishedRiverbank.Id);
        stored.Votes.Should().ContainSingle(vote =>
            vote.UserId == WellKnownUsers.Administrator && vote.VoteType == VoteType.Up);
    }

    [Fact]
    public async Task Voting_on_a_blog_that_is_not_published_is_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CastVoteDto(VoteType.Up);

        var response = await client.PostAsJsonAsync(
            $"/api/social/blogs/{BlogSeed.FortressImpressions.Id}/votes", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        stored.Votes.Should().BeEmpty();
    }

    [Fact]
    public async Task Anonymous_user_cannot_vote()
    {
        var client = Factory.CreateClient();
        var request = new CastVoteDto(VoteType.Up);

        var response = await client.PostAsJsonAsync(
            $"/api/social/blogs/{BlogSeed.PublishedRiverbank.Id}/votes", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
