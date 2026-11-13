using Games.Application.SocialChallengeAuthoring;
using Shared.Domain;

namespace Games.Application.SocialChallenges;

public interface ISocialChallengeReadRepository
{
    Task<PageResult<SocialChallengeDto>> GetPageAsync(int page, int pageSize);
    Task<SocialChallengeDto?> GetByIdAsync(Guid id);
}
