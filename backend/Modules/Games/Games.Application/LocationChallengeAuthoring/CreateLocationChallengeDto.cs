namespace Games.Application.LocationChallengeAuthoring;

public sealed record CreateLocationChallengeDto(
    string Name,
    double StartLatitude,
    double StartLongitude,
    int ExperienceGained,
    List<CreateHiddenPointDto> HiddenPoints);
