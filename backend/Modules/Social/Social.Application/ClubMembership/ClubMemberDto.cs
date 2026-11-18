namespace Social.Application.ClubMembership;

public sealed record ClubMemberDto(Guid UserId, string DisplayName, MemberRole Role);
