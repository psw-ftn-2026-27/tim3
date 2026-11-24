using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallenges;

public sealed class LocationChallengeParticipation : AggregateRoot
{
    private readonly List<Guid> _discoveredPointIds = [];

    public Guid ExplorerId { get; private set; }
    public Guid LocationChallengeId { get; private set; }
    public SearchCircle SearchCircle { get; private set; }
    public IReadOnlyList<Guid> DiscoveredPointIds => _discoveredPointIds;
    public LocationChallengeParticipationStatus Status { get; private set; }
    public DateTime? SolvedAt { get; private set; }
    public int? ExperienceGained { get; private set; }

    private LocationChallengeParticipation()
    {
        SearchCircle = null!;
    }

    public LocationChallengeParticipation(Guid explorerId, Guid locationChallengeId, SearchCircle searchCircle)
        : base(Guid.NewGuid())
    {
        ExplorerId = explorerId;
        LocationChallengeId = locationChallengeId;
        SearchCircle = searchCircle;
        Status = LocationChallengeParticipationStatus.Active;
    }

    public void EnsureCanContinue()
    {
        if (Status == LocationChallengeParticipationStatus.Solved)
        {
            throw new DomainException("A solved location challenge cannot be continued.");
        }
    }

    public void Discover(Guid hiddenPointId, int pointCount, int experienceGained, DateTime time)
    {
        EnsureCanContinue();
        if (_discoveredPointIds.Contains(hiddenPointId))
        {
            throw new DomainException("This hidden point is already discovered.");
        }

        _discoveredPointIds.Add(hiddenPointId);

        if (_discoveredPointIds.Count == pointCount)
        {
            Status = LocationChallengeParticipationStatus.Solved;
            SolvedAt = time;
            ExperienceGained = experienceGained;
        }
    }
}
