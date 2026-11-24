using Identity.Contracts;
using Shared.Domain.Exceptions;
using Social.Application.Clubs;

namespace Social.Application.ClubMembership;

public sealed class ClubMembershipQueries
{
    private readonly IClubReadRepository _clubReadRepository;
    private readonly IIdentityApi _identityApi;

    public ClubMembershipQueries(IClubReadRepository clubReadRepository, IIdentityApi identityApi)
    {
        _clubReadRepository = clubReadRepository;
        _identityApi = identityApi;
    }

    public async Task<ClubDetailsDto> GetClubAsync(Guid userId, Guid clubId)
    {
        var club = await _clubReadRepository.GetDetailsAsync(clubId);
        if (club is null)
        {
            throw new NotFoundException($"Club {clubId} does not exist.");
        }

        var role = RoleOf(userId, club);
        var visibleRequesterIds = role is MemberRole.Owner or MemberRole.Moderator ? club.RequesterIds : [];
        var names = await GetDisplayNamesAsync([.. club.MemberIds, .. visibleRequesterIds]);

        return new ClubDetailsDto(
            club.Id,
            club.OwnerId,
            club.Name,
            club.Description,
            club.Images,
            ToMemberDtos(club, names),
            MembershipOf(userId, club),
            role,
            ToUserDtos(visibleRequesterIds, names));
    }

    private static MembershipState MembershipOf(Guid userId, ClubDetailsRowDto club)
    {
        if (club.MemberIds.Contains(userId))
        {
            return MembershipState.Member;
        }
        if (club.RequesterIds.Contains(userId))
        {
            return MembershipState.Pending;
        }
        return MembershipState.None;
    }

    private static MemberRole? RoleOf(Guid userId, ClubDetailsRowDto club)
    {
        return club.MemberIds.Contains(userId) ? RoleOfMember(userId, club) : null;
    }

    // The owner's row carries the Member role, so the owner is recognized by the club's OwnerId.
    private static MemberRole RoleOfMember(Guid memberId, ClubDetailsRowDto club)
    {
        if (memberId == club.OwnerId)
        {
            return MemberRole.Owner;
        }
        if (club.ModeratorIds.Contains(memberId))
        {
            return MemberRole.Moderator;
        }
        return MemberRole.Member;
    }

    private static List<ClubMemberDto> ToMemberDtos(ClubDetailsRowDto club, Dictionary<Guid, string> names)
    {
        return club.MemberIds
            .Select(memberId => new ClubMemberDto(memberId, names.GetValueOrDefault(memberId, ""), RoleOfMember(memberId, club)))
            .ToList();
    }

    private static List<ClubUserDto> ToUserDtos(List<Guid> userIds, Dictionary<Guid, string> names)
    {
        return userIds
            .Select(userId => new ClubUserDto(userId, names.GetValueOrDefault(userId, "")))
            .ToList();
    }

    private async Task<Dictionary<Guid, string>> GetDisplayNamesAsync(List<Guid> userIds)
    {
        var names = await _identityApi.GetDisplayNamesAsync(userIds);
        return names.ToDictionary(name => name.UserId, name => name.DisplayName);
    }
}
