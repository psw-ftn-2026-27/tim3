using Games.Application.QuizAuthoring;
using Shared.Domain;

namespace Games.Application.Quizzes;

public interface IQuizReadRepository
{
    Task<QuizDto?> GetByIdAsync(Guid id);
    Task<PageResult<QuizDto>> GetAllAsync(int page, int pageSize);
}
