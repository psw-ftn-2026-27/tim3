using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Social.Application.Blogs;
using Social.Domain.Blogs;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.BlogReading;

public class BlogReadingQueryTests : BaseIntegrationTest
{
    public BlogReadingQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Draft_blogs_are_not_listed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var blogs = await client.GetFromJsonAsync<PageResult<BlogDto>>("/api/social/blogs/published", JsonOptions);

        blogs!.Items.Should().OnlyContain(blog => blog.Status != BlogStatus.Draft);
        blogs.Items.Should().Contain(blog => blog.Id == BlogSeed.PublishedRiverbank.Id);
        blogs.Items.Should().NotContain(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        blogs.TotalCount.Should().Be(BlogSeed.All.Count(blog => blog.Status != BlogStatus.Draft));
    }

    [Fact]
    public async Task Archived_blog_remains_visible_and_readable()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var blog = await client.GetFromJsonAsync<BlogDto>(
            $"/api/social/blogs/{BlogSeed.ArchivedFestival.Id}", JsonOptions);

        blog!.Id.Should().Be(BlogSeed.ArchivedFestival.Id);
        blog.Status.Should().Be(BlogStatus.Closed);
    }

    [Fact]
    public async Task Published_blog_is_shown_with_its_comments()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var blog = await client.GetFromJsonAsync<BlogDto>(
            $"/api/social/blogs/{BlogSeed.CommentedMuseumVisit.Id}", JsonOptions);

        blog!.Id.Should().Be(BlogSeed.CommentedMuseumVisit.Id);
        blog.Title.Should().Be(BlogSeed.CommentedMuseumVisit.Title);
        blog.Status.Should().Be(BlogStatus.Published);
        blog.Comments.Should().ContainSingle(comment => comment.UserId == WellKnownUsers.Administrator);
    }

    [Fact]
    public async Task Comments_are_shown_with_the_authors_display_name_and_last_modified_date()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var blog = await client.GetFromJsonAsync<BlogDto>(
            $"/api/social/blogs/{BlogSeed.CommentedByExplorer.Id}", JsonOptions);

        var comment = blog!.Comments.Should().ContainSingle().Subject;
        comment.UserId.Should().Be(WellKnownUsers.Explorer);
        comment.AuthorDisplayName.Should().Be("explorer");
        comment.LastModifiedAt.Should().Be(comment.CreatedAt);
    }

    [Fact]
    public async Task A_comment_carries_its_edit_deadline_from_the_server()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var blog = await client.GetFromJsonAsync<BlogDto>(
            $"/api/social/blogs/{BlogSeed.CommentedByExplorer.Id}", JsonOptions);

        var comment = blog!.Comments.Should().ContainSingle().Subject;
        comment.EditableUntil.Should().Be(comment.CreatedAt.AddMinutes(15));
    }

    [Fact]
    public async Task Each_reader_sees_their_own_vote_on_the_blog()
    {
        var voterClient = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var otherClient = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var asVoter = await voterClient.GetFromJsonAsync<BlogDto>(
            $"/api/social/blogs/{BlogSeed.VotedCoastline.Id}", JsonOptions);
        var asOther = await otherClient.GetFromJsonAsync<BlogDto>(
            $"/api/social/blogs/{BlogSeed.VotedCoastline.Id}", JsonOptions);

        asVoter!.CurrentUserVote.Should().Be(VoteType.Up);
        asOther!.CurrentUserVote.Should().BeNull();
    }

    [Fact]
    public async Task The_published_list_shows_each_blogs_total_score()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var blogs = await client.GetFromJsonAsync<PageResult<BlogDto>>("/api/social/blogs/published?pageSize=100", JsonOptions);

        blogs!.Items.Should().ContainSingle(blog => blog.Id == BlogSeed.HighlyRatedViewpoint.Id).Which.Score.Should().BePositive();
        blogs.Items.Should().ContainSingle(blog => blog.Id == BlogSeed.UnratedTrail.Id).Which.Score.Should().Be(0);
        blogs.Items.Should().ContainSingle(blog => blog.Id == BlogSeed.PoorlyRatedDetour.Id).Which.Score.Should().BeNegative();
    }

    [Fact]
    public async Task Draft_blog_cannot_be_read()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Unknown_blog_cannot_be_read()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync($"/api/social/blogs/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
