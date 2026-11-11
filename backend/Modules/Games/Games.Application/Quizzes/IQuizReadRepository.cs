using Games.Application.QuizAuthoring;

namespace Games.Application.Quizzes;

public interface IQuizReadRepository
{
    Task<QuizDto?> GetByIdAsync(Guid id);
    Task<List<QuizDto>> GetByAuthorAsync(Guid authorId);
}
