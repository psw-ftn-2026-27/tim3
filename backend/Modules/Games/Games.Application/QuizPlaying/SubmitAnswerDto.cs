namespace Games.Application.QuizPlaying;

public sealed record SubmitAnswerDto(Guid QuestionId, int AnswerIndex);
