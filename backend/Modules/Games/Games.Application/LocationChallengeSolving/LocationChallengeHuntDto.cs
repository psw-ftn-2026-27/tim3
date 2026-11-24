namespace Games.Application.LocationChallengeSolving;

public sealed record LocationChallengeHuntDto(
    Guid Id,
    string Name,
    int ExperienceGained,
    LocationChallengeState State,
    SearchCircleDto? SearchCircle,
    List<HiddenPointClueDto> HiddenPoints);
