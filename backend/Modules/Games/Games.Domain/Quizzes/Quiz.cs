using Games.Domain.Locations;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.Quizzes;

public sealed class Quiz : AggregateRoot
{
    public const int MinExperiencePoints = 1;
    public const int MaxExperiencePoints = 5000;
    public const int MinQuestionsToPublish = 3;

    private readonly List<Question> _questions = [];

    public Guid AuthorId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public int ExperiencePoints { get; private set; }
    public QuizStatus Status { get; private set; }
    public Location? Location { get; private set; }
    public IReadOnlyList<Question> Questions => _questions;

    private Quiz()
    {
        Title = null!;
        Description = null!;
    }

    public Quiz(Guid authorId, string title, string description, int experiencePoints)
        : base(Guid.NewGuid())
    {
        EnsureValidDetails(title, experiencePoints);

        AuthorId = authorId;
        Title = title;
        Description = description;
        ExperiencePoints = experiencePoints;
        Status = QuizStatus.Draft;
    }

    public void UpdateDetails(string title, string description, int experiencePoints, Location? location)
    {
        EnsureDraft();
        EnsureValidDetails(title, experiencePoints);

        Title = title;
        Description = description;
        ExperiencePoints = experiencePoints;
        Location = location;
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

    public void Publish()
    {
        if (_questions.Count < MinQuestionsToPublish)
        {
            throw new DomainException($"A quiz requires at least {MinQuestionsToPublish} questions to be published.");
        }
        if (string.IsNullOrWhiteSpace(Description))
        {
            throw new DomainException("A quiz requires instructions to be published.");
        }
        if (Location is null)
        {
            throw new DomainException("A quiz requires a location to be published.");
        }

        TransitionTo(QuizStatus.Published,
            "Only a draft or retired quiz can be published.",
            QuizStatus.Draft, QuizStatus.Retired);
    }

    public void Retire() => TransitionTo(QuizStatus.Retired,
        "Only a published quiz can be retired.",
        QuizStatus.Published);

    public void Archive() => TransitionTo(QuizStatus.Archived,
        "Only a retired quiz can be archived.",
        QuizStatus.Retired);

    public void EnsureCanBeDeleted()
    {
        if (Status != QuizStatus.Draft && Status != QuizStatus.Archived)
        {
            throw new DomainException("Only a draft or archived quiz can be deleted.");
        }
    }

    private void TransitionTo(QuizStatus target, string errorMessage, params QuizStatus[] allowedFrom)
    {
        if (!allowedFrom.Contains(Status))
        {
            throw new DomainException(errorMessage);
        }

        Status = target;
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

    // The details a quiz is created and updated with: a title and the experience points it awards.
    private static void EnsureValidDetails(string title, int experiencePoints)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("A quiz requires a title.");
        }
        if (experiencePoints < MinExperiencePoints || experiencePoints > MaxExperiencePoints)
        {
            throw new DomainException($"A quiz must award between {MinExperiencePoints} and {MaxExperiencePoints} experience points.");
        }
    }
}
