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

    private static Quiz CreateQuizWithThreeQuestions(string description)
    {
        var quiz = new Quiz(WellKnownUsers.Administrator, "Petrovaradinska tvrđava", description, 50);
        quiz.AddQuestion("Koliko je stara Sahat kula?", [new Answer("Oko 300 godina", true), new Answer("Oko 50 godina", false)]);
        quiz.AddQuestion("Na kojoj reci je tvrđava?", [new Answer("Dunav", true), new Answer("Sava", false)]);
        quiz.AddQuestion("Kako se zove tvrđavski festival?", [new Answer("Exit", true), new Answer("Zmajevi", false)]);
        return quiz;
    }

    private static Quiz CreatePublishedQuiz()
    {
        var quiz = CreateQuizWithThreeQuestions("Pitanja o tvrđavi.");
        quiz.Publish();
        return quiz;
    }

    private static Quiz CreateRetiredQuiz()
    {
        var quiz = CreatePublishedQuiz();
        quiz.Retire();
        return quiz;
    }

    private static Quiz CreateArchivedQuiz()
    {
        var quiz = CreateRetiredQuiz();
        quiz.Archive();
        return quiz;
    }

    private static Quiz CreateQuizIn(QuizStatus status) => status switch
    {
        QuizStatus.Draft => CreateQuizWithThreeQuestions("Pitanja o tvrđavi."),
        QuizStatus.Published => CreatePublishedQuiz(),
        QuizStatus.Retired => CreateRetiredQuiz(),
        QuizStatus.Archived => CreateArchivedQuiz(),
        _ => throw new ArgumentOutOfRangeException(nameof(status))
    };

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
    public void Question_with_fewer_than_two_answers_is_rejected()
    {
        var quiz = CreateQuiz();

        var addition = () => quiz.AddQuestion("Koji je tačan?", [new Answer("Jedini", true)]);

        addition.Should().Throw<DomainException>();
        quiz.Questions.Should().BeEmpty();
    }

    [Fact]
    public void Added_question_without_a_correct_answer_is_rejected()
    {
        var quiz = CreateQuiz();

        var addition = () => quiz.AddQuestion("Koji je tačan?", [new Answer("Prvi", false), new Answer("Drugi", false)]);

        addition.Should().Throw<DomainException>();
        quiz.Questions.Should().BeEmpty();
    }

    [Fact]
    public void Edited_question_without_a_correct_answer_is_rejected()
    {
        var quiz = CreateQuizWithQuestion();
        var questionId = quiz.Questions.Single().Id;

        var update = () => quiz.UpdateQuestion(questionId, "Koji je tačan?", [new Answer("Prvi", false), new Answer("Drugi", false)]);

        update.Should().Throw<DomainException>();
        quiz.Questions.Single().Answers.Should().Contain(answer => answer.IsCorrect);
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

    [Fact]
    public void Quiz_with_three_questions_and_instructions_is_published()
    {
        var quiz = CreateQuizWithThreeQuestions("Pitanja o tvrđavi.");

        quiz.Publish();

        quiz.Status.Should().Be(QuizStatus.Published);
    }

    [Fact]
    public void Quiz_with_two_questions_is_not_published()
    {
        var quiz = CreateQuizWithThreeQuestions("Pitanja o tvrđavi.");
        quiz.RemoveQuestion(quiz.Questions[0].Id);

        var publication = () => quiz.Publish();

        publication.Should().Throw<DomainException>();
        quiz.Status.Should().Be(QuizStatus.Draft);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Quiz_without_instructions_is_not_published(string description)
    {
        var quiz = CreateQuizWithThreeQuestions(description);

        var publication = () => quiz.Publish();

        publication.Should().Throw<DomainException>();
        quiz.Status.Should().Be(QuizStatus.Draft);
    }

    [Fact]
    public void Published_quiz_cannot_be_published_again()
    {
        var quiz = CreatePublishedQuiz();

        var publication = () => quiz.Publish();

        publication.Should().Throw<DomainException>();
    }

    [Fact]
    public void Details_of_a_published_quiz_cannot_be_changed()
    {
        var quiz = CreatePublishedQuiz();

        var update = () => quiz.UpdateDetails("Nov naziv", "Nov opis.", 40);

        update.Should().Throw<DomainException>();
        quiz.Title.Should().Be("Petrovaradinska tvrđava");
    }

    [Fact]
    public void Question_cannot_be_added_to_a_published_quiz()
    {
        var quiz = CreatePublishedQuiz();

        var addition = () => quiz.AddQuestion("Novo pitanje?", [new Answer("Da", true), new Answer("Ne", false)]);

        addition.Should().Throw<DomainException>();
        quiz.Questions.Should().HaveCount(3);
    }

    [Fact]
    public void Question_of_a_published_quiz_cannot_be_edited()
    {
        var quiz = CreatePublishedQuiz();
        var question = quiz.Questions[0];

        var update = () => quiz.UpdateQuestion(question.Id, "Izmenjeno pitanje?", [new Answer("Da", true), new Answer("Ne", false)]);

        update.Should().Throw<DomainException>();
        question.Text.Should().Be("Koliko je stara Sahat kula?");
    }

    [Fact]
    public void Question_cannot_be_removed_from_a_published_quiz()
    {
        var quiz = CreatePublishedQuiz();
        var questionId = quiz.Questions[0].Id;

        var removal = () => quiz.RemoveQuestion(questionId);

        removal.Should().Throw<DomainException>();
        quiz.Questions.Should().Contain(question => question.Id == questionId);
    }

    [Fact]
    public void Published_quiz_is_retired()
    {
        var quiz = CreatePublishedQuiz();

        quiz.Retire();

        quiz.Status.Should().Be(QuizStatus.Retired);
    }

    [Theory]
    [InlineData(QuizStatus.Draft)]
    [InlineData(QuizStatus.Retired)]
    [InlineData(QuizStatus.Archived)]
    public void Quiz_that_is_not_published_cannot_be_retired(QuizStatus status)
    {
        var quiz = CreateQuizIn(status);

        var retirement = () => quiz.Retire();

        retirement.Should().Throw<DomainException>();
        quiz.Status.Should().Be(status);
    }

    [Fact]
    public void Retired_quiz_is_archived()
    {
        var quiz = CreateRetiredQuiz();

        quiz.Archive();

        quiz.Status.Should().Be(QuizStatus.Archived);
    }

    [Theory]
    [InlineData(QuizStatus.Draft)]
    [InlineData(QuizStatus.Published)]
    [InlineData(QuizStatus.Archived)]
    public void Quiz_that_is_not_retired_cannot_be_archived(QuizStatus status)
    {
        var quiz = CreateQuizIn(status);

        var archiving = () => quiz.Archive();

        archiving.Should().Throw<DomainException>();
        quiz.Status.Should().Be(status);
    }

    [Fact]
    public void Retired_quiz_is_published_again()
    {
        var quiz = CreateRetiredQuiz();

        quiz.Publish();

        quiz.Status.Should().Be(QuizStatus.Published);
    }

    [Fact]
    public void Archived_quiz_cannot_be_published()
    {
        var quiz = CreateArchivedQuiz();

        var publication = () => quiz.Publish();

        publication.Should().Throw<DomainException>();
        quiz.Status.Should().Be(QuizStatus.Archived);
    }
}
