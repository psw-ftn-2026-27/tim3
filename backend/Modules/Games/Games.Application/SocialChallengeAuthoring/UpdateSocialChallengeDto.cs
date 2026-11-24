namespace Games.Application.SocialChallengeAuthoring;

public sealed record UpdateSocialChallengeDto(
    string Name,
    TimeOnly ActivePeriodStart,
    TimeOnly ActivePeriodEnd,
    int PeopleRequired,
    double GatheringRadius,
    int ExperienceGained,
    string EndMessage);
