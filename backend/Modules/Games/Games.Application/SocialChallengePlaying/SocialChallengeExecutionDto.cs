using Games.Domain.SocialChallengeExecutions;

namespace Games.Application.SocialChallengePlaying;

// The explorer's latest execution of a challenge. EndMessage and ExperienceGained are null
// until the execution is solved.
public sealed record SocialChallengeExecutionDto(
    SocialChallengeExecutionStatus Status,
    int ActiveCount,
    int PeopleRequired,
    string? EndMessage,
    int? ExperienceGained);
