using Games.Application.SocialChallenges;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.SocialChallengeLifecycle;

public sealed class SocialChallengeLifecycleService
{
    private readonly ISocialChallengeRepository _socialChallengeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SocialChallengeLifecycleService(
        ISocialChallengeRepository socialChallengeRepository,
        IUnitOfWork unitOfWork)
    {
        _socialChallengeRepository = socialChallengeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task PublishAsync(Guid socialChallengeId)
    {
        var socialChallenge = await GetSocialChallengeAsync(socialChallengeId);

        socialChallenge.Publish();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RetireAsync(Guid socialChallengeId)
    {
        var socialChallenge = await GetSocialChallengeAsync(socialChallengeId);

        socialChallenge.Retire();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ReactivateAsync(Guid socialChallengeId)
    {
        var socialChallenge = await GetSocialChallengeAsync(socialChallengeId);

        socialChallenge.Reactivate();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ArchiveAsync(Guid socialChallengeId)
    {
        var socialChallenge = await GetSocialChallengeAsync(socialChallengeId);

        socialChallenge.Archive();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid socialChallengeId)
    {
        var socialChallenge = await GetSocialChallengeAsync(socialChallengeId);

        socialChallenge.EnsureDeletable();
        _socialChallengeRepository.Remove(socialChallenge);
        await _unitOfWork.SaveChangesAsync();
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
