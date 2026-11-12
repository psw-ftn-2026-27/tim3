namespace Games.Application.QuizAuthoring;

public sealed record CreateQuestionDto(string Text, List<AnswerDto> Answers);
