using Games.Application.Quizzes;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizLifecycle;

public sealed class QuizLifecycleService
{
    private readonly IQuizRepository _quizRepository;
    private readonly IUnitOfWork _unitOfWork;

    public QuizLifecycleService(IQuizRepository quizRepository, IUnitOfWork unitOfWork)
    {
        _quizRepository = quizRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task PublishAsync(Guid quizId)
    {
        var quiz = await GetQuizAsync(quizId);

        quiz.Publish();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RetireAsync(Guid quizId)
    {
        var quiz = await GetQuizAsync(quizId);

        quiz.Retire();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ArchiveAsync(Guid quizId)
    {
        var quiz = await GetQuizAsync(quizId);

        quiz.Archive();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid quizId)
    {
        var quiz = await GetQuizAsync(quizId);

        quiz.EnsureCanBeDeleted();
        _quizRepository.Remove(quiz);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Quiz> GetQuizAsync(Guid quizId)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId);
        if (quiz is null)
        {
            throw new NotFoundException($"Quiz {quizId} does not exist.");
        }
        return quiz;
    }
}
