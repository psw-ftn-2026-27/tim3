using Social.Application.Clubs;

namespace Social.Application.ClubMembership;

// Role is the logged-in user's role in the club, null when they are not a member.
// PendingRequests is filled only for the club's owner and moderators; every other user gets an empty list.
public sealed record ClubDetailsDto(
    Guid Id,
    Guid OwnerId,
    string Name,
    string Description,
    List<string> Images,
    List<ClubMemberDto> Members,
    MembershipState Membership,
    MemberRole? Role,
    List<ClubUserDto> PendingRequests);
