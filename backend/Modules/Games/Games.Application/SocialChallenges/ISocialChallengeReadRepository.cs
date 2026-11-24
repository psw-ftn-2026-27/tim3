using Games.Application.SocialChallengeAuthoring;
using Games.Application.SocialChallengePlaying;
using Shared.Domain;

namespace Games.Application.SocialChallenges;

public interface ISocialChallengeReadRepository
{
    Task<PageResult<SocialChallengeDto>> GetPageAsync(int page, int pageSize);
    Task<SocialChallengeDto?> GetByIdAsync(Guid id);
    Task<PageResult<PublishedSocialChallengeDto>> GetPublishedAsync(Guid explorerId, int page, int pageSize);
}
