using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.QuizAuthoring;
using Games.Domain.Quizzes;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizAuthoring;

public class QuizAuthoringQueryTests : BaseIntegrationTest
{
    public QuizAuthoringQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Author_lists_only_their_own_quizzes()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var quizzes = await client.GetFromJsonAsync<List<QuizDto>>("/api/games/quizzes/mine", JsonOptions);

        quizzes.Should().HaveCount(QuizSeed.All.Count(quiz => quiz.AuthorId == WellKnownUsers.Explorer));
        quizzes.Should().OnlyContain(quiz => quiz.AuthorId == WellKnownUsers.Explorer);
        quizzes.Should().NotContain(quiz => quiz.Id == QuizSeed.SecondExplorersQuiz.Id);
    }

    [Fact]
    public async Task Author_opens_their_own_draft_quiz_with_its_questions()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var quiz = await client.GetFromJsonAsync<QuizDto>($"/api/games/quizzes/{QuizSeed.FortressQuiz.Id}/edit", JsonOptions);

        quiz!.Id.Should().Be(QuizSeed.FortressQuiz.Id);
        quiz.Status.Should().Be(QuizStatus.Draft);
        quiz.Location.Should().Be(new LocationDto(QuizSeed.FortressQuiz.Location!.Latitude, QuizSeed.FortressQuiz.Location.Longitude));
        var question = quiz.Questions.Should().ContainSingle().Subject;
        question.Id.Should().Be(QuizSeed.FortressQuiz.Questions.Single().Id);
        question.Answers.Should().Equal(QuizSeed.FortressQuiz.Questions.Single().Answers
            .Select(answer => new AnswerDto(answer.Text, answer.IsCorrect)));
    }

    [Fact]
    public async Task Author_cannot_open_another_authors_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync($"/api/games/quizzes/{QuizSeed.SecondExplorersQuiz.Id}/edit");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
