using Games.Domain.SocialChallenges;

namespace Games.Application.SocialChallengeAuthoring;

public sealed record SocialChallengeDto(
    Guid Id,
    string Name,
    SocialChallengeStatus Status,
    TimeOnly ActivePeriodStart,
    TimeOnly ActivePeriodEnd,
    int PeopleRequired,
    double GatheringRadius,
    int ExperienceGained,
    string EndMessage);
