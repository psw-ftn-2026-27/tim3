namespace Games.Application.LocationChallengeSolving;

public sealed record LocationChallengeToSolveDto(
    Guid Id,
    string Name,
    double StartLatitude,
    double StartLongitude,
    int ExperienceGained,
    LocationChallengeState State);
