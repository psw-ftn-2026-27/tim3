namespace Games.Application.QuizAuthoring;

// The question the administrator sends when adding a question or changing one; the question's
// id comes from the route, so it is not part of the body.
public sealed record QuestionInputDto(string Text, List<AnswerDto> Answers);
