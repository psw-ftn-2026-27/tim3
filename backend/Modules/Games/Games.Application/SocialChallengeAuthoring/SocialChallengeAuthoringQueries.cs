using Games.Application.SocialChallenges;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Application.SocialChallengeAuthoring;

public sealed class SocialChallengeAuthoringQueries
{
    private readonly ISocialChallengeReadRepository _socialChallengeReadRepository;

    public SocialChallengeAuthoringQueries(ISocialChallengeReadRepository socialChallengeReadRepository)
    {
        _socialChallengeReadRepository = socialChallengeReadRepository;
    }

    public Task<PageResult<SocialChallengeDto>> GetAllAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _socialChallengeReadRepository.GetPageAsync(page, pageSize);
    }

    public async Task<SocialChallengeDto> GetByIdAsync(Guid socialChallengeId)
    {
        var socialChallenge = await _socialChallengeReadRepository.GetByIdAsync(socialChallengeId);
        if (socialChallenge is null)
        {
            throw new NotFoundException($"Social challenge {socialChallengeId} does not exist.");
        }

        return socialChallenge;
    }
}
