namespace Games.Application.SocialChallengePlaying;

// A published social challenge as the asking explorer sees it: how many times they solved it.
public sealed record PublishedSocialChallengeDto(
    Guid Id,
    string Name,
    TimeOnly ActivePeriodStart,
    TimeOnly ActivePeriodEnd,
    int PeopleRequired,
    double Latitude,
    double Longitude,
    int SolvedCount);
