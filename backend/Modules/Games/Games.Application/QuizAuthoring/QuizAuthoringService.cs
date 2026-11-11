using Games.Application.Quizzes;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizAuthoring;

public sealed class QuizAuthoringService
{
    private readonly IQuizRepository _quizRepository;
    private readonly IUnitOfWork _unitOfWork;

    public QuizAuthoringService(IQuizRepository quizRepository, IUnitOfWork unitOfWork)
    {
        _quizRepository = quizRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateAsync(Guid authorId, CreateQuizDto dto)
    {
        var quiz = new Quiz(authorId, dto.Title, dto.Description, ToLocation(dto.Location));

        _quizRepository.Add(quiz);
        await _unitOfWork.SaveChangesAsync();

        return quiz.Id;
    }

    public async Task UpdateDetailsAsync(Guid quizId, Guid authorId, UpdateQuizDetailsDto dto)
    {
        var quiz = await GetOwnedQuizAsync(quizId, authorId);

        quiz.UpdateDetails(dto.Title, dto.Description, ToLocation(dto.Location));
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AddQuestionAsync(Guid quizId, Guid authorId, CreateQuestionDto dto)
    {
        var quiz = await GetOwnedQuizAsync(quizId, authorId);

        quiz.AddQuestion(dto.Text, ToAnswers(dto.Answers));
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateQuestionAsync(Guid quizId, Guid questionId, Guid authorId, UpdateQuestionDto dto)
    {
        var quiz = await GetOwnedQuizAsync(quizId, authorId);

        quiz.UpdateQuestion(questionId, dto.Text, ToAnswers(dto.Answers));
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveQuestionAsync(Guid quizId, Guid questionId, Guid authorId)
    {
        var quiz = await GetOwnedQuizAsync(quizId, authorId);

        quiz.RemoveQuestion(questionId);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Quiz> GetOwnedQuizAsync(Guid quizId, Guid authorId)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId);
        if (quiz is null || quiz.AuthorId != authorId)
        {
            throw new NotFoundException($"Quiz {quizId} does not exist.");
        }
        return quiz;
    }

    private static Location? ToLocation(LocationDto? dto)
    {
        return dto is null ? null : new Location(dto.Latitude, dto.Longitude);
    }

    private static List<Answer> ToAnswers(List<AnswerDto> dtos)
    {
        return dtos.Select(dto => new Answer(dto.Text, dto.IsCorrect)).ToList();
    }
}
