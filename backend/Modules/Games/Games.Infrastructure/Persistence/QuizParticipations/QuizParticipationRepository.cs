using Games.Application.QuizParticipations;
using Games.Domain.QuizParticipations;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.QuizParticipations;

internal sealed class QuizParticipationRepository : IQuizParticipationRepository
{
    private readonly GamesDbContext _dbContext;

    public QuizParticipationRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<QuizParticipation?> GetAsync(Guid quizId, Guid explorerId)
    {
        return _dbContext.QuizParticipations.FirstOrDefaultAsync(participation =>
            participation.QuizId == quizId && participation.ExplorerId == explorerId);
    }

    public void Add(QuizParticipation participation)
    {
        _dbContext.QuizParticipations.Add(participation);
    }
}
