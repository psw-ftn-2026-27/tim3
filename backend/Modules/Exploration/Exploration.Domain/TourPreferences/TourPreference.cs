using Exploration.Domain.Tours;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.TourPreferences;

public sealed class TourPreference : AggregateRoot
{
    public Guid ExplorerId { get; private set; }
    public List<TourDifficulty> Difficulties { get; private set; }
    public TransportMode? Transport { get; private set; }
    public List<string> Tags { get; private set; }
    public bool AppliesToSearch { get; private set; }

    private TourPreference()
    {
        Difficulties = null!;
        Tags = null!;
    }

    public TourPreference(
        Guid explorerId,
        List<TourDifficulty> difficulties,
        TransportMode? transport,
        List<string> tags,
        bool appliesToSearch)
        : base(Guid.NewGuid())
    {
        EnsureValid(difficulties, transport, tags);

        ExplorerId = explorerId;
        Difficulties = difficulties;
        Transport = transport;
        Tags = tags;
        AppliesToSearch = appliesToSearch;
    }

    public void Change(List<TourDifficulty> difficulties, TransportMode? transport, List<string> tags, bool appliesToSearch)
    {
        EnsureValid(difficulties, transport, tags);

        Difficulties = difficulties;
        Transport = transport;
        Tags = tags;
        AppliesToSearch = appliesToSearch;
    }

    private static void EnsureValid(List<TourDifficulty> difficulties, TransportMode? transport, List<string> tags)
    {
        // P2: a difficulty level is chosen at most once.
        if (difficulties.Distinct().Count() != difficulties.Count)
        {
            throw new DomainException("A tour preference cannot repeat a difficulty.");
        }
        // P4: a tag is chosen at most once, regardless of letter case.
        if (tags.Any(string.IsNullOrWhiteSpace))
        {
            throw new DomainException("A tour preference cannot hold an empty tag.");
        }
        if (tags.Distinct(StringComparer.OrdinalIgnoreCase).Count() != tags.Count)
        {
            throw new DomainException("A tour preference cannot repeat a tag.");
        }
        // P5: at least one of difficulty, transport and tags is chosen.
        if (difficulties.Count == 0 && transport is null && tags.Count == 0)
        {
            throw new DomainException("A tour preference requires a difficulty, a transport or a tag.");
        }
    }
}
