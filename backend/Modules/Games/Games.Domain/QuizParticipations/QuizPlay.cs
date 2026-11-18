using Games.Domain.Locations;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Domain.QuizParticipations;

// Playing a quiz needs both the quiz (status, location, questions, XP) and the explorer's
// participation, so the rules that span the two live here.
public sealed class QuizPlay
{
    public const double MaxActivationDistanceInMetres = 20;
    public static readonly TimeSpan MaxPositionAge = TimeSpan.FromSeconds(30);

    public void Activate(Quiz quiz, QuizParticipation participation, Location position, DateTime positionSetAt, DateTime now)
    {
        EnsureSameQuiz(quiz, participation);
        if (quiz.Status != QuizStatus.Published || quiz.Location is null)
        {
            throw new DomainException("This quiz is not available.");
        }
        if (now - positionSetAt > MaxPositionAge)
        {
            throw new DomainException("Please update your location.");
        }
        if (position.DistanceTo(quiz.Location) > MaxActivationDistanceInMetres)
        {
            throw new DomainException("You're too far away.");
        }

        participation.StartPass(quiz.Questions.Select(question => question.Id).ToList());
    }

    public void Answer(Quiz quiz, QuizParticipation participation, Guid questionId, int answerIndex, DateTime now)
    {
        EnsureSameQuiz(quiz, participation);
        var question = quiz.Questions.FirstOrDefault(question => question.Id == questionId)
            ?? throw new NotFoundException($"Question {questionId} does not exist.");
        if (answerIndex < 0 || answerIndex >= question.Answers.Count)
        {
            throw new DomainException("The chosen answer does not exist.");
        }

        participation.Record(questionId, answerIndex, question.Answers[answerIndex].IsCorrect, now);

        // P4, P5: the end of the first pass solves the quiz with its XP times the share of correct
        // answers, rounded down. A later pass finds the quiz already solved, so it awards nothing (P6).
        if (participation.SolvedAt is null && participation.NextQuestionId() is null)
        {
            participation.Solve(now, quiz.ExperiencePoints * participation.CorrectAnswerCount() / quiz.Questions.Count);
        }
    }

    private static void EnsureSameQuiz(Quiz quiz, QuizParticipation participation)
    {
        if (participation.QuizId != quiz.Id)
        {
            throw new DomainException("The participation belongs to another quiz.");
        }
    }
}
