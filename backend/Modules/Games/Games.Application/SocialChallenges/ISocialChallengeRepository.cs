using Games.Domain.SocialChallenges;

namespace Games.Application.SocialChallenges;

public interface ISocialChallengeRepository
{
    Task<SocialChallenge?> GetByIdAsync(Guid id);
    void Add(SocialChallenge socialChallenge);
    void Remove(SocialChallenge socialChallenge);
}
