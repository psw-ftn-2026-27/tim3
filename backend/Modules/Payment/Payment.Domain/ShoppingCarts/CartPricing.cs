using Payment.Domain.Coupons;
using Payment.Domain.GiftCards;

namespace Payment.Domain.ShoppingCarts;

public sealed class CartPricing
{
    private readonly CouponAllocation _allocation;

    public CartPricing(CouponAllocation allocation)
    {
        _allocation = allocation;
    }

    // P3: coupons are applied first, and the gift card then reduces the total left after them.
    public CartPrice Price(
        ShoppingCart cart,
        IReadOnlyCollection<Coupon> coupons,
        IReadOnlySet<Guid> usedCouponIds,
        GiftCard? giftCard,
        DateTime moment)
    {
        var allocation = _allocation.Allocate(cart, coupons, usedCouponIds, DateOnly.FromDateTime(moment));
        if (giftCard is null)
        {
            return new CartPrice(allocation, null, allocation.TotalInEc, allocation.TotalInEc);
        }

        var reason = NotAppliedReason(cart, giftCard, allocation.TotalInEc, moment);
        if (reason is not null)
        {
            var notApplied = new PricedGiftCard(giftCard.Id, giftCard.Amount, 0, 0, new Dictionary<Guid, int>(), reason);
            return new CartPrice(allocation, notApplied, allocation.TotalInEc, allocation.TotalInEc);
        }

        // P2: the card is spent in full, the total never drops below 0 and the rest of the card is lost.
        var applied = (int)Math.Min(giftCard.Amount, allocation.TotalInEc);
        var priced = new PricedGiftCard(
            giftCard.Id, giftCard.Amount, applied, giftCard.Amount - applied, SpreadOverTours(cart, allocation, applied), null);
        return new CartPrice(allocation, priced, allocation.TotalInEc, allocation.TotalInEc - applied);
    }

    private static GiftCardNotAppliedReason? NotAppliedReason(
        ShoppingCart cart, GiftCard giftCard, long totalAfterCoupons, DateTime moment)
    {
        if (!giftCard.CanBeUsedBy(cart.UserId, moment))
        {
            return giftCard.StatusAt(moment) switch
            {
                GiftCardStatus.Expired => GiftCardNotAppliedReason.Expired,
                GiftCardStatus.Redeemed => GiftCardNotAppliedReason.Redeemed,
                _ => GiftCardNotAppliedReason.NotRecipient
            };
        }

        // P4: a card is not applied to a cart that costs nothing after the coupons.
        return totalAfterCoupons == 0 ? GiftCardNotAppliedReason.TotalIsZero : null;
    }

    // Each tour, in the order it was added, takes at most its price after the coupon.
    private static Dictionary<Guid, int> SpreadOverTours(ShoppingCart cart, CouponAllocationResult allocation, int applied)
    {
        var perTour = new Dictionary<Guid, int>();
        var left = applied;
        foreach (var item in cart.Items.OrderBy(item => item.Sequence))
        {
            var part = Math.Min(left, item.PriceInEc - (allocation.AppliedTo(item.TourId)?.DiscountInEc ?? 0));
            if (part > 0)
            {
                perTour[item.TourId] = part;
                left -= part;
            }
        }

        return perTour;
    }
}
