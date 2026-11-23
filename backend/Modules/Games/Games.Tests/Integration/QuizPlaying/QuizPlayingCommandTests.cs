using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.QuizPlaying;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizPlaying;

public class QuizPlayingCommandTests : BaseIntegrationTest
{
    public QuizPlayingCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Last_answer_of_the_first_pass_stores_the_participation_as_solved_with_its_experience()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var participation = QuizParticipationSeed.SynagogueBeforeLastAnswerOfFirstPass;
        var lastQuestion = QuizSeed.PublishedSynagogueQuiz.Questions[2];

        var response = await client.PostAsJsonAsync($"/api/games/quizzes/{participation.QuizId}/answers",
            new SubmitAnswerDto(lastQuestion.Id, 0));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.QuizParticipations.Single(stored => stored.Id == participation.Id);
        stored.SolvedAt.Should().NotBeNull();
        stored.ExperienceGained.Should().Be(QuizSeed.PublishedSynagogueQuiz.ExperiencePoints * 2 / 3);
        stored.Answers.Should().Contain(answer => answer.QuestionId == lastQuestion.Id);
    }

    [Fact]
    public async Task Anonymous_user_cannot_answer_a_quiz_question()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/games/quizzes/{QuizSeed.PublishedSynagogueQuiz.Id}/answers",
            new SubmitAnswerDto(QuizSeed.PublishedSynagogueQuiz.Questions[2].Id, 0));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_answer_a_quiz_question()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync($"/api/games/quizzes/{QuizSeed.PublishedSynagogueQuiz.Id}/answers",
            new SubmitAnswerDto(QuizSeed.PublishedSynagogueQuiz.Questions[2].Id, 0));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Anonymous_user_cannot_start_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.PublishedSynagogueQuiz.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_start_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.PublishedSynagogueQuiz.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
