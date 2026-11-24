using Social.Application.Profiles;
using Social.Domain.Profiles;

namespace Social.Application.FollowManagement;

public sealed class FollowManagementService
{
    private readonly IProfileRepository _profileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FollowManagementService(IProfileRepository profileRepository, IUnitOfWork unitOfWork)
    {
        _profileRepository = profileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task RemoveFollowerAsync(Guid userId, Guid followerId)
    {
        var follower = await _profileRepository.GetByUserIdAsync(followerId) ?? new Profile(followerId);
        var remover = await _profileRepository.GetByUserIdAsync(userId);
        if (remover is null)
        {
            remover = new Profile(userId);
            _profileRepository.Add(remover);
        }

        FollowerRemoval.Remove(remover, follower);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UnfollowAsync(Guid userId, Guid followedId)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId) ?? new Profile(userId);

        profile.Unfollow(followedId);
        await _unitOfWork.SaveChangesAsync();
    }
}
