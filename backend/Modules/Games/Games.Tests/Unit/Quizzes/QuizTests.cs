using FluentAssertions;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Unit.Quizzes;

public class QuizTests
{
    private static Quiz CreateQuiz() =>
        new(WellKnownUsers.Administrator, "Petrovaradinska tvrđava", "Pitanja o tvrđavi.", 50);

    private static Quiz CreateQuizWithQuestion()
    {
        var quiz = CreateQuiz();
        quiz.AddQuestion("Koliko je stara Sahat kula?", [new Answer("Oko 300 godina", true), new Answer("Oko 50 godina", false)]);
        return quiz;
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_quiz_requires_a_title(string title)
    {
        var creation = () => new Quiz(WellKnownUsers.Administrator, title, "Pitanja o tvrđavi.", 50);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(5001)]
    public void New_quiz_requires_experience_points_from_one_to_five_thousand(int experiencePoints)
    {
        var creation = () => new Quiz(WellKnownUsers.Administrator, "Petrovaradinska tvrđava", "Pitanja o tvrđavi.", experiencePoints);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Edited_quiz_requires_a_title(string title)
    {
        var quiz = CreateQuiz();

        var update = () => quiz.UpdateDetails(title, "Nov opis.", 50);

        update.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(5001)]
    public void Edited_quiz_requires_experience_points_from_one_to_five_thousand(int experiencePoints)
    {
        var quiz = CreateQuiz();

        var update = () => quiz.UpdateDetails("Petrovaradinska tvrđava", "Nov opis.", experiencePoints);

        update.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Added_question_requires_text(string text)
    {
        var quiz = CreateQuiz();

        var addition = () => quiz.AddQuestion(text, [new Answer("Prvi", true), new Answer("Drugi", false)]);

        addition.Should().Throw<DomainException>();
        quiz.Questions.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Edited_question_requires_text(string text)
    {
        var quiz = CreateQuizWithQuestion();
        var questionId = quiz.Questions.Single().Id;

        var update = () => quiz.UpdateQuestion(questionId, text, [new Answer("Prvi", true), new Answer("Drugi", false)]);

        update.Should().Throw<DomainException>();
        quiz.Questions.Single().Text.Should().Be("Koliko je stara Sahat kula?");
    }

    [Fact]
    public void Question_with_more_than_four_answers_is_rejected()
    {
        var quiz = CreateQuiz();
        List<Answer> answers =
        [
            new("Prvi", true),
            new("Drugi", false),
            new("Treći", false),
            new("Četvrti", false),
            new("Peti", false)
        ];

        var addition = () => quiz.AddQuestion("Koji je tačan?", answers);

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void Added_question_with_two_correct_answers_is_rejected()
    {
        var quiz = CreateQuiz();

        var addition = () => quiz.AddQuestion("Koji je tačan?", [new Answer("Prvi", true), new Answer("Drugi", true)]);

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void Edited_question_with_two_correct_answers_is_rejected()
    {
        var quiz = CreateQuizWithQuestion();
        var questionId = quiz.Questions.Single().Id;

        var update = () => quiz.UpdateQuestion(questionId, "Koji je tačan?", [new Answer("Prvi", true), new Answer("Drugi", true)]);

        update.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Answer_requires_text(string text)
    {
        var quiz = CreateQuiz();

        var addition = () => quiz.AddQuestion("Koji je tačan?", [new Answer(text, true), new Answer("Drugi", false)]);

        addition.Should().Throw<DomainException>();
    }
}
