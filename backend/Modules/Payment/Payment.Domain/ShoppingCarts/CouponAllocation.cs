using Payment.Domain.Coupons;

namespace Payment.Domain.ShoppingCarts;

public sealed class CouponAllocation
{
    public CouponAllocationResult Allocate(
        ShoppingCart cart, IReadOnlyCollection<Coupon> coupons, IReadOnlySet<Guid> usedCouponIds, DateOnly today)
    {
        var entered = cart.CouponIds
            .Select(id => coupons.SingleOrDefault(coupon => coupon.Id == id))
            .OfType<Coupon>()
            .ToList();
        var candidates = entered.Select(coupon => Evaluate(cart, coupon, usedCouponIds, today)).ToList();

        // P3: one coupon per item; the larger percent wins, and on equal percent the coupon entered first.
        var winners = candidates
            .Where(candidate => candidate.TourId is not null)
            .GroupBy(candidate => candidate.TourId)
            .Select(contest => contest
                .OrderByDescending(candidate => PercentOf(entered, candidate))
                .ThenBy(candidate => entered.FindIndex(coupon => coupon.Id == candidate.CouponId))
                .First())
            .ToHashSet();
        var allocated = candidates
            .Select(candidate => candidate.TourId is null || winners.Contains(candidate)
                ? candidate
                : NotApplied(candidate.CouponId, CouponNotAppliedReason.BetterCouponApplied))
            .ToList();

        var discount = allocated.Sum(coupon => (long)coupon.DiscountInEc);
        return new CouponAllocationResult(allocated, cart.TotalInEc - discount);
    }

    // P2: a product coupon targets only its product; a general coupon targets the author's most expensive paid item,
    // the earlier added one on equal price.
    internal static OrderItem? FindTargetItem(ShoppingCart cart, Coupon coupon)
    {
        if (coupon.ProductId is not null)
        {
            return cart.Items.SingleOrDefault(item => item.ProductId == coupon.ProductId);
        }

        return cart.Items
            .Where(item => item.AuthorId == coupon.AuthorId && item.PriceInEc > 0)
            .OrderByDescending(item => item.PriceInEc)
            .ThenBy(item => item.Sequence)
            .FirstOrDefault();
    }

    private static AllocatedCoupon Evaluate(
        ShoppingCart cart, Coupon coupon, IReadOnlySet<Guid> usedCouponIds, DateOnly today)
    {
        if (coupon.IsExpiredOn(today))
        {
            return NotApplied(coupon.Id, CouponNotAppliedReason.Expired);
        }

        if (usedCouponIds.Contains(coupon.Id))
        {
            return NotApplied(coupon.Id, CouponNotAppliedReason.AlreadyUsed);
        }

        var item = FindTargetItem(cart, coupon);
        if (item is null)
        {
            return NotApplied(coupon.Id, CouponNotAppliedReason.NoMatchingItem);
        }

        // P6: the discount is taken from the price in the cart and rounded down to a whole EC.
        var discount = (int)((long)item.PriceInEc * coupon.DiscountPercent / 100);
        if (discount == 0)
        {
            return NotApplied(coupon.Id, CouponNotAppliedReason.DiscountTooSmall);
        }

        return new AllocatedCoupon(coupon.Id, item.TourId, discount, null);
    }

    private static int PercentOf(List<Coupon> coupons, AllocatedCoupon candidate)
    {
        return coupons.Single(coupon => coupon.Id == candidate.CouponId).DiscountPercent;
    }

    private static AllocatedCoupon NotApplied(Guid couponId, CouponNotAppliedReason reason)
    {
        return new AllocatedCoupon(couponId, null, 0, reason);
    }
}
