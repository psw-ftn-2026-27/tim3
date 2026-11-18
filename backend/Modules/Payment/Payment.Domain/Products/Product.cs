using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Products;

public sealed class Product : AggregateRoot
{
    public Guid TourId { get; private set; }
    public string Name { get; private set; } = null!;
    public Guid AuthorId { get; private set; }
    public int PriceInEc { get; private set; }

    private Product() { }

    public Product(Guid tourId, string name, Guid authorId, int priceInEc) : base(Guid.NewGuid())
    {
        if (tourId == Guid.Empty || authorId == Guid.Empty)
        {
            throw new DomainException("A product must have a tour and an owner.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A product requires a name.");
        }

        if (priceInEc < 0)
        {
            throw new DomainException("The product price cannot be negative.");
        }

        TourId = tourId;
        Name = name;
        AuthorId = authorId;
        PriceInEc = priceInEc;
    }

    public void EnsureSameOffer(string name, Guid authorId, int priceInEc)
    {
        if (Name != name || AuthorId != authorId || PriceInEc != priceInEc)
        {
            throw new DomainException("A product already exists for this tour. Retry publication with the original tour details and price.");
        }
    }
}
