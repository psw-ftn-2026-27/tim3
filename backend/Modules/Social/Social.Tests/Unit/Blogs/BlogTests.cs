using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Blogs;
using Xunit;

namespace Social.Tests.Unit.Blogs;

public class BlogTests
{
    private static Blog CreateBlog() =>
        new(WellKnownUsers.Explorer, "Utisci sa tvrđave", "Opis obilaska tvrđave.", ["https://example.com/tvrdjava.jpg"]);

    private static Blog CreatePublishedBlog()
    {
        var blog = CreateBlog();
        blog.Publish();
        return blog;
    }

    [Fact]
    public void New_blog_starts_as_a_draft()
    {
        var blog = CreateBlog();

        blog.Status.Should().Be(BlogStatus.Draft);
        blog.AuthorId.Should().Be(WellKnownUsers.Explorer);
        blog.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        blog.Comments.Should().BeEmpty();
    }

    [Fact]
    public void New_blog_without_images_has_no_images()
    {
        var blog = new Blog(WellKnownUsers.Explorer, "Utisci sa tvrđave", "Opis obilaska tvrđave.", null);

        blog.Images.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_blog_requires_a_title(string title)
    {
        var creation = () => new Blog(WellKnownUsers.Explorer, title, "Opis obilaska tvrđave.", []);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_blog_requires_a_description(string description)
    {
        var creation = () => new Blog(WellKnownUsers.Explorer, "Utisci sa tvrđave", description, []);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Author_edits_blog_content_and_last_modified_date_moves()
    {
        var blog = CreateBlog();

        blog.Update("Novi naslov", "Nov opis obilaska.", ["https://example.com/nova.jpg"]);

        blog.Title.Should().Be("Novi naslov");
        blog.Description.Should().Be("Nov opis obilaska.");
        blog.Images.Should().ContainSingle().Which.Should().Be("https://example.com/nova.jpg");
        blog.LastModifiedAt.Should().BeOnOrAfter(blog.CreatedAt);
        blog.LastModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Author_edits_blog_to_have_no_images()
    {
        var blog = CreateBlog();

        blog.Update("Novi naslov", "Nov opis obilaska.", null);

        blog.Images.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Edited_blog_requires_a_title(string title)
    {
        var blog = CreateBlog();

        var update = () => blog.Update(title, "Nov opis obilaska.", []);

        update.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Edited_blog_requires_a_description(string description)
    {
        var blog = CreateBlog();

        var update = () => blog.Update("Novi naslov", description, []);

        update.Should().Throw<DomainException>();
    }

    [Fact]
    public void Draft_blog_is_published()
    {
        var blog = CreateBlog();

        blog.Publish();

        blog.Status.Should().Be(BlogStatus.Published);
    }

    [Fact]
    public void Published_blog_cannot_be_published_again()
    {
        var blog = CreatePublishedBlog();

        var publishing = () => blog.Publish();

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_blog_is_archived()
    {
        var blog = CreatePublishedBlog();

        blog.Archive();

        blog.Status.Should().Be(BlogStatus.Closed);
    }

    [Fact]
    public void A_blog_that_is_not_published_cannot_be_archived()
    {
        var blog = CreateBlog();

        var archiving = () => blog.Archive();

        archiving.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_blog_cannot_be_edited()
    {
        var blog = CreatePublishedBlog();
        blog.Archive();

        var update = () => blog.Update("Novi naslov", "Nov opis obilaska.", []);

        update.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_comments_on_a_published_blog()
    {
        var blog = CreatePublishedBlog();

        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");

        blog.Comments.Should().ContainSingle(comment =>
            comment.UserId == WellKnownUsers.Administrator && comment.Text == "Odlična tura!");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Comment_requires_text(string text)
    {
        var blog = CreatePublishedBlog();

        var addition = () => blog.AddComment(WellKnownUsers.Administrator, text);

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_edits_their_own_comment()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();

        blog.UpdateComment(comment.Id, WellKnownUsers.Administrator, "Ipak prosečna tura.", comment.CreatedAt);

        blog.Comments.Single().Text.Should().Be("Ipak prosečna tura.");
    }

    [Fact]
    public void Editing_a_comment_within_the_window_moves_the_last_modified_date_and_keeps_the_created_date()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();
        var createdAt = comment.CreatedAt;
        var editedAt = createdAt.AddMinutes(5);

        blog.UpdateComment(comment.Id, WellKnownUsers.Administrator, "Ipak prosečna tura.", editedAt);

        comment.LastModifiedAt.Should().Be(editedAt);
        comment.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void Editing_a_comment_after_the_window_is_rejected()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();

        var update = () => blog.UpdateComment(comment.Id, WellKnownUsers.Administrator, "Prekasna izmena.",
            comment.CreatedAt.AddMinutes(16));

        update.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_cannot_edit_another_users_comment()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();

        var update = () => blog.UpdateComment(comment.Id, WellKnownUsers.Explorer, "Izmena tuđeg komentara.", comment.CreatedAt);

        update.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void User_deletes_their_own_comment()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();

        blog.DeleteComment(comment.Id, WellKnownUsers.Administrator, comment.CreatedAt);

        blog.Comments.Should().BeEmpty();
    }

    [Fact]
    public void Deleting_a_comment_after_the_window_is_rejected()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();

        var deletion = () => blog.DeleteComment(comment.Id, WellKnownUsers.Administrator, comment.CreatedAt.AddMinutes(16));

        deletion.Should().Throw<DomainException>();
        blog.Comments.Should().ContainSingle();
    }

    [Fact]
    public void User_who_has_not_voted_leaves_a_vote()
    {
        var blog = CreatePublishedBlog();

        blog.CastVote(WellKnownUsers.Explorer, VoteType.Up);

        blog.Votes.Should().ContainSingle(vote =>
            vote.UserId == WellKnownUsers.Explorer && vote.VoteType == VoteType.Up);
    }

    [Fact]
    public void Clicking_the_same_vote_again_clears_it()
    {
        var blog = CreatePublishedBlog();
        blog.CastVote(WellKnownUsers.Explorer, VoteType.Up);

        blog.CastVote(WellKnownUsers.Explorer, VoteType.Up);

        blog.Votes.Should().BeEmpty();
    }

    [Fact]
    public void Clicking_the_opposite_vote_changes_the_mark_and_keeps_one_vote()
    {
        var blog = CreatePublishedBlog();
        blog.CastVote(WellKnownUsers.Explorer, VoteType.Up);

        blog.CastVote(WellKnownUsers.Explorer, VoteType.Down);

        blog.Votes.Should().ContainSingle(vote =>
            vote.UserId == WellKnownUsers.Explorer && vote.VoteType == VoteType.Down);
    }

    [Fact]
    public void Voting_on_a_blog_that_is_not_published_is_rejected()
    {
        var blog = CreateBlog();

        var voting = () => blog.CastVote(WellKnownUsers.Explorer, VoteType.Up);

        voting.Should().Throw<DomainException>();
    }
}
