using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.QuizParticipations;

// What one explorer does in one quiz. The quiz itself is referred to by ID only.
public sealed class QuizParticipation : AggregateRoot
{
    private readonly List<Guid> _passQuestionIds = [];
    private readonly List<GivenAnswer> _answers = [];

    public Guid QuizId { get; private set; }
    public Guid ExplorerId { get; private set; }
    public int CurrentPass { get; private set; }
    public DateTime? SolvedAt { get; private set; }
    public int? ExperienceGained { get; private set; }
    public IReadOnlyList<Guid> PassQuestionIds => _passQuestionIds.AsReadOnly();
    public IReadOnlyList<GivenAnswer> Answers => _answers;

    private QuizParticipation() { }

    public QuizParticipation(Guid quizId, Guid explorerId) : base(Guid.NewGuid())
    {
        QuizId = quizId;
        ExplorerId = explorerId;
    }

    // An interrupted pass is resumed rather than restarted, so it does not count as finished.
    public void StartPass(IReadOnlyList<Guid> questionIds)
    {
        if (questionIds.All(IsAnsweredCorrectly))
        {
            throw new DomainException("You've already solved this quiz.");
        }
        if (NextQuestionId() is not null)
        {
            return;
        }

        CurrentPass++;
        _passQuestionIds.Clear();
        _passQuestionIds.AddRange(questionIds);
    }

    public void Record(Guid questionId, int answerIndex, bool isCorrect, DateTime answeredAt)
    {
        var nextQuestionId = NextQuestionId();
        if (nextQuestionId is null)
        {
            throw new DomainException("No pass is in progress. Start the quiz first.");
        }
        if (nextQuestionId != questionId)
        {
            throw new DomainException("Answer the questions in order, each once per pass.");
        }

        _answers.Add(new GivenAnswer(questionId, answerIndex, isCorrect, CurrentPass, answeredAt));
    }

    public void Solve(DateTime solvedAt, int experienceGained)
    {
        if (SolvedAt is not null)
        {
            throw new DomainException("The quiz is already solved.");
        }
        if (CurrentPass != 1 || NextQuestionId() is not null)
        {
            throw new DomainException("A quiz is solved only at the end of the first pass.");
        }
        if (experienceGained < 0)
        {
            throw new DomainException("Experience gained cannot be negative.");
        }

        SolvedAt = solvedAt;
        ExperienceGained = experienceGained;
    }

    // The next question of the current pass: not yet answered in this pass and not answered
    // correctly before, because a correct answer is locked (P7). Null when no pass is in progress.
    public Guid? NextQuestionId()
    {
        foreach (var questionId in _passQuestionIds)
        {
            if (!IsAnsweredInCurrentPass(questionId) && !IsAnsweredCorrectly(questionId))
            {
                return questionId;
            }
        }
        return null;
    }

    public int CorrectAnswerCount() => _answers.Count(answer => answer.IsCorrect);

    // The correct answers given in earlier passes, which the current pass shows as locked.
    public IReadOnlyList<GivenAnswer> LockedAnswers() =>
        _answers.Where(answer => answer.IsCorrect && answer.Pass < CurrentPass).ToList();

    public GivenAnswer? LastAnswerInCurrentPass() =>
        _answers.LastOrDefault(answer => answer.Pass == CurrentPass);

    private bool IsAnsweredInCurrentPass(Guid questionId) =>
        _answers.Any(answer => answer.QuestionId == questionId && answer.Pass == CurrentPass);

    private bool IsAnsweredCorrectly(Guid questionId) =>
        _answers.Any(answer => answer.QuestionId == questionId && answer.IsCorrect);
}
