namespace Games.Application.QuizPlaying;

// The question to answer next. Answers are only their texts, so the correct one is not revealed;
// the explorer picks one by its index. Position is the question's place in the pass, from 1.
public sealed record PlayQuestionDto(Guid Id, int Position, string Text, List<string> Answers);
