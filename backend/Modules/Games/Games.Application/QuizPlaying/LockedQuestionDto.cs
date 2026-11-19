namespace Games.Application.QuizPlaying;

// A question answered correctly in an earlier pass, shown locked with the explorer's correct answer.
public sealed record LockedQuestionDto(Guid Id, int Position, string Text, List<string> Answers, int ChosenAnswerIndex);
