namespace Games.Application.QuizAuthoring;

public sealed record CreateQuizDto(string Title, string Description, LocationDto? Location);
