using Games.Application.Quizzes;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizAuthoring;

public sealed class QuizAuthoringQueries
{
    private readonly IQuizReadRepository _quizReadRepository;

    public QuizAuthoringQueries(IQuizReadRepository quizReadRepository)
    {
        _quizReadRepository = quizReadRepository;
    }

    public Task<PageResult<QuizDto>> GetAllAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _quizReadRepository.GetAllAsync(page, pageSize);
    }

    public async Task<QuizDto> GetByIdAsync(Guid quizId)
    {
        var quiz = await _quizReadRepository.GetByIdAsync(quizId);
        if (quiz is null)
        {
            throw new NotFoundException($"Quiz {quizId} does not exist.");
        }

        return quiz;
    }
}
