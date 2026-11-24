using Games.Application.SocialChallengeExecutions;
using Games.Application.SocialChallenges;
using Games.Domain.SocialChallengeExecutions;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Application.SocialChallengePlaying;

public sealed class SocialChallengePlayingQueries
{
    private readonly ISocialChallengeReadRepository _socialChallengeReadRepository;
    private readonly ISocialChallengeRepository _socialChallengeRepository;
    private readonly ISocialChallengeExecutionRepository _executionRepository;

    public SocialChallengePlayingQueries(
        ISocialChallengeReadRepository socialChallengeReadRepository,
        ISocialChallengeRepository socialChallengeRepository,
        ISocialChallengeExecutionRepository executionRepository)
    {
        _socialChallengeReadRepository = socialChallengeReadRepository;
        _socialChallengeRepository = socialChallengeRepository;
        _executionRepository = executionRepository;
    }

    public Task<PageResult<PublishedSocialChallengeDto>> GetPublishedAsync(Guid explorerId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _socialChallengeReadRepository.GetPublishedAsync(explorerId, page, pageSize);
    }

    // Expiry and the active count are derived by the execution from the current time, so the
    // execution is loaded as an aggregate instead of repeating that derivation in SQL.
    public async Task<SocialChallengeExecutionDto> GetExecutionAsync(Guid socialChallengeId, Guid explorerId)
    {
        var now = DateTime.UtcNow;
        var socialChallenge = await _socialChallengeRepository.GetByIdAsync(socialChallengeId);
        if (socialChallenge is null)
        {
            throw new NotFoundException($"Social challenge {socialChallengeId} does not exist.");
        }
        var execution = await _executionRepository.GetLatestForExplorerAsync(socialChallengeId, explorerId);
        if (execution is null)
        {
            throw new NotFoundException($"You have not activated social challenge {socialChallengeId}.");
        }

        var status = execution.IsExpired(now) ? SocialChallengeExecutionStatus.Expired : execution.Status;
        var participant = execution.Participants.Single(participant => participant.ExplorerId == explorerId);

        return new SocialChallengeExecutionDto(
            status,
            execution.ActiveCount(now),
            socialChallenge.PeopleRequired,
            status == SocialChallengeExecutionStatus.Solved ? socialChallenge.EndMessage : null,
            participant.ExperienceGained);
    }
}
