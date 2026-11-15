using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.SocialChallenges;

public sealed class SocialChallenge : AggregateRoot
{
    private const int MinimumPeopleRequired = 2;
    private const double MaximumGatheringRadius = 50;
    private const double SquareMetersPerPerson = 10;
    private const int MaximumExperienceGained = 20000;

    public string Name { get; private set; }
    public SocialChallengeStatus Status { get; private set; }
    public ActivePeriod ActivePeriod { get; private set; }
    public int PeopleRequired { get; private set; }
    public double GatheringRadius { get; private set; }
    public int ExperienceGained { get; private set; }
    public string EndMessage { get; private set; }

    private SocialChallenge()
    {
        Name = null!;
        ActivePeriod = null!;
        EndMessage = null!;
    }

    public SocialChallenge(string name, ActivePeriod activePeriod, int peopleRequired,
        double gatheringRadius, int experienceGained, string endMessage)
        : base(Guid.NewGuid())
    {
        EnsureValid(name, peopleRequired, gatheringRadius, experienceGained, endMessage);

        Name = name;
        Status = SocialChallengeStatus.Draft;
        ActivePeriod = activePeriod;
        PeopleRequired = peopleRequired;
        GatheringRadius = gatheringRadius;
        ExperienceGained = experienceGained;
        EndMessage = endMessage;
    }

    public void Update(string name, ActivePeriod activePeriod, int peopleRequired,
        double gatheringRadius, int experienceGained, string endMessage)
    {
        if (Status != SocialChallengeStatus.Draft)
        {
            throw new DomainException("A social challenge can be changed only while it is a draft.");
        }
        EnsureValid(name, peopleRequired, gatheringRadius, experienceGained, endMessage);

        Name = name;
        ActivePeriod = activePeriod;
        PeopleRequired = peopleRequired;
        GatheringRadius = gatheringRadius;
        ExperienceGained = experienceGained;
        EndMessage = endMessage;
    }

    private static void EnsureValid(string name, int peopleRequired, double gatheringRadius,
        int experienceGained, string endMessage)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A social challenge requires a name.");
        }
        if (string.IsNullOrWhiteSpace(endMessage))
        {
            throw new DomainException("A social challenge requires an end message.");
        }
        if (peopleRequired < MinimumPeopleRequired)
        {
            throw new DomainException($"A social challenge requires at least {MinimumPeopleRequired} people.");
        }
        if (gatheringRadius <= 0 || gatheringRadius > MaximumGatheringRadius)
        {
            throw new DomainException($"A social challenge requires a gathering radius greater than 0 and at most {MaximumGatheringRadius} m.");
        }
        if (peopleRequired > MaximumPeopleFor(gatheringRadius))
        {
            throw new DomainException($"A gathering radius of {gatheringRadius} m fits at most {MaximumPeopleFor(gatheringRadius)} people.");
        }
        if (experienceGained < 0 || experienceGained > MaximumExperienceGained)
        {
            throw new DomainException($"A social challenge requires experience between 0 and {MaximumExperienceGained}.");
        }
    }

    private static int MaximumPeopleFor(double gatheringRadius)
    {
        return (int)Math.Floor(Math.PI * gatheringRadius * gatheringRadius / SquareMetersPerPerson);
    }
}
