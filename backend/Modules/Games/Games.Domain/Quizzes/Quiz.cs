using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.Quizzes;

public sealed class Quiz : AggregateRoot
{
    public const int MinExperiencePoints = 1;
    public const int MaxExperiencePoints = 5000;

    private readonly List<Question> _questions = [];

    public Guid AuthorId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public int ExperiencePoints { get; private set; }
    public QuizStatus Status { get; private set; }
    public IReadOnlyList<Question> Questions => _questions;

    private Quiz()
    {
        Title = null!;
        Description = null!;
    }

    public Quiz(Guid authorId, string title, string description, int experiencePoints)
        : base(Guid.NewGuid())
    {
        EnsureTitle(title);
        EnsureExperiencePoints(experiencePoints);

        AuthorId = authorId;
        Title = title;
        Description = description;
        ExperiencePoints = experiencePoints;
        Status = QuizStatus.Draft;
    }

    public void UpdateDetails(string title, string description, int experiencePoints)
    {
        EnsureDraft();
        EnsureTitle(title);
        EnsureExperiencePoints(experiencePoints);

        Title = title;
        Description = description;
        ExperiencePoints = experiencePoints;
    }

    public void AddQuestion(string text, List<Answer> answers)
    {
        EnsureDraft();
        _questions.Add(new Question(text, answers));
    }

    public void UpdateQuestion(Guid questionId, string text, List<Answer> answers)
    {
        EnsureDraft();
        GetQuestion(questionId).Update(text, answers);
    }

    public void RemoveQuestion(Guid questionId)
    {
        EnsureDraft();
        _questions.Remove(GetQuestion(questionId));
    }

    private Question GetQuestion(Guid questionId)
    {
        var question = _questions.FirstOrDefault(question => question.Id == questionId);
        if (question is null)
        {
            throw new NotFoundException($"Question {questionId} does not exist.");
        }
        return question;
    }

    private void EnsureDraft()
    {
        if (Status != QuizStatus.Draft)
        {
            throw new DomainException("A quiz can be changed only while it is a draft.");
        }
    }

    private static void EnsureTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("A quiz requires a title.");
        }
    }

    private static void EnsureExperiencePoints(int experiencePoints)
    {
        if (experiencePoints < MinExperiencePoints || experiencePoints > MaxExperiencePoints)
        {
            throw new DomainException($"A quiz must award between {MinExperiencePoints} and {MaxExperiencePoints} experience points.");
        }
    }
}
