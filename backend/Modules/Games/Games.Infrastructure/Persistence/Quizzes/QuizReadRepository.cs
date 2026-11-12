using Games.Application.QuizAuthoring;
using Games.Application.Quizzes;
using Games.Domain.Quizzes;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Games.Infrastructure.Persistence.Quizzes;

internal sealed class QuizReadRepository : IQuizReadRepository
{
    private readonly GamesDbContext _dbContext;

    public QuizReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<QuizDto?> GetByIdAsync(Guid id)
    {
        return ProjectToDtos(_dbContext.Quizzes.Where(quiz => quiz.Id == id)).FirstOrDefaultAsync();
    }

    public async Task<PageResult<QuizDto>> GetAllAsync(int page, int pageSize)
    {
        var quizzes = _dbContext.Quizzes
            .OrderBy(quiz => quiz.Title)
            .ThenBy(quiz => quiz.Id);

        var totalCount = await quizzes.CountAsync();
        var items = await ProjectToDtos(quizzes)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<QuizDto>(items, totalCount);
    }

    private static IQueryable<QuizDto> ProjectToDtos(IQueryable<Quiz> quizzes)
    {
        return quizzes
            .Select(quiz => new QuizDto(
                quiz.Id,
                quiz.AuthorId,
                quiz.Title,
                quiz.Description,
                quiz.ExperiencePoints,
                quiz.Status,
                quiz.Questions
                    .Select(question => new QuestionDto(
                        question.Id,
                        question.Text,
                        question.Answers
                            .Select(answer => new AnswerDto(answer.Text, answer.IsCorrect))
                            .ToList()))
                    .ToList()));
    }
}
