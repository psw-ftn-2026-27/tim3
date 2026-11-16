using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.TourPurchases;

public sealed class TourPurchaseToken : AggregateRoot
{
    public Guid BuyerId { get; private set; }
    public Guid TourId { get; private set; }
    public DateTime PurchasedAt { get; private set; }

    private TourPurchaseToken() { }

    public TourPurchaseToken(Guid buyerId, Guid tourId, DateTime purchasedAt) : base(Guid.NewGuid())
    {
        if (buyerId == Guid.Empty || tourId == Guid.Empty)
        {
            throw new DomainException("Kupovina mora imati kupca i turu.");
        }

        BuyerId = buyerId;
        TourId = tourId;
        PurchasedAt = purchasedAt;
    }
}
