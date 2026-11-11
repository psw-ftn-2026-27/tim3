using Identity.Contracts;
using Shared.Domain;
using Social.Application.Profiles;
using Social.Domain.Profiles;

namespace Social.Application.FollowRequesting;

public sealed class FollowRequestingQueries
{
    private readonly IProfileReadRepository _profileReadRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly IIdentityApi _identityApi;

    public FollowRequestingQueries(
        IProfileReadRepository profileReadRepository,
        IProfileRepository profileRepository,
        IIdentityApi identityApi)
    {
        _profileReadRepository = profileReadRepository;
        _profileRepository = profileRepository;
        _identityApi = identityApi;
    }

    public async Task<PageResult<FoundUserDto>> SearchUsersAsync(Guid userId, string text, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var users = await _identityApi.SearchUsersAsync(text, userId, page, pageSize);
        var profile = await _profileRepository.GetByUserIdAsync(userId) ?? new Profile(userId);
        var now = DateTime.UtcNow;

        var items = users.Items
            .Select(user => new FoundUserDto(user.UserId, user.DisplayName, ToDto(profile.RelationTo(user.UserId, now))))
            .ToList();
        return new PageResult<FoundUserDto>(items, users.TotalCount);
    }

    public async Task<PageResult<SentFollowRequestDto>> GetSentRequestsAsync(Guid userId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var rows = await _profileReadRepository.GetSentRequestsAsync(userId, page, pageSize);
        var names = await GetDisplayNamesAsync(rows.Items.Select(row => row.RecipientId).ToList());

        var items = rows.Items
            .Select(row => new SentFollowRequestDto(
                row.Id, row.RecipientId, names.GetValueOrDefault(row.RecipientId, ""), row.SentAt, row.Status))
            .ToList();
        return new PageResult<SentFollowRequestDto>(items, rows.TotalCount);
    }

    public async Task<PageResult<IncomingFollowRequestDto>> GetIncomingRequestsAsync(Guid userId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var rows = await _profileReadRepository.GetIncomingRequestsAsync(userId, page, pageSize);
        var names = await GetDisplayNamesAsync(rows.Items.Select(row => row.SenderId).ToList());

        var items = rows.Items
            .Select(row => new IncomingFollowRequestDto(
                row.Id, row.SenderId, names.GetValueOrDefault(row.SenderId, ""), row.SentAt))
            .ToList();
        return new PageResult<IncomingFollowRequestDto>(items, rows.TotalCount);
    }

    private async Task<Dictionary<Guid, string>> GetDisplayNamesAsync(List<Guid> userIds)
    {
        var names = await _identityApi.GetDisplayNamesAsync(userIds);
        return names.ToDictionary(name => name.UserId, name => name.DisplayName);
    }

    private static RelationDto ToDto(Relation relation)
    {
        return new RelationDto(relation.Kind, relation.RequestId, relation.AvailableFrom);
    }
}
