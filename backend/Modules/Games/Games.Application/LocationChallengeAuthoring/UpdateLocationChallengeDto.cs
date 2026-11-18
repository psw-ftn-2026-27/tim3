namespace Games.Application.LocationChallengeAuthoring;

public sealed record UpdateLocationChallengeDto(
    string Name,
    double StartLatitude,
    double StartLongitude,
    int ExperienceGained,
    List<CreateHiddenPointDto> HiddenPoints);
