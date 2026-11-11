using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.Quizzes;

public sealed class Question : Entity
{
    private const int MaxAnswers = 4;

    private readonly List<Answer> _answers = [];

    public string Text { get; private set; }
    public IReadOnlyList<Answer> Answers => _answers;

    private Question()
    {
        Text = null!;
    }

    public Question(string text, List<Answer> answers) : base(Guid.NewGuid())
    {
        EnsureValidAnswers(answers);

        Text = text;
        _answers.AddRange(answers);
    }

    public void Update(string text, List<Answer> answers)
    {
        EnsureValidAnswers(answers);

        Text = text;
        _answers.Clear();
        _answers.AddRange(answers);
    }

    private static void EnsureValidAnswers(List<Answer> answers)
    {
        if (answers.Count > MaxAnswers)
        {
            throw new DomainException($"A question has at most {MaxAnswers} answers.");
        }
        if (answers.Count(answer => answer.IsCorrect) > 1)
        {
            throw new DomainException("A question has at most one correct answer.");
        }
    }
}
