using Games.Domain.Quizzes;

namespace Games.Application.QuizAuthoring;

public sealed record QuizDto(
    Guid Id,
    Guid AuthorId,
    string Title,
    string Description,
    int ExperiencePoints,
    QuizStatus Status,
    List<QuestionDto> Questions);
