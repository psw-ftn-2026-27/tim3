using Games.Application.QuizParticipations;
using Games.Application.Quizzes;
using Games.Domain.QuizParticipations;
using Games.Domain.Quizzes;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizPlaying;

public sealed class QuizPlayingQueries
{
    private readonly IQuizReadRepository _quizReadRepository;
    private readonly IQuizRepository _quizRepository;
    private readonly IQuizParticipationRepository _participationRepository;

    public QuizPlayingQueries(
        IQuizReadRepository quizReadRepository,
        IQuizRepository quizRepository,
        IQuizParticipationRepository participationRepository)
    {
        _quizReadRepository = quizReadRepository;
        _quizRepository = quizRepository;
        _participationRepository = participationRepository;
    }

    public Task<PageResult<PublishedQuizDto>> GetPublishedAsync(Guid explorerId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _quizReadRepository.GetPublishedAsync(explorerId, page, pageSize);
    }

    // The next question and the locked ones are derived by the participation, so the quiz and the
    // participation are loaded as aggregates instead of repeating that derivation in SQL.
    public async Task<QuizPlayDto> GetPlayAsync(Guid quizId, Guid explorerId)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId)
            ?? throw new NotFoundException($"Quiz {quizId} does not exist.");
        var participation = await _participationRepository.GetAsync(quizId, explorerId)
            ?? throw new NotFoundException($"You have not started quiz {quizId}.");

        var nextQuestionId = participation.NextQuestionId();
        var currentQuestion = nextQuestionId is null
            ? null
            : ToPlayQuestion(quiz, participation, nextQuestionId.Value);
        var lockedQuestions = participation.LockedAnswers()
            .Select(answer => ToLockedQuestion(quiz, participation, answer))
            .OrderBy(question => question.Position)
            .ToList();

        return new QuizPlayDto(
            quiz.Id,
            quiz.Title,
            participation.CurrentPass,
            participation.PassQuestionIds.Count,
            currentQuestion,
            participation.LastAnswerInCurrentPass()?.IsCorrect,
            lockedQuestions,
            participation.CorrectAnswerCount(),
            participation.ExperienceGained);
    }

    private static PlayQuestionDto ToPlayQuestion(Quiz quiz, QuizParticipation participation, Guid questionId)
    {
        var question = quiz.Questions.Single(question => question.Id == questionId);
        return new PlayQuestionDto(
            question.Id, PositionOf(participation, question.Id), question.Text, AnswerTexts(question));
    }

    private static LockedQuestionDto ToLockedQuestion(Quiz quiz, QuizParticipation participation, GivenAnswer answer)
    {
        var question = quiz.Questions.Single(question => question.Id == answer.QuestionId);
        return new LockedQuestionDto(
            question.Id, PositionOf(participation, question.Id), question.Text, AnswerTexts(question), answer.AnswerIndex);
    }

    private static int PositionOf(QuizParticipation participation, Guid questionId) =>
        participation.PassQuestionIds.ToList().IndexOf(questionId) + 1;

    private static List<string> AnswerTexts(Question question) =>
        question.Answers.Select(answer => answer.Text).ToList();
}
