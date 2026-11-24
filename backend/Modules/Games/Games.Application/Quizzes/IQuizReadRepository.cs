using Games.Application.QuizAuthoring;
using Games.Application.QuizPlaying;
using Shared.Domain;

namespace Games.Application.Quizzes;

public interface IQuizReadRepository
{
    Task<QuizDto?> GetByIdAsync(Guid id);
    Task<PageResult<QuizDto>> GetAllAsync(int page, int pageSize);
    Task<PageResult<PublishedQuizDto>> GetPublishedAsync(Guid explorerId, int page, int pageSize);
}
