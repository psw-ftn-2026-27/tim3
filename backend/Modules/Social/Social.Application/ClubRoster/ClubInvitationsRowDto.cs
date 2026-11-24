namespace Social.Application.ClubRoster;

// What the read repository knows about a club's invitations: user IDs only. The display
// names come from Identity, so ClubRosterQueries turns this into a list of ClubUserDto.
public sealed record ClubInvitationsRowDto(Guid OwnerId, List<Guid> InviteeIds);
