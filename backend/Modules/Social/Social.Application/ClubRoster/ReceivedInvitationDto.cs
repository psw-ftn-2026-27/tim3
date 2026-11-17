namespace Social.Application.ClubRoster;

// A club that has invited the logged-in user.
public sealed record ReceivedInvitationDto(Guid ClubId, string ClubName);
