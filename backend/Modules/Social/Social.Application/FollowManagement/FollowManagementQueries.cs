using Identity.Contracts;
using Shared.Domain;
using Social.Application.Profiles;

namespace Social.Application.FollowManagement;

public sealed class FollowManagementQueries
{
    private readonly IProfileReadRepository _profileReadRepository;
    private readonly IIdentityApi _identityApi;

    public FollowManagementQueries(IProfileReadRepository profileReadRepository, IIdentityApi identityApi)
    {
        _profileReadRepository = profileReadRepository;
        _identityApi = identityApi;
    }

    public Task<ProfileSummaryDto> GetProfileSummaryAsync(Guid userId)
    {
        return _profileReadRepository.GetSummaryAsync(userId);
    }

    public async Task<PageResult<ConnectedUserDto>> GetFollowersAsync(Guid userId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var followerIds = await _profileReadRepository.GetFollowerIdsAsync(userId, page, pageSize);
        return await WithDisplayNamesAsync(followerIds);
    }

    public async Task<PageResult<ConnectedUserDto>> GetFollowingAsync(Guid userId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var followingIds = await _profileReadRepository.GetFollowingIdsAsync(userId, page, pageSize);
        return await WithDisplayNamesAsync(followingIds);
    }

    private async Task<PageResult<ConnectedUserDto>> WithDisplayNamesAsync(PageResult<Guid> userIds)
    {
        var names = await _identityApi.GetDisplayNamesAsync(userIds.Items);
        var namesById = names.ToDictionary(name => name.UserId, name => name.DisplayName);

        var items = userIds.Items
            .Select(userId => new ConnectedUserDto(userId, namesById.GetValueOrDefault(userId, "")))
            .ToList();
        return new PageResult<ConnectedUserDto>(items, userIds.TotalCount);
    }
}
