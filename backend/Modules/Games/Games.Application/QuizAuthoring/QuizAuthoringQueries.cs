using Games.Application.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizAuthoring;

public sealed class QuizAuthoringQueries
{
    private readonly IQuizReadRepository _quizReadRepository;

    public QuizAuthoringQueries(IQuizReadRepository quizReadRepository)
    {
        _quizReadRepository = quizReadRepository;
    }

    public Task<List<QuizDto>> GetByAuthorAsync(Guid authorId)
    {
        return _quizReadRepository.GetByAuthorAsync(authorId);
    }

    public async Task<QuizDto> GetByIdAsync(Guid quizId, Guid authorId)
    {
        var quiz = await _quizReadRepository.GetByIdAsync(quizId);
        if (quiz is null || quiz.AuthorId != authorId)
        {
            throw new NotFoundException($"Quiz {quizId} does not exist.");
        }

        return quiz;
    }
}
