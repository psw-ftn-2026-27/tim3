namespace Social.Application.ClubMembership;

// PendingRequests is filled only for the club's owner; every other user gets an empty list.
public sealed record ClubDetailsDto(
    Guid Id,
    Guid OwnerId,
    string Name,
    string Description,
    List<string> Images,
    List<ClubUserDto> Members,
    MembershipState Membership,
    List<ClubUserDto> PendingRequests);
