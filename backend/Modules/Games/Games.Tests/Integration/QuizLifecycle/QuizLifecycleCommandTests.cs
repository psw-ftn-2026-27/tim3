using System.Net;
using FluentAssertions;
using Games.Domain.Quizzes;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizLifecycle;

public class QuizLifecycleCommandTests : BaseIntegrationTest
{
    public QuizLifecycleCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_publishes_a_quiz_that_is_stored_as_published()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.ReadyToPublishCathedralQuiz.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.ReadyToPublishCathedralQuiz.Id);
        stored.Status.Should().Be(QuizStatus.Published);
    }

    [Fact]
    public async Task Quiz_rejected_by_the_domain_stays_a_draft()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.FortressQuiz.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.FortressQuiz.Id);
        stored.Status.Should().Be(QuizStatus.Draft);
    }

    [Fact]
    public async Task Publication_of_a_missing_quiz_is_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var missingQuizId = Guid.NewGuid();
        int publishedCount;
        using (var arrangeContext = Factory.CreateContext<GamesDbContext>())
        {
            publishedCount = arrangeContext.Quizzes.Count(quiz => quiz.Status == QuizStatus.Published);
        }

        var response = await client.PostAsync($"/api/games/quizzes/{missingQuizId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count(quiz => quiz.Status == QuizStatus.Published).Should().Be(publishedCount);
    }

    [Fact]
    public async Task Anonymous_user_cannot_publish_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.ReadyToPublishCathedralQuiz.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_publish_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.ReadyToPublishCathedralQuiz.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_retires_a_published_quiz_that_is_stored_as_retired()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.PublishedSynagogueQuiz.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.PublishedSynagogueQuiz.Id);
        stored.Status.Should().Be(QuizStatus.Retired);
    }

    [Fact]
    public async Task Retirement_of_a_missing_quiz_is_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var missingQuizId = Guid.NewGuid();
        int retiredCount;
        using (var arrangeContext = Factory.CreateContext<GamesDbContext>())
        {
            retiredCount = arrangeContext.Quizzes.Count(quiz => quiz.Status == QuizStatus.Retired);
        }

        var response = await client.PostAsync($"/api/games/quizzes/{missingQuizId}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count(quiz => quiz.Status == QuizStatus.Retired).Should().Be(retiredCount);
    }

    [Fact]
    public async Task Anonymous_user_cannot_retire_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.PublishedSynagogueQuiz.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_retire_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.PublishedSynagogueQuiz.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_archives_a_retired_quiz_that_is_stored_as_archived()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.RetiredBridgeQuiz.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.RetiredBridgeQuiz.Id);
        stored.Status.Should().Be(QuizStatus.Archived);
    }

    [Fact]
    public async Task Published_quiz_rejected_for_archiving_by_the_domain_stays_published()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.PublishedSynagogueQuiz.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.PublishedSynagogueQuiz.Id);
        stored.Status.Should().Be(QuizStatus.Published);
    }

    [Fact]
    public async Task Archiving_of_a_missing_quiz_is_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var missingQuizId = Guid.NewGuid();
        int archivedCount;
        using (var arrangeContext = Factory.CreateContext<GamesDbContext>())
        {
            archivedCount = arrangeContext.Quizzes.Count(quiz => quiz.Status == QuizStatus.Archived);
        }

        var response = await client.PostAsync($"/api/games/quizzes/{missingQuizId}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count(quiz => quiz.Status == QuizStatus.Archived).Should().Be(archivedCount);
    }

    [Fact]
    public async Task Anonymous_user_cannot_archive_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.RetiredBridgeQuiz.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_archive_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.RetiredBridgeQuiz.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
