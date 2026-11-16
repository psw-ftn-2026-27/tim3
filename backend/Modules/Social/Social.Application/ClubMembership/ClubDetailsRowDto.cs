namespace Social.Application.ClubMembership;

// What the read repository knows about a club: user IDs only. The display names come from
// Identity, so ClubMembershipQueries turns this into a ClubDetailsDto.
public sealed record ClubDetailsRowDto(
    Guid Id,
    Guid OwnerId,
    string Name,
    string Description,
    List<string> Images,
    List<Guid> MemberIds,
    List<Guid> RequesterIds);
