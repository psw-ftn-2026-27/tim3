namespace Payment.Domain.ShoppingCarts;

// The gift card chosen in the cart: applied for AppliedInEc, spread over the tours in AppliedPerTour,
// with UnusedInEc of its amount lost, or not applied for Reason.
public sealed record PricedGiftCard(
    Guid GiftCardId,
    int AmountInEc,
    int AppliedInEc,
    int UnusedInEc,
    IReadOnlyDictionary<Guid, int> AppliedPerTour,
    GiftCardNotAppliedReason? Reason)
{
    public bool IsApplied => Reason is null;
}

public sealed record CartPrice(
    CouponAllocationResult Coupons, PricedGiftCard? GiftCard, long TotalAfterCouponsInEc, long TotalFromWalletInEc)
{
    public int DiscountOn(Guid tourId)
    {
        return Coupons.AppliedTo(tourId)?.DiscountInEc ?? 0;
    }

    public int GiftCardOn(Guid tourId)
    {
        return GiftCard?.AppliedPerTour.GetValueOrDefault(tourId) ?? 0;
    }
}
