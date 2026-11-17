namespace Games.Application.LocationChallengeAuthoring;

public sealed record LocationChallengeSummaryDto(
    Guid Id,
    string Name,
    int ExperienceGained,
    int HiddenPointCount);
