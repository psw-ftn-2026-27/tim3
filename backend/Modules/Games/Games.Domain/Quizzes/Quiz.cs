using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.Quizzes;

public sealed class Quiz : AggregateRoot
{
    private readonly List<Question> _questions = [];

    public Guid AuthorId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public Location? Location { get; private set; }
    public QuizStatus Status { get; private set; }
    public IReadOnlyList<Question> Questions => _questions;

    private Quiz()
    {
        Title = null!;
        Description = null!;
    }

    public Quiz(Guid authorId, string title, string description, Location? location)
        : base(Guid.NewGuid())
    {
        EnsureTitle(title);

        AuthorId = authorId;
        Title = title;
        Description = description;
        Location = location;
        Status = QuizStatus.Draft;
    }

    public void UpdateDetails(string title, string description, Location? location)
    {
        EnsureTitle(title);

        Title = title;
        Description = description;
        Location = location;
    }

    public void AddQuestion(string text, List<Answer> answers)
    {
        _questions.Add(new Question(text, answers));
    }

    public void UpdateQuestion(Guid questionId, string text, List<Answer> answers)
    {
        GetQuestion(questionId).Update(text, answers);
    }

    public void RemoveQuestion(Guid questionId)
    {
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

    private static void EnsureTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("A quiz requires a title.");
        }
    }
}
