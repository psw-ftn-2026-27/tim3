using Games.Application.SocialChallengeExecutions;
using Games.Domain.SocialChallengeExecutions;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.SocialChallengeExecutions;

internal sealed class SocialChallengeExecutionRepository : ISocialChallengeExecutionRepository
{
    private readonly GamesDbContext _dbContext;

    public SocialChallengeExecutionRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SocialChallengeExecution?> GetActiveAsync(Guid challengeId)
    {
        return _dbContext.SocialChallengeExecutions
            .Where(execution => execution.ChallengeId == challengeId
                && execution.Status == SocialChallengeExecutionStatus.Active)
            .OrderByDescending(execution => execution.StartedAt)
            .FirstOrDefaultAsync();
    }

    public Task<SocialChallengeExecution?> GetLatestForExplorerAsync(Guid challengeId, Guid explorerId)
    {
        return _dbContext.SocialChallengeExecutions
            .Where(execution => execution.ChallengeId == challengeId
                && execution.Participants.Any(participant => participant.ExplorerId == explorerId))
            .OrderByDescending(execution => execution.StartedAt)
            .FirstOrDefaultAsync();
    }

    public Task<List<Guid>> GetRewardedExplorerIdsAsync(Guid challengeId, DateTime from, DateTime to)
    {
        return _dbContext.SocialChallengeExecutions
            .Where(execution => execution.ChallengeId == challengeId
                && execution.Status == SocialChallengeExecutionStatus.Solved
                && execution.SolvedAt >= from
                && execution.SolvedAt < to)
            .SelectMany(execution => execution.Participants)
            .Where(participant => participant.ExperienceGained > 0)
            .Select(participant => participant.ExplorerId)
            .Distinct()
            .ToListAsync();
    }

    public void Add(SocialChallengeExecution execution)
    {
        _dbContext.SocialChallengeExecutions.Add(execution);
    }
}
