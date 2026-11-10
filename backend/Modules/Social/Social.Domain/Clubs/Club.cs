using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.Clubs;

public sealed class Club : AggregateRoot
{
    private const int MaxImages = 5;

    public Guid OwnerId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public List<string> Images { get; private set; }

    private Club()
    {
        Name = null!;
        Description = null!;
        Images = null!;
    }

    public Club(Guid ownerId, string name, string description, List<string> images)
        : base(Guid.NewGuid())
    {
        EnsureValid(name, description, images);

        OwnerId = ownerId;
        Name = name;
        Description = description;
        Images = images;
    }

    public void Update(Guid userId, string name, string description, List<string> images)
    {
        EnsureOwnedBy(userId);
        EnsureValid(name, description, images);

        Name = name;
        Description = description;
        Images = images;
    }

    public void EnsureCanBeDeletedBy(Guid userId)
    {
        EnsureOwnedBy(userId);
    }

    private void EnsureOwnedBy(Guid userId)
    {
        if (OwnerId != userId)
        {
            throw new NotFoundException($"Club {Id} does not exist.");
        }
    }

    private static void EnsureValid(string name, string description, List<string> images)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A club requires a name.");
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("A club requires a description.");
        }
        if (images.Count < 1 || images.Count > MaxImages)
        {
            throw new DomainException($"A club requires between one and {MaxImages} images.");
        }
    }
}
