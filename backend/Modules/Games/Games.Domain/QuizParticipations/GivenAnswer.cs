namespace Games.Domain.QuizParticipations;

// One answer an explorer gave, kept from every pass (P9). The chosen answer is its position
// among the question's answers, which cannot change once the quiz is published.
public sealed record GivenAnswer
{
    public Guid QuestionId { get; private init; }
    public int AnswerIndex { get; private init; }
    public bool IsCorrect { get; private init; }
    public int Pass { get; private init; }
    public DateTime AnsweredAt { get; private init; }

    private GivenAnswer() { }

    internal GivenAnswer(Guid questionId, int answerIndex, bool isCorrect, int pass, DateTime answeredAt)
    {
        QuestionId = questionId;
        AnswerIndex = answerIndex;
        IsCorrect = isCorrect;
        Pass = pass;
        AnsweredAt = answeredAt;
    }
}
