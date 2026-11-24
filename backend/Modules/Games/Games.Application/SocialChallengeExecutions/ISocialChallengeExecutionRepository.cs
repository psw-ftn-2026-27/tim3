using Games.Domain.SocialChallengeExecutions;

namespace Games.Application.SocialChallengeExecutions;

public interface ISocialChallengeExecutionRepository
{
    Task<SocialChallengeExecution?> GetActiveAsync(Guid challengeId);
    Task<SocialChallengeExecution?> GetLatestForExplorerAsync(Guid challengeId, Guid explorerId);
    Task<List<Guid>> GetRewardedExplorerIdsAsync(Guid challengeId, DateTime from, DateTime to);
    void Add(SocialChallengeExecution execution);
}
