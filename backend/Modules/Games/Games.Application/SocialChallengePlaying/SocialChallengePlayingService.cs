using Games.Application.SocialChallengeExecutions;
using Games.Application.SocialChallenges;
using Games.Domain.Locations;
using Games.Domain.SocialChallengeExecutions;
using Games.Domain.SocialChallenges;
using Maps.Contracts;
using Shared.Domain.Exceptions;

namespace Games.Application.SocialChallengePlaying;

public sealed class SocialChallengePlayingService
{
    private readonly ISocialChallengeRepository _socialChallengeRepository;
    private readonly ISocialChallengeExecutionRepository _executionRepository;
    private readonly IMapsApi _mapsApi;
    private readonly SocialChallengePlay _socialChallengePlay;
    private readonly IUnitOfWork _unitOfWork;

    public SocialChallengePlayingService(
        ISocialChallengeRepository socialChallengeRepository,
        ISocialChallengeExecutionRepository executionRepository,
        IMapsApi mapsApi,
        SocialChallengePlay socialChallengePlay,
        IUnitOfWork unitOfWork)
    {
        _socialChallengeRepository = socialChallengeRepository;
        _executionRepository = executionRepository;
        _mapsApi = mapsApi;
        _socialChallengePlay = socialChallengePlay;
        _unitOfWork = unitOfWork;
    }

    public async Task ActivateAsync(Guid socialChallengeId, Guid explorerId)
    {
        var socialChallenge = await GetSocialChallengeAsync(socialChallengeId);
        var position = await GetPositionAsync(explorerId);
        var currentExecution = await _executionRepository.GetActiveAsync(socialChallengeId);

        var execution = _socialChallengePlay.Activate(socialChallenge, currentExecution, explorerId,
            new Location(position.Latitude, position.Longitude), position.SetAt, DateTime.UtcNow);
        if (execution != currentExecution)
        {
            _executionRepository.Add(execution);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ReportPositionAsync(Guid socialChallengeId, Guid explorerId)
    {
        var now = DateTime.UtcNow;
        var socialChallenge = await GetSocialChallengeAsync(socialChallengeId);
        var position = await GetPositionAsync(explorerId);
        var execution = await _executionRepository.GetLatestForExplorerAsync(socialChallengeId, explorerId);
        if (execution is null)
        {
            throw new NotFoundException($"You have not activated social challenge {socialChallengeId}.");
        }
        var day = socialChallenge.ActivePeriod.DayOf(now);
        var rewardedToday = await _executionRepository.GetRewardedExplorerIdsAsync(socialChallengeId, day.From, day.To);

        _socialChallengePlay.Report(socialChallenge, execution, explorerId,
            new Location(position.Latitude, position.Longitude), position.SetAt, now, rewardedToday);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<CurrentPosition> GetPositionAsync(Guid explorerId)
    {
        var position = await _mapsApi.GetCurrentPositionAsync(explorerId);
        if (position is null)
        {
            throw new DomainException("Set your position on the map first.");
        }
        return position;
    }

    private async Task<SocialChallenge> GetSocialChallengeAsync(Guid socialChallengeId)
    {
        var socialChallenge = await _socialChallengeRepository.GetByIdAsync(socialChallengeId);
        if (socialChallenge is null)
        {
            throw new NotFoundException($"Social challenge {socialChallengeId} does not exist.");
        }
        return socialChallenge;
    }
}
