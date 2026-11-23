using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Games.Application.QuizPlaying;
using Games.Domain.Quizzes;
using Games.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizPlaying;

public class QuizPlayingQueryTests : BaseIntegrationTest
{
    public QuizPlayingQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_lists_only_published_quizzes_marked_as_solved_by_that_explorer()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var publishedQuizIds = QuizSeed.All
            .Where(quiz => quiz.Status == QuizStatus.Published && quiz.Location is not null)
            .Select(quiz => quiz.Id);
        var solved = QuizParticipationSeed.SolvedDanubeParkWithTwoCorrect;

        var quizzes = await client.GetFromJsonAsync<PageResult<PublishedQuizDto>>("/api/games/quizzes/published", JsonOptions);

        quizzes!.Items.Select(quiz => quiz.Id).Should().BeEquivalentTo(publishedQuizIds);
        var solvedQuiz = quizzes.Items.Single(quiz => quiz.Id == solved.QuizId);
        solvedQuiz.IsSolved.Should().BeTrue();
        solvedQuiz.CorrectAnswerCount.Should().Be(solved.Answers.Count(answer => answer.IsCorrect));
        quizzes.Items.Single(quiz => quiz.Id == QuizSeed.PublishedSynagogueQuiz.Id).IsSolved.Should().BeFalse();
    }

    [Fact]
    public async Task Play_state_does_not_reveal_the_correct_answer_of_the_current_question()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var participation = QuizParticipationSeed.SynagogueBeforeLastAnswerOfFirstPass;

        var body = await client.GetStringAsync($"/api/games/quizzes/{participation.QuizId}/play");

        var play = JsonSerializer.Deserialize<QuizPlayDto>(body, JsonOptions)!;
        play.CurrentQuestion!.Id.Should().Be(participation.NextQuestionId()!.Value);
        using var document = JsonDocument.Parse(body);
        var currentQuestion = document.RootElement.GetProperty("currentQuestion");
        currentQuestion.GetRawText().Should().NotContainEquivalentOf("correct");
        currentQuestion.GetProperty("answers").EnumerateArray()
            .Should().OnlyContain(answer => answer.ValueKind == JsonValueKind.String);
    }
}
