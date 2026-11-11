using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.QuizAuthoring;
using Games.Domain.Quizzes;
using Games.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizAuthoring;

public class QuizAuthoringQueryTests : BaseIntegrationTest
{
    public QuizAuthoringQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_lists_the_quizzes_of_every_administrator()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var quizzes = await client.GetFromJsonAsync<PageResult<QuizDto>>("/api/games/quizzes", JsonOptions);

        quizzes!.Items.Should().Contain(quiz => quiz.Id == QuizSeed.FortressQuiz.Id);
        quizzes.Items.Should().Contain(quiz => quiz.Id == QuizSeed.OtherAdministratorsQuiz.Id);
        quizzes.TotalCount.Should().Be(QuizSeed.All.Length);
    }

    [Fact]
    public async Task Quizzes_are_listed_page_by_page()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var quizzes = await client.GetFromJsonAsync<PageResult<QuizDto>>("/api/games/quizzes?page=2&pageSize=1", JsonOptions);

        quizzes!.Items.Should().ContainSingle();
        quizzes.TotalCount.Should().Be(QuizSeed.All.Length);
    }

    [Fact]
    public async Task Administrator_opens_a_draft_quiz_with_its_questions()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var quiz = await client.GetFromJsonAsync<QuizDto>($"/api/games/quizzes/{QuizSeed.FortressQuiz.Id}/edit", JsonOptions);

        quiz!.Id.Should().Be(QuizSeed.FortressQuiz.Id);
        quiz.Status.Should().Be(QuizStatus.Draft);
        var question = quiz.Questions.Should().ContainSingle().Subject;
        question.Id.Should().Be(QuizSeed.FortressQuiz.Questions.Single().Id);
        question.Answers.Should().Equal(QuizSeed.FortressQuiz.Questions.Single().Answers
            .Select(answer => new AnswerDto(answer.Text, answer.IsCorrect)));
    }

    [Fact]
    public async Task Administrator_opens_a_quiz_made_by_another_administrator()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var quiz = await client.GetFromJsonAsync<QuizDto>($"/api/games/quizzes/{QuizSeed.OtherAdministratorsQuiz.Id}/edit", JsonOptions);

        quiz!.Id.Should().Be(QuizSeed.OtherAdministratorsQuiz.Id);
        quiz.AuthorId.Should().Be(QuizSeed.OtherAdministrator);
        quiz.Questions.Should().ContainSingle();
    }
}
