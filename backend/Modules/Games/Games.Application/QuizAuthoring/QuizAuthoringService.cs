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
        var quiz = new Quiz(authorId, dto.Title, dto.Description, dto.ExperiencePoints);

        _quizRepository.Add(quiz);
        await _unitOfWork.SaveChangesAsync();

        return quiz.Id;
    }

    public async Task UpdateDetailsAsync(Guid quizId, UpdateQuizDetailsDto dto)
    {
        var quiz = await GetQuizAsync(quizId);

        quiz.UpdateDetails(dto.Title, dto.Description, dto.ExperiencePoints);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AddQuestionAsync(Guid quizId, CreateQuestionDto dto)
    {
        var quiz = await GetQuizAsync(quizId);

        quiz.AddQuestion(dto.Text, ToAnswers(dto.Answers));
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateQuestionAsync(Guid quizId, Guid questionId, UpdateQuestionDto dto)
    {
        var quiz = await GetQuizAsync(quizId);

        quiz.UpdateQuestion(questionId, dto.Text, ToAnswers(dto.Answers));
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveQuestionAsync(Guid quizId, Guid questionId)
    {
        var quiz = await GetQuizAsync(quizId);

        quiz.RemoveQuestion(questionId);
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

    private static List<Answer> ToAnswers(List<AnswerDto> dtos)
    {
        return dtos.Select(dto => new Answer(dto.Text, dto.IsCorrect)).ToList();
    }
}
