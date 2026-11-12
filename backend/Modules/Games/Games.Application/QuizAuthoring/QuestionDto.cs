namespace Games.Application.QuizAuthoring;

public sealed record QuestionDto(Guid Id, string Text, List<AnswerDto> Answers);
