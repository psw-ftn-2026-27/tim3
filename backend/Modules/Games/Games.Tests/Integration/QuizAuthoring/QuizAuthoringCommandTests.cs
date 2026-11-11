using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.QuizAuthoring;
using Games.Domain.Quizzes;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizAuthoring;

public class QuizAuthoringCommandTests : BaseIntegrationTest
{
    public QuizAuthoringCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_creates_a_draft_quiz_as_its_author()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateQuizDto("Sahat kula", "Kratak kviz o Sahat kuli.", new LocationDto(45.2530, 19.8610));
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.PostAsJsonAsync("/api/games/quizzes", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore + 1);
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == createdId);
        stored.AuthorId.Should().Be(WellKnownUsers.Explorer);
        stored.Status.Should().Be(QuizStatus.Draft);
        stored.Title.Should().Be("Sahat kula");
        stored.Location.Should().Be(new Location(45.2530, 19.8610));
        stored.Questions.Should().BeEmpty();
    }

    [Fact]
    public async Task Quiz_without_a_title_is_not_saved()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateQuizDto("   ", "Kratak kviz.", null);
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.PostAsJsonAsync("/api/games/quizzes", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_quiz()
    {
        var client = Factory.CreateClient();
        var request = new CreateQuizDto("Sahat kula", "Kratak kviz.", null);

        var response = await client.PostAsJsonAsync("/api/games/quizzes", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_create_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateQuizDto("Sahat kula", "Kratak kviz.", null);

        var response = await client.PostAsJsonAsync("/api/games/quizzes", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_updates_the_quiz_details()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new UpdateQuizDetailsDto("Muzej Vojvodine i okolina", "Nov opis kviza.", new LocationDto(45.2560, 19.8470));

        var response = await client.PutAsJsonAsync($"/api/games/quizzes/{QuizSeed.EmptyMuseumQuiz.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.EmptyMuseumQuiz.Id);
        stored.Title.Should().Be("Muzej Vojvodine i okolina");
        stored.Description.Should().Be("Nov opis kviza.");
        stored.Location.Should().Be(new Location(45.2560, 19.8470));
    }

    [Fact]
    public async Task Details_with_a_location_outside_the_bounds_are_not_saved()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new UpdateQuizDetailsDto("Nov naziv", "Nov opis kviza.", new LocationDto(95, 19.8470));

        var response = await client.PutAsJsonAsync($"/api/games/quizzes/{QuizSeed.FortressQuiz.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.FortressQuiz.Id);
        stored.Title.Should().Be(QuizSeed.FortressQuiz.Title);
        stored.Location.Should().Be(QuizSeed.FortressQuiz.Location);
    }

    [Fact]
    public async Task Author_cannot_update_another_authors_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new UpdateQuizDetailsDto("Preuzet kviz", "Izmena tuđeg kviza.", null);

        var response = await client.PutAsJsonAsync($"/api/games/quizzes/{QuizSeed.SecondExplorersQuiz.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.SecondExplorersQuiz.Id);
        stored.Title.Should().Be(QuizSeed.SecondExplorersQuiz.Title);
        stored.Location.Should().Be(QuizSeed.SecondExplorersQuiz.Location);
    }

    [Fact]
    public async Task Anonymous_user_cannot_update_quiz_details()
    {
        var client = Factory.CreateClient();
        var request = new UpdateQuizDetailsDto("Nov naziv", "Nov opis kviza.", null);

        var response = await client.PutAsJsonAsync($"/api/games/quizzes/{QuizSeed.FortressQuiz.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_update_quiz_details()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new UpdateQuizDetailsDto("Nov naziv", "Nov opis kviza.", null);

        var response = await client.PutAsJsonAsync($"/api/games/quizzes/{QuizSeed.FortressQuiz.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_adds_a_question_to_the_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateQuestionDto("Kada je osnovan muzej?",
            [new AnswerDto("1847.", true), new AnswerDto("1947.", false)]);

        var response = await client.PostAsJsonAsync($"/api/games/quizzes/{QuizSeed.EmptyMuseumQuiz.Id}/questions", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.EmptyMuseumQuiz.Id);
        var question = stored.Questions.Should().ContainSingle().Subject;
        question.Text.Should().Be("Kada je osnovan muzej?");
        question.Answers.Should().Equal(new Answer("1847.", true), new Answer("1947.", false));
    }

    [Fact]
    public async Task Added_question_with_two_correct_answers_is_not_saved()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateQuestionDto("Kada je osnovan muzej?",
            [new AnswerDto("1847.", true), new AnswerDto("1947.", true)]);
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var questionCountBefore = arrangeContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.FortressQuiz.Id).Questions.Count;

        var response = await client.PostAsJsonAsync($"/api/games/quizzes/{QuizSeed.FortressQuiz.Id}/questions", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.FortressQuiz.Id);
        stored.Questions.Should().HaveCount(questionCountBefore);
    }

    [Fact]
    public async Task Author_cannot_add_a_question_to_another_authors_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateQuestionDto("Tuđe pitanje?", [new AnswerDto("Da", true), new AnswerDto("Ne", false)]);
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var questionCountBefore = arrangeContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.SecondExplorersQuiz.Id).Questions.Count;

        var response = await client.PostAsJsonAsync($"/api/games/quizzes/{QuizSeed.SecondExplorersQuiz.Id}/questions", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.SecondExplorersQuiz.Id);
        stored.Questions.Should().HaveCount(questionCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_add_a_question()
    {
        var client = Factory.CreateClient();
        var request = new CreateQuestionDto("Pitanje?", [new AnswerDto("Da", true), new AnswerDto("Ne", false)]);

        var response = await client.PostAsJsonAsync($"/api/games/quizzes/{QuizSeed.FortressQuiz.Id}/questions", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_add_a_question()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateQuestionDto("Pitanje?", [new AnswerDto("Da", true), new AnswerDto("Ne", false)]);

        var response = await client.PostAsJsonAsync($"/api/games/quizzes/{QuizSeed.FortressQuiz.Id}/questions", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_updates_a_question()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var questionId = QuizSeed.FortressQuiz.Questions.Single().Id;
        var request = new UpdateQuestionDto("Uz koju reku je tvrđava?",
            [new AnswerDto("Sava", false), new AnswerDto("Dunav", true)]);

        var response = await client.PutAsJsonAsync(
            $"/api/games/quizzes/{QuizSeed.FortressQuiz.Id}/questions/{questionId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.FortressQuiz.Id);
        var question = stored.Questions.Single(question => question.Id == questionId);
        question.Text.Should().Be("Uz koju reku je tvrđava?");
        question.Answers.Should().Equal(new Answer("Sava", false), new Answer("Dunav", true));
    }

    [Fact]
    public async Task Edited_question_with_two_correct_answers_is_not_saved()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var seededQuestion = QuizSeed.FortressQuiz.Questions.Single();
        var request = new UpdateQuestionDto("Uz koju reku je tvrđava?",
            [new AnswerDto("Sava", true), new AnswerDto("Dunav", true)]);

        var response = await client.PutAsJsonAsync(
            $"/api/games/quizzes/{QuizSeed.FortressQuiz.Id}/questions/{seededQuestion.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.FortressQuiz.Id);
        var question = stored.Questions.Single(question => question.Id == seededQuestion.Id);
        question.Text.Should().Be(seededQuestion.Text);
        question.Answers.Should().Equal(seededQuestion.Answers);
    }

    [Fact]
    public async Task Author_cannot_update_a_question_of_another_authors_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var seededQuestion = QuizSeed.SecondExplorersQuiz.Questions.Single();
        var request = new UpdateQuestionDto("Preuzeto pitanje?", [new AnswerDto("Da", true), new AnswerDto("Ne", false)]);

        var response = await client.PutAsJsonAsync(
            $"/api/games/quizzes/{QuizSeed.SecondExplorersQuiz.Id}/questions/{seededQuestion.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.SecondExplorersQuiz.Id);
        var question = stored.Questions.Single(question => question.Id == seededQuestion.Id);
        question.Text.Should().Be(seededQuestion.Text);
        question.Answers.Should().Equal(seededQuestion.Answers);
    }

    [Fact]
    public async Task Anonymous_user_cannot_update_a_question()
    {
        var client = Factory.CreateClient();
        var questionId = QuizSeed.FortressQuiz.Questions.Single().Id;
        var request = new UpdateQuestionDto("Pitanje?", [new AnswerDto("Da", true), new AnswerDto("Ne", false)]);

        var response = await client.PutAsJsonAsync(
            $"/api/games/quizzes/{QuizSeed.FortressQuiz.Id}/questions/{questionId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_update_a_question()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var questionId = QuizSeed.FortressQuiz.Questions.Single().Id;
        var request = new UpdateQuestionDto("Pitanje?", [new AnswerDto("Da", true), new AnswerDto("Ne", false)]);

        var response = await client.PutAsJsonAsync(
            $"/api/games/quizzes/{QuizSeed.FortressQuiz.Id}/questions/{questionId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_removes_a_question()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var questionId = QuizSeed.FortressQuiz.Questions.Single().Id;

        var response = await client.DeleteAsync($"/api/games/quizzes/{QuizSeed.FortressQuiz.Id}/questions/{questionId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.FortressQuiz.Id);
        stored.Questions.Should().NotContain(question => question.Id == questionId);
    }

    [Fact]
    public async Task Author_cannot_remove_a_question_from_another_authors_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var questionId = QuizSeed.SecondExplorersQuiz.Questions.Single().Id;

        var response = await client.DeleteAsync($"/api/games/quizzes/{QuizSeed.SecondExplorersQuiz.Id}/questions/{questionId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.SecondExplorersQuiz.Id);
        stored.Questions.Should().Contain(question => question.Id == questionId);
    }

    [Fact]
    public async Task Anonymous_user_cannot_remove_a_question()
    {
        var client = Factory.CreateClient();
        var questionId = QuizSeed.FortressQuiz.Questions.Single().Id;

        var response = await client.DeleteAsync($"/api/games/quizzes/{QuizSeed.FortressQuiz.Id}/questions/{questionId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_remove_a_question()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var questionId = QuizSeed.FortressQuiz.Questions.Single().Id;

        var response = await client.DeleteAsync($"/api/games/quizzes/{QuizSeed.FortressQuiz.Id}/questions/{questionId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
