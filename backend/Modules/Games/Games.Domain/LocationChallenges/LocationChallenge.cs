using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallenges;

public sealed class LocationChallenge : AggregateRoot
{
    public const int MinExperienceGained = 1;
    public const int MaxExperienceGained = 7000;

    private readonly List<HiddenPoint> _hiddenPoints = [];

    public string Name { get; private set; }
    public Location StartLocation { get; private set; }
    public int ExperienceGained { get; private set; }
    public IReadOnlyList<HiddenPoint> HiddenPoints => _hiddenPoints;

    private LocationChallenge()
    {
        Name = null!;
        StartLocation = null!;
    }

    public LocationChallenge(string name, Location? startLocation, int experienceGained,
        List<HiddenPoint> hiddenPoints)
        : base(Guid.NewGuid())
    {
        EnsureValid(name, startLocation, experienceGained, hiddenPoints);

        Name = name;
        StartLocation = startLocation!;
        ExperienceGained = experienceGained;
        _hiddenPoints.AddRange(hiddenPoints);
    }

    public void Update(string name, Location? startLocation, int experienceGained,
        List<HiddenPoint> hiddenPoints)
    {
        EnsureValid(name, startLocation, experienceGained, hiddenPoints);

        Name = name;
        StartLocation = startLocation!;
        ExperienceGained = experienceGained;
        _hiddenPoints.Clear();
        _hiddenPoints.AddRange(hiddenPoints);
    }

    private static void EnsureValid(string name, Location? startLocation, int experienceGained,
        List<HiddenPoint> hiddenPoints)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A location challenge requires a name.");
        }
        if (startLocation is null)
        {
            throw new DomainException("A location challenge requires a start location.");
        }
        if (experienceGained < MinExperienceGained || experienceGained > MaxExperienceGained)
        {
            throw new DomainException($"A location challenge must award between {MinExperienceGained} and {MaxExperienceGained} experience points.");
        }
        if (hiddenPoints.Count == 0)
        {
            throw new DomainException("A location challenge requires at least one hidden point.");
        }
    }
}
