using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.TourPurchases;

public sealed class TourPurchaseToken : AggregateRoot
{
    public Guid BuyerId { get; private set; }
    public Guid TourId { get; private set; }
    public DateTime PurchasedAt { get; private set; }
    public Guid? CouponId { get; private set; }
    public int DiscountInEc { get; private set; }

    private TourPurchaseToken() { }

    public TourPurchaseToken(
        Guid buyerId, Guid tourId, DateTime purchasedAt, Guid? couponId = null, int discountInEc = 0)
        : base(Guid.NewGuid())
    {
        if (buyerId == Guid.Empty || tourId == Guid.Empty)
        {
            throw new DomainException("A purchase must have a buyer and a tour.");
        }

        if (discountInEc < 0 || (couponId is null && discountInEc != 0))
        {
            throw new DomainException("A discount cannot be negative and requires a coupon.");
        }

        BuyerId = buyerId;
        TourId = tourId;
        PurchasedAt = purchasedAt;
        CouponId = couponId;
        DiscountInEc = discountInEc;
    }
}
