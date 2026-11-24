using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed class Tour : AggregateRoot
{
    private const int MinimumDescriptionLengthForPublishing = 100;
    private const int MinimumKeyPointsForPublishing = 2;
    private const int MinimumKeyPointsForLength = 2;

    private readonly List<TransportTime> _transportTimes = [];
    private readonly List<KeyPoint> _keyPoints = [];

    public Guid AuthorId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public TourDifficulty Difficulty { get; private set; }
    public List<string> Tags { get; private set; }
    public TourStatus Status { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public DateTime? ArchivedAt { get; private set; }
    public GroupSize? RecommendedGroupSize { get; private set; }
    public IReadOnlyList<TransportTime> TransportTimes => _transportTimes;
    public IReadOnlyList<KeyPoint> KeyPoints => _keyPoints;
    public TourLength Length { get; private set; }

    private Tour()
    {
        Name = null!;
        Description = null!;
        Tags = null!;
        Length = null!;
    }

    public Tour(Guid authorId, string name, string description, TourDifficulty difficulty, List<string> tags, GroupSize? groupSize)
        : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A tour requires a name.");
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("A tour requires a description.");
        }
        if (tags is null || tags.Count == 0)
        {
            throw new DomainException("A tour requires at least one tag.");
        }
        if (groupSize is null)
        {
            throw new DomainException("A tour requires a recommended group size.");
        }

        AuthorId = authorId;
        Name = name;
        Description = description;
        Difficulty = difficulty;
        Tags = tags;
        RecommendedGroupSize = groupSize;
        Status = TourStatus.Draft;
        Length = new TourLength(0, false);
    }

    public void AddTransportTime(TransportMode transport, int minutes)
    {
        if (Status == TourStatus.Archived)
        {
            throw new DomainException("An archived tour does not accept a transport time.");
        }
        if (_transportTimes.Any(time => time.Transport == transport))
        {
            throw new DomainException($"The tour already has a time for {transport}.");
        }

        _transportTimes.Add(new TransportTime(transport, minutes));
    }

    public void DefineKeyPoints(List<KeyPoint> keyPoints, TourLength length)
    {
        if (Status != TourStatus.Draft)
        {
            throw new DomainException("Key points can be changed only while the tour is a draft.");
        }
        if (keyPoints.Count < MinimumKeyPointsForLength && length.Kilometres != 0)
        {
            throw new DomainException($"A tour with fewer than {MinimumKeyPointsForLength} key points has a length of zero.");
        }

        _keyPoints.Clear();
        _keyPoints.AddRange(keyPoints);
        for (var index = 0; index < _keyPoints.Count; index++)
        {
            _keyPoints[index].Order = index + 1;
        }
        Length = length;
    }

    public void Publish(TourPrice? price)
    {
        if (Status != TourStatus.Draft)
        {
            throw new DomainException("Only a draft tour can be published.");
        }
        if (price is null)
        {
            throw new DomainException("A tour can be published only with a chosen price.");
        }
        if (Description.Length < MinimumDescriptionLengthForPublishing)
        {
            throw new DomainException($"A tour can be published only with a description of at least {MinimumDescriptionLengthForPublishing} characters.");
        }
        if (_transportTimes.Count == 0)
        {
            throw new DomainException("A tour can be published only with at least one transport time.");
        }
        if (_keyPoints.Count < MinimumKeyPointsForPublishing)
        {
            throw new DomainException($"A tour can be published only with at least {MinimumKeyPointsForPublishing} key points.");
        }

        Status = TourStatus.Published;
        PublishedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        if (Status != TourStatus.Published)
        {
            throw new DomainException("Only a published tour can be archived.");
        }

        Status = TourStatus.Archived;
        ArchivedAt = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        if (Status != TourStatus.Archived)
        {
            throw new DomainException("Only an archived tour can be reactivated.");
        }

        Status = TourStatus.Published;
        ArchivedAt = null;
    }
}
