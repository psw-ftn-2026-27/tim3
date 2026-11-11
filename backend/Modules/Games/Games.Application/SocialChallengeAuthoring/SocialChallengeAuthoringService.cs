using AutoMapper;
using Games.Application.SocialChallenges;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.SocialChallengeAuthoring;

public sealed class SocialChallengeAuthoringService
{
    private readonly ISocialChallengeRepository _socialChallengeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SocialChallengeAuthoringService(
        ISocialChallengeRepository socialChallengeRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _socialChallengeRepository = socialChallengeRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SocialChallengeDto> CreateAsync(CreateSocialChallengeDto dto)
    {
        var socialChallenge = new SocialChallenge(dto.Name, dto.ActivePeriodStart, dto.ActivePeriodEnd,
            dto.PeopleRequired, dto.GatheringRadius, dto.ExperienceGained, dto.EndMessage);

        _socialChallengeRepository.Add(socialChallenge);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<SocialChallengeDto>(socialChallenge);
    }

    public async Task UpdateAsync(Guid socialChallengeId, UpdateSocialChallengeDto dto)
    {
        var socialChallenge = await GetSocialChallengeAsync(socialChallengeId);

        socialChallenge.Update(dto.Name, dto.ActivePeriodStart, dto.ActivePeriodEnd,
            dto.PeopleRequired, dto.GatheringRadius, dto.ExperienceGained, dto.EndMessage);
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
