using Games.Application.QuizAuthoring;
using Games.Application.Quizzes;
using Games.Domain.Quizzes;
using Microsoft.EntityFrameworkCore;

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

    public Task<List<QuizDto>> GetByAuthorAsync(Guid authorId)
    {
        return ProjectToDtos(_dbContext.Quizzes.Where(quiz => quiz.AuthorId == authorId)).ToListAsync();
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
