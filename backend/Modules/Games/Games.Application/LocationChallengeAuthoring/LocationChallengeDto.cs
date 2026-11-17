namespace Games.Application.LocationChallengeAuthoring;

public sealed record LocationChallengeDto(
    Guid Id,
    string Name,
    double StartLatitude,
    double StartLongitude,
    int ExperienceGained,
    List<HiddenPointDto> HiddenPoints);
