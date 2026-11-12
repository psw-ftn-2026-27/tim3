namespace Games.Application.QuizAuthoring;

public sealed record UpdateQuestionDto(string Text, List<AnswerDto> Answers);
