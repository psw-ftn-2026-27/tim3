using Games.Application.Quizzes;

namespace Games.Application.QuizAuthoring;

public sealed record UpdateQuizDetailsDto(string Title, string Description, int ExperiencePoints, LocationDto? Location);
