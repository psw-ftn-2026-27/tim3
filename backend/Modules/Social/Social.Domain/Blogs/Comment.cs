using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.Blogs;

public sealed class Comment : Entity
{
    public static readonly TimeSpan EditWindow = TimeSpan.FromMinutes(15);

    public Guid UserId { get; private set; }
    public string Text { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastModifiedAt { get; private set; }
    public DateTime EditableUntil => CreatedAt + EditWindow;

    private Comment()
    {
        Text = null!;
    }

    public Comment(Guid userId, string text) : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new DomainException("A comment requires text.");
        }

        UserId = userId;
        Text = text;
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = CreatedAt;
    }

    public void EnsureWithinEditWindow(DateTime now)
    {
        if (now > CreatedAt + EditWindow)
        {
            throw new DomainException("A comment can be changed only within 15 minutes of posting.");
        }
    }

    public void Update(string text, DateTime now)
    {
        EnsureWithinEditWindow(now);

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new DomainException("A comment requires text.");
        }

        Text = text;
        LastModifiedAt = now;
    }
}
