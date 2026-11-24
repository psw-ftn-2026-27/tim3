using FluentAssertions;
using Games.Domain.Locations;
using Games.Domain.QuizParticipations;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Unit.QuizParticipations;

public class QuizPlayTests
{
    private static readonly Location QuizLocation = new(45.2517, 19.8616);
    private static readonly DateTime Now = new(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc);
    private const double MetresPerDegreeOfLatitude = 6_371_000 * Math.PI / 180;

    private readonly QuizPlay _quizPlay = new();

    private static Quiz CreateDraftQuiz()
    {
        var quiz = new Quiz(WellKnownUsers.Administrator, "Petrovaradinska tvrđava", "Pitanja o tvrđavi.", 50);
        quiz.AddQuestion("Koliko je stara Sahat kula?", [new Answer("Oko 300 godina", true), new Answer("Oko 50 godina", false)]);
        quiz.AddQuestion("Na kojoj reci je tvrđava?", [new Answer("Dunav", true), new Answer("Sava", false)]);
        quiz.AddQuestion("Kako se zove tvrđavski festival?", [new Answer("Exit", true), new Answer("Zmajevi", false)]);
        quiz.UpdateDetails("Petrovaradinska tvrđava", "Pitanja o tvrđavi.", 50, QuizLocation);
        return quiz;
    }

    private static Quiz CreatePublishedQuiz()
    {
        var quiz = CreateDraftQuiz();
        quiz.Publish();
        return quiz;
    }

    // A position due north of the quiz, the given number of metres away.
    private static Location PositionNorthOfQuiz(double metres) =>
        new(QuizLocation.Latitude + metres / MetresPerDegreeOfLatitude, QuizLocation.Longitude);

    private QuizParticipation CreateActivatedParticipation(Quiz quiz)
    {
        var participation = new QuizParticipation(quiz.Id, WellKnownUsers.Explorer);
        _quizPlay.Activate(quiz, participation, QuizLocation, Now, Now);
        return participation;
    }

    [Fact]
    public void Explorer_at_the_maximum_distance_starts_the_quiz()
    {
        var quiz = CreatePublishedQuiz();
        var participation = new QuizParticipation(quiz.Id, WellKnownUsers.Explorer);

        _quizPlay.Activate(quiz, participation, PositionNorthOfQuiz(QuizPlay.MaxActivationDistanceInMetres), Now, Now);

        participation.NextQuestionId().Should().Be(quiz.Questions[0].Id);
    }

    [Fact]
    public void Explorer_beyond_the_maximum_distance_cannot_start_the_quiz()
    {
        var quiz = CreatePublishedQuiz();
        var participation = new QuizParticipation(quiz.Id, WellKnownUsers.Explorer);

        var activation = () => _quizPlay.Activate(quiz, participation,
            PositionNorthOfQuiz(QuizPlay.MaxActivationDistanceInMetres + 0.1), Now, Now);

        activation.Should().Throw<DomainException>();
        participation.CurrentPass.Should().Be(0);
    }

    [Fact]
    public void Unpublished_quiz_cannot_be_started()
    {
        var quiz = CreateDraftQuiz();
        var participation = new QuizParticipation(quiz.Id, WellKnownUsers.Explorer);

        var activation = () => _quizPlay.Activate(quiz, participation, QuizLocation, Now, Now);

        activation.Should().Throw<DomainException>();
        participation.CurrentPass.Should().Be(0);
    }

    [Fact]
    public void Position_exactly_as_old_as_allowed_is_enough_to_start_the_quiz()
    {
        var quiz = CreatePublishedQuiz();
        var participation = new QuizParticipation(quiz.Id, WellKnownUsers.Explorer);

        _quizPlay.Activate(quiz, participation, QuizLocation, Now - QuizPlay.MaxPositionAge, Now);

        participation.CurrentPass.Should().Be(1);
    }

    [Fact]
    public void Position_older_than_allowed_cannot_start_the_quiz()
    {
        var quiz = CreatePublishedQuiz();
        var participation = new QuizParticipation(quiz.Id, WellKnownUsers.Explorer);

        var activation = () => _quizPlay.Activate(quiz, participation, QuizLocation,
            Now - QuizPlay.MaxPositionAge - TimeSpan.FromSeconds(1), Now);

        activation.Should().Throw<DomainException>();
        participation.CurrentPass.Should().Be(0);
    }

    [Fact]
    public void End_of_the_first_pass_solves_the_quiz_with_the_correct_share_of_experience_rounded_down()
    {
        var quiz = CreatePublishedQuiz();
        var participation = CreateActivatedParticipation(quiz);
        _quizPlay.Answer(quiz, participation, quiz.Questions[0].Id, 0, Now);
        _quizPlay.Answer(quiz, participation, quiz.Questions[1].Id, 1, Now);

        _quizPlay.Answer(quiz, participation, quiz.Questions[2].Id, 0, Now);

        participation.SolvedAt.Should().Be(Now);
        participation.ExperienceGained.Should().Be(33);
    }

    [Fact]
    public void Repeat_pass_records_answers_to_incorrect_questions_without_changing_experience_or_solve_time()
    {
        var quiz = CreatePublishedQuiz();
        var participation = CreateActivatedParticipation(quiz);
        _quizPlay.Answer(quiz, participation, quiz.Questions[0].Id, 0, Now);
        _quizPlay.Answer(quiz, participation, quiz.Questions[1].Id, 1, Now);
        _quizPlay.Answer(quiz, participation, quiz.Questions[2].Id, 1, Now);
        var later = Now.AddHours(1);
        _quizPlay.Activate(quiz, participation, QuizLocation, later, later);
        _quizPlay.Answer(quiz, participation, quiz.Questions[1].Id, 0, later);

        _quizPlay.Answer(quiz, participation, quiz.Questions[2].Id, 0, later);

        participation.Answers.Should().HaveCount(5);
        participation.NextQuestionId().Should().BeNull();
        participation.SolvedAt.Should().Be(Now);
        participation.ExperienceGained.Should().Be(16);
    }
}
